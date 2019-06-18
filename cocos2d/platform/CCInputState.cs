#region File Description

//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Cocos2D
{
    public enum CCGestureDirection
    {
        Up,
        Down,
        Right,
        Left,
        None
    }

    public struct CCGesture
    {
        // attributes
        private GestureType _gestureType;
        private TimeSpan _timestamp;
        private CCPoint _position;
        private CCPoint _position2;
        private CCPoint _delta;
        private CCPoint _delta2;

        #region Properties

        /// <summary>
        /// Gets the type of the gesture.
        /// </summary>
        public GestureType GestureType
        {
            get
            {
                return this._gestureType;
            }
        }

        /// <summary>
        /// Gets the starting time for this multi-touch gesture sample.
        /// </summary>
        public TimeSpan Timestamp
        {
            get
            {
                return this._timestamp;
            }
        }

        /// <summary>
        /// Gets the position of the first touch-point in the gesture sample.
        /// </summary>
        public CCPoint Position
        {
            get
            {
                return this._position;
            }
        }

        /// <summary>
        /// Gets the position of the second touch-point in the gesture sample.
        /// </summary>
        public CCPoint Position2
        {
            get
            {
                return this._position2;
            }
        }

        /// <summary>
        /// Gets the delta information for the first touch-point in the gesture sample.
        /// </summary>
        public CCPoint Delta
        {
            get
            {
                return this._delta;
            }
        }

        /// <summary>
        /// Gets the delta information for the second touch-point in the gesture sample.
        /// </summary>
        public CCPoint Delta2
        {
            get
            {
                return this._delta2;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new <see cref="GestureSample"/>.
        /// </summary>
        /// <param name="gestureType"><see cref="GestureType"/></param>
        /// <param name="timestamp"></param>
        /// <param name="position"></param>
        /// <param name="position2"></param>
        /// <param name="delta"></param>
        /// <param name="delta2"></param>
        public CCGesture(GestureType gestureType, TimeSpan timestamp, CCPoint position, CCPoint position2, CCPoint delta, CCPoint delta2)
        {
            this._gestureType = gestureType;
            this._timestamp = timestamp;
            this._position = position;
            this._position2 = position2;
            this._delta = delta;
            this._delta2 = delta2;
        }
    }

    public static class CCExtensionMethods
    {
        public static CCGestureDirection GetDirection(this GestureSample gesture)
        {
            CCGestureDirection direction = CCGestureDirection.None;

            if (gesture.Delta.Y != 0)
            {
                direction = gesture.Delta.Y > 0 ? CCGestureDirection.Down : CCGestureDirection.Up;
            }
            else if (gesture.Delta.X != 0)
            {
                direction = gesture.Delta.X > 0 ? CCGestureDirection.Right : CCGestureDirection.Left;
            }
            return direction;
        }
    }

    /// <summary>
    ///   an enum of all available mouse buttons.
    /// </summary>
    public enum CCMouseButtons
    {
        LeftButton,
        MiddleButton,
        RightButton,
        ExtraButton1,
        ExtraButton2
    }

    /// <summary>
    /// Helper for reading input from keyboard, gamepad, and touch input. This class 
    /// tracks both the current and previous state of the input devices, and implements 
    /// query methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class CCInputState
    {
        public static readonly CCInputState Instance = new CCInputState();

        /// <summary>
        /// The value of an analog control that reads as a "pressed button".
        /// </summary>
        private const float AnalogLimit = 0.5f;

        public const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates = new KeyboardState[MaxInputs];
        public readonly KeyboardState[] LastKeyboardStates = new KeyboardState[MaxInputs];

        public readonly GamePadState[] CurrentGamePadStates = new GamePadState[MaxInputs];
        public readonly GamePadState[] LastGamePadStates = new GamePadState[MaxInputs];

        public readonly bool[] GamePadWasConnected = new bool[MaxInputs];

#if (WINDOWS && !WINRT) || WINDOWSGL || MACOS || ENABLE_MOUSE
        public MouseState CurrentMouseState;
        public MouseState LastMouseState;
#endif

        public TouchCollection TouchState;

        public readonly List<GestureSample> Gestures = new List<GestureSample>();

        /// <summary>
        /// Initialization
        /// </summary>
        private CCInputState()
        {
            ConsumeGamePadState = true;
        }

        #region Update

        public bool ConsumeGamePadState { get; set; }

        /// <summary>
        /// Reads the latest state user input.
        /// </summary>
        public void Update(float deltaTime)
        {
#if (WINDOWS && !WINRT) || WINDOWSGL || MACOS || ENABLE_MOUSE
            LastMouseState = CurrentMouseState;
            CurrentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
#endif

            if (CCDirector.SharedDirector.GamePadEnabled && ConsumeGamePadState)
            {
                try
                {
                    for (int i = 0; i < MaxInputs; i++)
                    {
                        LastKeyboardStates[i] = CurrentKeyboardStates[i];
                        LastGamePadStates[i] = CurrentGamePadStates[i];

                        CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                        CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                        // Keep track of whether a gamepad has ever been
                        // connected, so we can detect if it is unplugged.
                        if (CurrentGamePadStates[i].IsConnected)
                        {
                            GamePadWasConnected[i] = true;
                        }
                    }
                }
                catch (DllNotFoundException)
                {
                    // No gamepad support, so disable it.
                    ConsumeGamePadState = false;
                }
            }
            // Get the raw touch state from the TouchPanel
            TouchState = TouchPanel.GetState();

            if (TouchPanel.EnabledGestures != GestureType.None)
            {
                // Read in any detected gestures into our list for the screens to later process
                Gestures.Clear();
                while (TouchPanel.IsGestureAvailable)
                {
                    Gestures.Add(TouchPanel.ReadGesture());
                }
            }
        }

        #endregion

        /// <summary>
        /// Helper for checking if a key was pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsKeyPressed(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                var i = (int) playerIndex;

                return CurrentKeyboardStates[i].IsKeyDown(key);
            }

            // Accept input from any player.
            return (IsKeyPressed(key, PlayerIndex.One, out playerIndex) ||
                    IsKeyPressed(key, PlayerIndex.Two, out playerIndex) ||
                    IsKeyPressed(key, PlayerIndex.Three, out playerIndex) ||
                    IsKeyPressed(key, PlayerIndex.Four, out playerIndex));
        }


        /// <summary>
        /// Helper for checking if a button was pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsButtonPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                var i = (int) playerIndex;

                return CurrentGamePadStates[i].IsButtonDown(button);
            }

            // Accept input from any player.
            return (IsButtonPressed(button, PlayerIndex.One, out playerIndex) ||
                    IsButtonPressed(button, PlayerIndex.Two, out playerIndex) ||
                    IsButtonPressed(button, PlayerIndex.Three, out playerIndex) ||
                    IsButtonPressed(button, PlayerIndex.Four, out playerIndex));
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                var i = (int) playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) && LastKeyboardStates[i].IsKeyUp(key));
            }

            // Accept input from any player.
            return (IsKeyPress(key, PlayerIndex.One, out playerIndex) ||
                    IsKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                    IsKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                    IsKeyPress(key, PlayerIndex.Four, out playerIndex));
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                var i = (int) playerIndex;

                return (CurrentGamePadStates[i].IsButtonDown(button) && LastGamePadStates[i].IsButtonUp(button));
            }

            // Accept input from any player.
            return (IsButtonPress(button, PlayerIndex.One, out playerIndex) ||
                    IsButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                    IsButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                    IsButtonPress(button, PlayerIndex.Four, out playerIndex));
        }

        #region Mouse

#if (WINDOWS && !WINRT) || WINDOWSGL || MACOS || ENABLE_MOUSE

        public MouseState Mouse
        {
            get { return CurrentMouseState; }
        }

        /// <summary>
        ///   The current mouse position.
        /// </summary>
        public Point MousePosition
        {
            get { return new Point(CurrentMouseState.X, CurrentMouseState.Y); }
        }

        /// <summary>
        ///   The current mouse velocity.
        ///   Expressed as: 
        ///   current mouse position - last mouse position.
        /// </summary>
        public Vector2 MouseVelocity
        {
            get
            {
                return (
                           new Vector2(CurrentMouseState.X, CurrentMouseState.Y) -
                           new Vector2(LastMouseState.X, LastMouseState.Y)
                       );
            }
        }

        /// <summary>
        ///   The current mouse scroll wheel position.
        ///   See the Mouse's ScrollWheel property for details.
        /// </summary>
        public float MouseScrollWheelPosition
        {
            get { return CurrentMouseState.ScrollWheelValue; }
        }

        /// <summary>
        ///   The mouse scroll wheel velocity.
        ///   
        ///   Expressed as:
        ///   current scroll wheel position - the last scroll wheel position.
        /// </summary>
        public float MouseScrollWheelVelocity
        {
            get { return (CurrentMouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue); }
        }

        /// <summary>
        ///   Checks if the requested mosue button is a new press.
        /// </summary>
        /// <param name = "button">
        ///   The mouse button to check.
        /// </param>
        /// <returns>
        ///   A bool indicating whether the selected mouse button is being
        ///   pressed in the current state but not in the last state.
        /// </returns>
        public bool IsMouseButtonPress(CCMouseButtons button)
        {
            switch (button)
            {
                case CCMouseButtons.LeftButton:
                    return (
                               LastMouseState.LeftButton == ButtonState.Released &&
                               CurrentMouseState.LeftButton == ButtonState.Pressed);
                case CCMouseButtons.MiddleButton:
                    return (
                               LastMouseState.MiddleButton == ButtonState.Released &&
                               CurrentMouseState.MiddleButton == ButtonState.Pressed);
                case CCMouseButtons.RightButton:
                    return (
                               LastMouseState.RightButton == ButtonState.Released &&
                               CurrentMouseState.RightButton == ButtonState.Pressed);
                case CCMouseButtons.ExtraButton1:
                    return (
                               LastMouseState.XButton1 == ButtonState.Released &&
                               CurrentMouseState.XButton1 == ButtonState.Pressed);
                case CCMouseButtons.ExtraButton2:
                    return (
                               LastMouseState.XButton2 == ButtonState.Released &&
                               CurrentMouseState.XButton2 == ButtonState.Pressed);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the requested mosue button is an old press.
        /// </summary>
        /// <param name="button">
        /// The mouse button to check.
        /// </param>
        /// <returns>
        /// A bool indicating whether the selected mouse button is not being 
        /// pressed in the current state and is being pressed in the old state.
        /// </returns>
        public bool IsMouseButtonRelease(CCMouseButtons button)
        {
            switch (button)
            {
                case CCMouseButtons.LeftButton:
                    return (
                               LastMouseState.LeftButton == ButtonState.Pressed &&
                               CurrentMouseState.LeftButton == ButtonState.Released);
                case CCMouseButtons.MiddleButton:
                    return (
                               LastMouseState.MiddleButton == ButtonState.Pressed &&
                               CurrentMouseState.MiddleButton == ButtonState.Released);
                case CCMouseButtons.RightButton:
                    return (
                               LastMouseState.RightButton == ButtonState.Pressed &&
                               CurrentMouseState.RightButton == ButtonState.Released);
                case CCMouseButtons.ExtraButton1:
                    return (
                               LastMouseState.XButton1 == ButtonState.Pressed &&
                               CurrentMouseState.XButton1 == ButtonState.Released);
                case CCMouseButtons.ExtraButton2:
                    return (
                               LastMouseState.XButton2 == ButtonState.Pressed &&
                               CurrentMouseState.XButton2 == ButtonState.Released);
                default:
                    return false;
            }
        }

        public bool IsMouseButtonDown(CCMouseButtons button)
        {
            switch (button)
            {
                case CCMouseButtons.LeftButton:
                    return CurrentMouseState.LeftButton == ButtonState.Pressed;
                case CCMouseButtons.RightButton:
                    return CurrentMouseState.RightButton == ButtonState.Pressed;
                case CCMouseButtons.MiddleButton:
                    return CurrentMouseState.MiddleButton == ButtonState.Pressed;
                case CCMouseButtons.ExtraButton1:
                    return CurrentMouseState.XButton1 == ButtonState.Pressed;
                case CCMouseButtons.ExtraButton2:
                    return CurrentMouseState.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public bool IsMouseButtonUp(CCMouseButtons button)
        {
            switch (button)
            {
                case CCMouseButtons.LeftButton:
                    return CurrentMouseState.LeftButton == ButtonState.Released;
                case CCMouseButtons.RightButton:
                    return CurrentMouseState.RightButton == ButtonState.Released;
                case CCMouseButtons.MiddleButton:
                    return CurrentMouseState.MiddleButton == ButtonState.Released;
                case CCMouseButtons.ExtraButton1:
                    return CurrentMouseState.XButton1 == ButtonState.Released;
                case CCMouseButtons.ExtraButton2:
                    return CurrentMouseState.XButton2 == ButtonState.Released;
                default:
                    return false;
            }
        }
#endif

        #endregion

        public bool IsBackPress(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            return IsKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
        }
    }
}