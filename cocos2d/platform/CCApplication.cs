using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Cocos2D
{

    public abstract class CCApplication : DrawableGameComponent
    {
        protected static CCApplication s_pSharedApplication;
        private readonly List<CCTouch> endedTouches = new List<CCTouch>();
        private readonly Dictionary<int, LinkedListNode<CCTouch>> m_pTouchMap = new Dictionary<int, LinkedListNode<CCTouch>>();
        private readonly LinkedList<CCTouch> m_pTouches = new LinkedList<CCTouch>();
        private readonly List<CCTouch> movedTouches = new List<CCTouch>();
        private readonly List<CCTouch> newTouches = new List<CCTouch>();
#if WINDOWS || WINDOWSGL || MACOS || WINDOWSGL
        private int _lastMouseId;
        private MouseState _lastMouseState;
        private MouseState _prevMouseState;
#endif
        protected bool m_bCaptured;
        protected ICCEGLTouchDelegate m_pDelegate;

        public GameTime GameTime;

        private bool _initialized;

        public CCApplication(Game game, IGraphicsDeviceService service)
            : base(game)
        {
            if (Game.Services.GetService(typeof(IGraphicsDeviceService)) == null)
            {
                Game.Services.AddService(typeof(IGraphicsDeviceService), service);
            }

            CCDrawManager.Init(service);

            Content = game.Content;
            HandleMediaStateAutomatically = true;

            game.IsFixedTimeStep = true;

            TouchPanel.EnabledGestures = GestureType.Tap;

            game.Activated += GameActivated;
            game.Deactivated += GameDeactivated;
            game.Exiting += GameExiting;

            //TODO: Move to CCContentManager
#if IOS || WINDOWS_PHONE8
            // Please read the following discussions for the reasons of this.
            // http://monogame.codeplex.com/discussions/393775
            // http://monogame.codeplex.com/discussions/396792
            // 
            // https://github.com/mono/MonoGame/pull/726
            //
            // Also search Google for -> ContentTypeReaderManager.AddTypeCreator

            // .FNT Reader
            ContentTypeReaderManager.AddTypeCreator (
                "Microsoft.Xna.Framework.Content.DictionaryReader`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[Cocos2D.CCBMFontConfiguration+CCBMFontDef, cocos2d-xna, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null]]",
                ( ) => new DictionaryReader<Int32, CCBMFontConfiguration.CCBMFontDef> ()

                );

            ContentTypeReaderManager.AddTypeCreator (
                "Microsoft.Xna.Framework.Content.DictionaryReader`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[Cocos2D.CCBMFontConfiguration+CCKerningHashElement, cocos2d-xna, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null]]",
                ( ) => new DictionaryReader<Int32, CCBMFontConfiguration.CCKerningHashElement> ()
                
                );
            ContentTypeReaderManager.AddTypeCreator (
                "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[Cocos2D.CCRect, cocos2d-xna, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null]]",
                ( ) => new CCRectReader ()

                );

            ContentTypeReaderManager.AddTypeCreator (
                "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[Cocos2D.CCPoint, cocos2d-xna, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null]]",
                ( ) => new CCPointReader ()
                
                );
            ContentTypeReaderManager.AddTypeCreator (
                "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[Cocos2D.CCSize, cocos2d-xna, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null]]",
                ( ) => new CCSizeReader ()
                
                );

            ContentTypeReaderManager.AddTypeCreator (
                "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[Cocos2D.CCBMFontConfiguration+CCKerningHashElement, cocos2d-xna, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null]]",
                ( ) => new KerningHashElementReader ()
                
                );

            ContentTypeReaderManager.AddTypeCreator (
                "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[Cocos2D.CCBMFontConfiguration+CCBMFontPadding, cocos2d-xna, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null]]",
                ( ) => new CCBMFontPaddingtReader ()
                
                );
#endif
        }

        protected bool HandleMediaStateAutomatically { get; set; }

        private void GameActivated(object sender, EventArgs e)
        {
            // Clear out the prior gamepad state because we don't want it anymore.
            m_PriorGamePadState.Clear();
#if !IOS
            if (HandleMediaStateAutomatically)
            {
                CocosDenshion.CCSimpleAudioEngine.SharedEngine.SaveMediaState();
            }
#endif
            ApplicationWillEnterForeground();
        }

        private void GameDeactivated(object sender, EventArgs e)
        {
            ApplicationDidEnterBackground();
#if !IOS
            if (HandleMediaStateAutomatically)
            {
                CocosDenshion.CCSimpleAudioEngine.SharedEngine.RestoreMediaState();
            }
#endif
        }

        void GameExiting(object sender, EventArgs e)
        {
            CCDirector.SharedDirector.End();
        }

        /// <summary>
        /// Callback by CCDirector for limit FPS
        /// </summary>
        /// <param name="interval">The time, which expressed in seconds, between current frame and next. </param>
        public virtual double AnimationInterval
        {
            get { return Game.TargetElapsedTime.Milliseconds / 10000000f; }
            set { Game.TargetElapsedTime = TimeSpan.FromTicks((int) (value * 10000000)); }
        }

        public ICCEGLTouchDelegate TouchDelegate
        {
            set { m_pDelegate = value; }
        }

        [Obsolete("use CCContentManager.SharedContentManager")]
        public ContentManager Content { get; private set; }

        public void ClearTouches()
        {
            m_pTouches.Clear();
            m_pTouchMap.Clear();
        }

        /// <summary>
        /// Loads the content for the game and then calls ApplicationDidFinishLaunching.
        /// </summary>
        protected override void LoadContent()
        {
            if (!_initialized)
            {
                CCContentManager.Initialize(Game.Content.ServiceProvider, Game.Content.RootDirectory);

                base.LoadContent();

                ApplicationDidFinishLaunching();

                _initialized = true;
            }
            else
            {
                base.LoadContent();
            }
        }

        public override void Initialize()
        {
            s_pSharedApplication = this;

            InitInstance();

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            GameTime = gameTime;

#if !PSM &&!NETFX_CORE
            if (CCDirector.SharedDirector.Accelerometer != null)
            {
                CCDirector.SharedDirector.Accelerometer.Update();
            }
#endif
            // Process touch events 
            ProcessTouch();

            if (CCDirector.SharedDirector.GamePadEnabled)
            {
                // Process the game pad
                // This consumes game pad state.
                ProcessGamePad();
            }

            ProcessKeyboard();

            CCDirector.SharedDirector.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameTime = gameTime;

            CCDrawManager.BeginDraw();

            CCDirector.SharedDirector.MainLoop(gameTime);

            base.Draw(gameTime);

            CCDrawManager.EndDraw();
        }

        protected virtual void HandleGesture(GestureSample gesture)
        {
            //TODO: Create CCGesture and convert the coordinates into the local coordinates.
        }

        #region GamePad Support
        public event CCGamePadButtonDelegate GamePadButtonUpdate;
        public event CCGamePadDPadDelegate GamePadDPadUpdate;
        public event CCGamePadStickUpdateDelegate GamePadStickUpdate;
        public event CCGamePadTriggerDelegate GamePadTriggerUpdate;
        public event CCGamePadConnectionDelegate GamePadConnectionUpdate;

        private Dictionary<PlayerIndex, GamePadState> m_PriorGamePadState = new Dictionary<PlayerIndex, GamePadState>();

        private void ProcessGamePad(GamePadState gps, PlayerIndex player)
            {
            GamePadState lastState = new GamePadState();
            if (m_PriorGamePadState.ContainsKey(player))
                {
                lastState = m_PriorGamePadState[player];
                // Notify listeners when the gamepad is connected.
                if ((lastState.IsConnected != gps.IsConnected) && GamePadConnectionUpdate != null)
                {
                    GamePadConnectionUpdate(player, false);
                }
                // TODO: Check button pressed/released status for button tap events.
            }
            if (gps.IsConnected)
            {
                GamePadCapabilities caps = GamePad.GetCapabilities(player);
                if (GamePadButtonUpdate != null)
                {
                    CCGamePadButtonStatus back = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus start = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus system = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus a = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus b = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus x = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus y = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus leftShoulder = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus rightShoulder = CCGamePadButtonStatus.NotApplicable;
                    if (caps.HasBackButton)
                    {
                        back = (gps.Buttons.Back == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                }
                    if (caps.HasStartButton)
                    {
                        start = (gps.Buttons.Start == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
            }
                    if (caps.HasBigButton)
            {
                        system = (gps.Buttons.BigButton == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    if (caps.HasAButton)
                {
                        a = (gps.Buttons.A == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                }
                    if (caps.HasBButton)
                    {
                        b = (gps.Buttons.B == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
            }
                    if (caps.HasXButton)
            {
                        x = (gps.Buttons.X == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    if (caps.HasYButton)
                {
                        y = (gps.Buttons.Y == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    if (caps.HasLeftShoulderButton)
                    {
                        leftShoulder = (gps.Buttons.LeftShoulder == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    if (caps.HasRightShoulderButton)
                    {
                        rightShoulder = (gps.Buttons.RightShoulder == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    GamePadButtonUpdate(back, start, system, a, b, x, y, leftShoulder, rightShoulder, player);
                }
                // Process the game sticks
                if (GamePadStickUpdate != null && (caps.HasLeftXThumbStick || caps.HasLeftYThumbStick || caps.HasRightXThumbStick || caps.HasRightYThumbStick ||  caps.HasLeftStickButton || caps.HasRightStickButton))
                {
                    CCPoint vecLeft;
                    if (caps.HasLeftXThumbStick || caps.HasLeftYThumbStick)
                    {
                        vecLeft = new CCPoint(gps.ThumbSticks.Left);
                        vecLeft.Normalize();
                    }
                    else
                    {
                        vecLeft = CCPoint.Zero;
                    }
                    CCPoint vecRight;
                    if (caps.HasRightXThumbStick || caps.HasRightYThumbStick)
                    {
                        vecRight = new CCPoint(gps.ThumbSticks.Right);
                        vecRight.Normalize();
                    }
                    else
                    {
                        vecRight = CCPoint.Zero;
                    }
                    CCGameStickStatus left = new CCGameStickStatus();
                    left.Direction = vecLeft;
                    left.Magnitude = ((caps.HasLeftXThumbStick || caps.HasLeftYThumbStick) ? gps.ThumbSticks.Left.Length() : 0f);
                    left.IsDown = ((caps.HasLeftStickButton) ? gps.IsButtonDown(Buttons.LeftStick) : false);
                    CCGameStickStatus right = new CCGameStickStatus();
                    right.Direction = vecRight;
                    right.Magnitude = ((caps.HasRightXThumbStick || caps.HasRightYThumbStick) ? gps.ThumbSticks.Right.Length() : 0f);
                    right.IsDown = ((caps.HasLeftStickButton) ? gps.IsButtonDown(Buttons.RightStick) : false);
                    GamePadStickUpdate(left, right, player);
                }
                // Process the game triggers
                if (GamePadTriggerUpdate != null && (caps.HasLeftTrigger || caps.HasRightTrigger))
                {
                    GamePadTriggerUpdate(caps.HasLeftTrigger ? gps.Triggers.Left : 0f, caps.HasRightTrigger ? gps.Triggers.Right : 0f, player);
                }
                // Process the D-Pad
                if (GamePadDPadUpdate != null)
                {
                    CCGamePadButtonStatus left = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus right = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus up = CCGamePadButtonStatus.NotApplicable;
                    CCGamePadButtonStatus down = CCGamePadButtonStatus.NotApplicable;
                    if (caps.HasDPadDownButton)
                    {
                        down = (gps.DPad.Down == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    if (caps.HasDPadUpButton)
                    {
                        up = (gps.DPad.Up == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    if (caps.HasDPadLeftButton)
                    {
                        left = (gps.DPad.Left == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    if (caps.HasDPadRightButton)
                    {
                        right = (gps.DPad.Right == ButtonState.Pressed ? CCGamePadButtonStatus.Pressed : CCGamePadButtonStatus.Released);
                    }
                    GamePadDPadUpdate(left, up, right, down, player);
                }
            }
            m_PriorGamePadState[player] = gps;
        }

        private void ProcessGamePad()
        {
            // On Android, the gamepad is always connected.
            GamePadState gps1 = GamePad.GetState(PlayerIndex.One);
            GamePadState gps2 = GamePad.GetState(PlayerIndex.Two);
            GamePadState gps3 = GamePad.GetState(PlayerIndex.Three);
            GamePadState gps4 = GamePad.GetState(PlayerIndex.Four);
            ProcessGamePad(gps1, PlayerIndex.One);
            ProcessGamePad(gps2, PlayerIndex.Two);
            ProcessGamePad(gps3, PlayerIndex.Three);
            ProcessGamePad(gps4, PlayerIndex.Four);
        }
        #endregion

        #region Keyboard support
        private KeyboardState m_priorKeyboardState;

        private void ProcessKeyboard()
        {
			// Read the current keyboard state
            KeyboardState currentKeyState = Keyboard.GetState();

			// Check for Keypad interaction
            if(currentKeyState.IsKeyUp(Keys.Back) && m_priorKeyboardState.IsKeyDown(Keys.Back)) 
            {
                CCDirector.SharedDirector.KeypadDispatcher.DispatchKeypadMsg(CCKeypadMSGType.BackClicked);
            }
            else if(currentKeyState.IsKeyUp(Keys.Home) && m_priorKeyboardState.IsKeyDown(Keys.Home)) 
            {
                CCDirector.SharedDirector.KeypadDispatcher.DispatchKeypadMsg(CCKeypadMSGType.MenuClicked);
            }

			CCDirector.SharedDirector.KeyboardDispatcher.DispatchKeyboardState ();

			// Store the state for the next loop
            m_priorKeyboardState = currentKeyState;

        }
        #endregion

        private CCPoint TransformPoint(float x, float y) {
            CCPoint newPoint;
            newPoint.X = x * TouchPanel.DisplayWidth / Game.Window.ClientBounds.Width;
            newPoint.Y = y * TouchPanel.DisplayHeight / Game.Window.ClientBounds.Height;
            return newPoint;
        }

        private void ProcessTouch()
        {
            if (m_pDelegate != null)
            {
                newTouches.Clear();
                movedTouches.Clear();
                endedTouches.Clear();

                CCRect viewPort = CCDrawManager.ViewPortRect;
                CCPoint pos;

                // TODO: allow configuration to treat the game pad as a touch device.

#if WINDOWS || WINDOWSGL || MACOS
                _prevMouseState = _lastMouseState;
                _lastMouseState = Mouse.GetState();

                if (_prevMouseState.LeftButton == ButtonState.Released && _lastMouseState.LeftButton == ButtonState.Pressed)
                {
#if NETFX_CORE
                    pos = TransformPoint(_lastMouseState.X, _lastMouseState.Y);
                    pos = CCDrawManager.ScreenToWorld(pos.X, pos.Y);
#else
                    pos = CCDrawManager.ScreenToWorld(_lastMouseState.X, _lastMouseState.Y);
#endif
                    _lastMouseId++;
                    m_pTouches.AddLast(new CCTouch(_lastMouseId, pos.X, pos.Y));
                    m_pTouchMap.Add(_lastMouseId, m_pTouches.Last);
                    newTouches.Add(m_pTouches.Last.Value);

                    m_bCaptured = true;
                }
                else if (_prevMouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (m_pTouchMap.ContainsKey(_lastMouseId))
                    {
                        if (_prevMouseState.X != _lastMouseState.X || _prevMouseState.Y != _lastMouseState.Y)
                        {
#if NETFX_CORE
                            pos = TransformPoint(_lastMouseState.X, _lastMouseState.Y);
                            pos = CCDrawManager.ScreenToWorld(pos.X, pos.Y);
#else
                            pos = CCDrawManager.ScreenToWorld(_lastMouseState.X, _lastMouseState.Y);
#endif
                            movedTouches.Add(m_pTouchMap[_lastMouseId].Value);
                            m_pTouchMap[_lastMouseId].Value.SetTouchInfo(_lastMouseId, pos.X, pos.Y);
                        }
                    }
                }
                else if (_prevMouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Released)
                {
                    if (m_pTouchMap.ContainsKey(_lastMouseId))
                    {
                        endedTouches.Add(m_pTouchMap[_lastMouseId].Value);
                        m_pTouches.Remove(m_pTouchMap[_lastMouseId]);
                        m_pTouchMap.Remove(_lastMouseId);
                    }
                }
#endif

                TouchCollection touchCollection = TouchPanel.GetState();

                foreach (TouchLocation touch in touchCollection)
                {
                    switch (touch.State)
                    {
                        case TouchLocationState.Pressed:
                            if (m_pTouchMap.ContainsKey(touch.Id))
                            {
                                break;
                            }

                            if (viewPort.ContainsPoint(touch.Position.X, touch.Position.Y))
                            {
                                pos = CCDrawManager.ScreenToWorld(touch.Position.X, touch.Position.Y);

                                m_pTouches.AddLast(new CCTouch(touch.Id, pos.X, pos.Y));
                                m_pTouchMap.Add(touch.Id, m_pTouches.Last);
                                newTouches.Add(m_pTouches.Last.Value);
                            }
                            break;

                        case TouchLocationState.Moved:
                            LinkedListNode<CCTouch> existingTouch;
                            if (m_pTouchMap.TryGetValue(touch.Id, out existingTouch))
                            {
                                pos = CCDrawManager.ScreenToWorld(touch.Position.X, touch.Position.Y);
                                var delta = existingTouch.Value.LocationInView - pos;
                                if (delta.LengthSQ > 1.0f)
                                {
                                    movedTouches.Add(existingTouch.Value);
                                    existingTouch.Value.SetTouchInfo(touch.Id, pos.X, pos.Y);
                                }
                            }
                            break;

                        case TouchLocationState.Released:
                            if (m_pTouchMap.TryGetValue(touch.Id, out existingTouch))
                            {
                                endedTouches.Add(existingTouch.Value);
                                m_pTouches.Remove(existingTouch);
                                m_pTouchMap.Remove(touch.Id);
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (newTouches.Count > 0)
                {
                    m_pDelegate.TouchesBegan(newTouches);
                }

                if (movedTouches.Count > 0)
                {
                    m_pDelegate.TouchesMoved(movedTouches);
                }

                if (endedTouches.Count > 0)
                {
                    m_pDelegate.TouchesEnded(endedTouches);
                }
            }
        }

        private CCTouch GetTouchBasedOnId(int nID)
        {
            if (m_pTouchMap.ContainsKey(nID))
            {
                LinkedListNode<CCTouch> curTouch = m_pTouchMap[nID];
                //If ID's match...
                if (curTouch.Value.Id == nID)
                {
                    //return the corresponding touch
                    return curTouch.Value;
                }
            }
            //If we reached here, we found no touches
            //matching the specified id.
            return null;
        }

        // sharedApplication pointer

        /// <summary>
        /// Get current applicaiton instance.
        /// </summary>
        /// <value> Current application instance pointer. </value>
        public static CCApplication SharedApplication
        {
            get { return s_pSharedApplication; }
        }

        /// <summary>
        /// Implement for initialize OpenGL instance, set source path, etc...
        /// </summary>
        public virtual bool InitInstance()
        {
            return true;
        }

        /// <summary>
        /// Implement CCDirector and CCScene init code here.
        /// </summary>
        /// <returns>
        ///     return true    Initialize success, app continue.
        ///     return false   Initialize failed, app terminate.
        /// </returns>
        public virtual bool ApplicationDidFinishLaunching()
        {
            return false;
        }

        /// <summary>
        /// Called when the game enters the background. This happens when the 'windows' button is pressed
        /// on a WP phone. On Android, it happens when the device is ide or the power button is pressed.
        /// </summary>
        public virtual void ApplicationDidEnterBackground()
        {
        }

        /// <summary>
        /// Called when the game returns to the foreground, such as when the game is launched after
        /// being paused.
        /// </summary>
        public virtual void ApplicationWillEnterForeground()
        {
        }
    }
}