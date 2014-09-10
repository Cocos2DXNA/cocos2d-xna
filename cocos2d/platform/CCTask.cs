using System;
#if WINDOWS_PHONE|| XBOX360
using System.ComponentModel;
#else
using System.Threading.Tasks;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
#if IOS
using MonoTouch.Foundation;
using MonoTouch.OpenGLES;
#if ES11
using OpenTK.Graphics.ES11;
#else
using OpenTK.Graphics.ES20;
#endif
#elif WINDOWS || LINUX || ANGLE
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK;
using OpenTK.Graphics.OpenGL;
#endif
#if WINDOWS_PHONE
using System.Windows;
#endif

namespace Cocos2D
{
    public static class CCTask
    {

        #region Threading

        public const int kMaxWaitForUIThread = 750; // In milliseconds
#if !WINDOWS_PHONE
        static int mainThreadId;
#endif

#if ANDROID
        static List<Action> actions = new List<Action>();
        //static Mutex actionsMutex = new Mutex();
#elif IOS
        public static EAGLContext BackgroundContext;
#elif WINDOWS || LINUX || ANGLE
        public static IGraphicsContext BackgroundContext;
        public static IWindowInfo WindowInfo;
#endif

#if !WINDOWS_PHONE
        static CCTask()
        {
#if WINDOWS_STOREAPP
            mainThreadId = Environment.CurrentManagedThreadId;
#else
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
#endif
        }
#endif

            /// <summary>
            /// Checks if the code is currently running on the UI thread.
            /// </summary>
            /// <returns>true if the code is currently running on the UI thread.</returns>
            public static bool IsOnUIThread()
            {
#if WINDOWS_PHONE
                return Deployment.Current.Dispatcher.CheckAccess();
#elif WINDOWS_STOREAPP
            return (mainThreadId == Environment.CurrentManagedThreadId);
#else
            return mainThreadId == Thread.CurrentThread.ManagedThreadId;
#endif
            }

            /// <summary>
            /// Throws an exception if the code is not currently running on the UI thread.
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown if the code is not currently running on the UI thread.</exception>
            public static void EnsureUIThread()
            {
                if (!IsOnUIThread())
                    throw new InvalidOperationException("Operation not called on UI thread.");
            }

#if WINDOWS_PHONE

            public static void RunOnUiThread(Action action)
            {
                RunOnContainerThread(Deployment.Current.Dispatcher, action);
            }

            public static void RunOnContainerThread(System.Windows.Threading.Dispatcher target, Action action)
            {
                target.BeginInvoke(action);
            }

            public static void BlockOnContainerThread(System.Windows.Threading.Dispatcher target, Action action)
            {
                if (target.CheckAccess())
                {
                    action();
                }
                else
                {
                    EventWaitHandle wait = new AutoResetEvent(false);
                    target.BeginInvoke(() =>
                    {
                        action();
                        wait.Set();
                    });
                    if (!wait.WaitOne(kMaxWaitForUIThread))
                    {
#if DEBUG
                        Debug.WriteLine("ERROR! action will not invoke because the current dispatcher is not UI-thread compatible.");
#endif
                    }
                }
            }
#endif

            /// <summary>
            /// Runs the given action on the UI thread and blocks the current thread while the action is running.
            /// If the current thread is the UI thread, the action will run immediately.
            /// </summary>
            /// <param name="action">The action to be run on the UI thread</param>
            public static void BlockOnUIThread(Action action)
            {
                if (action == null)
                    throw new ArgumentNullException("action");

#if (DIRECTX && !WINDOWS_PHONE) || PSM
            action();
#else
                // If we are already on the UI thread, just call the action and be done with it
                if (IsOnUIThread())
                {
#if WINDOWS_PHONE
                    try
                    {
                        action();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Need to be on a different thread
                        BlockOnContainerThread(Deployment.Current.Dispatcher, action);
                    }
#else
                action();
#endif
                    return;
                }

#if IOS
            lock (BackgroundContext)
            {
                // Make the context current on this thread if it is not already
                if (!Object.ReferenceEquals(EAGLContext.CurrentContext, BackgroundContext))
                    EAGLContext.SetCurrentContext(BackgroundContext);
                // Execute the action
                action();
                // Must flush the GL calls so the GPU asset is ready for the main context to use it
                GL.Flush();
                GraphicsExtensions.CheckGLError();
            }
#elif WINDOWS || LINUX || ANGLE
            lock (BackgroundContext)
            {
                // Make the context current on this thread
                BackgroundContext.MakeCurrent(WindowInfo);
                // Execute the action
                action();
                // Must flush the GL calls so the texture is ready for the main context to use
                GL.Flush();
                GraphicsExtensions.CheckGLError();
                // Must make the context not current on this thread or the next thread will get error 170 from the MakeCurrent call
                BackgroundContext.MakeCurrent(null);
            }
#elif WINDOWS_PHONE
                BlockOnContainerThread(Deployment.Current.Dispatcher, action);
#else
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
#if MONOMAC
            MonoMac.AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
#else
            Add(() =>
#endif
            {
#if ANDROID
                //if (!Game.Instance.Window.GraphicsContext.IsCurrent)
                ((AndroidGameWindow)Game.Instance.Window).GameView.MakeCurrent();
#endif
                action();
                resetEvent.Set();
            });
            resetEvent.Wait();
#endif
#endif
            }

#if ANDROID
        static void Add(Action action)
        {
            lock (actions)
            {
                actions.Add(action);
            }
        }

        /// <summary>
        /// Runs all pending actions.  Must be called from the UI thread.
        /// </summary>
        public static void Run()
        {
            EnsureUIThread();

            lock (actions)
            {
                foreach (Action action in actions)
                {
                    action();
                }
                actions.Clear();
            }
        }
#endif

        #endregion

        private class TaskSelector : ICCSelectorProtocol
        {
            public void Update(float dt)
            {
            }
        }

        private static ICCSelectorProtocol _taskSelector = new TaskSelector();

        public static void RunOnScheduler(Action action)
        {
            var scheduler = CCDirector.SharedDirector.Scheduler;
            scheduler.ScheduleSelector(f => action(), _taskSelector, 0, 0, 0, false);
        }

        public static object RunAsync(Action action)
        {
            return RunAsync(action, null);
        }
		
        public static object RunAsync(Action action, Action<object> taskCompleted)
        {
#if WINDOWS_PHONE || XBOX360
            var worker = new BackgroundWorker();
            
            worker.DoWork +=
                (sender, args) =>
                {
                    action();
                };

            if (taskCompleted != null)
            {
                worker.RunWorkerCompleted +=
                    (sender, args) =>
                    {
                        var scheduler = CCDirector.SharedDirector.Scheduler;
                        scheduler.ScheduleSelector(f => taskCompleted(worker), _taskSelector, 0, 0, 0, false);
                    };
            }

            worker.RunWorkerAsync();

            return worker;
#else
            var task = new Task(
                () =>
                {
                    action();

                    if (taskCompleted != null)
                    {
                        var scheduler = CCDirector.SharedDirector.Scheduler;
                        scheduler.ScheduleSelector(f => taskCompleted(null), _taskSelector, 0, 0, 0, false);
                    }
                }
                );
                    
            task.Start();

            return task;
#endif
        }
    }
}
