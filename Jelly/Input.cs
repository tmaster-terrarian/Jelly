using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Jelly.Graphics;

namespace Jelly;

public static class Input
{
    private static KeyboardState currentKeyboardState;
    private static KeyboardState previousKeyboardState;

    private static readonly List<char> _textInput = [];
    private const double InitialDelay = 0.45; // Initial delay before repeating input
    private const double RepeatRate = 0.045;  // Rate of repeated input
    private static readonly double[] _charsDelay = new double[255];
    private static Keys[] _lastPressedKeys = [];

    public static bool InputDisabled { get; set; }

    public static KeyboardState KeyboardState => currentKeyboardState;

    public static Point MousePosition => new(
        Mouse.GetState().X / Renderer.PixelScale,
        Mouse.GetState().Y / Renderer.PixelScale
    );

    public static Point MousePositionClamped => MousePosition.Clamp(Point.Zero, new(Renderer.ScreenSize.X - 1, Renderer.ScreenSize.Y - 1));

    public static Point GetMousePositionWithZoom(float zoom, bool clamp) => clamp
        ? new Point(
            (int)MathHelper.Clamp(Mouse.GetState().X / Renderer.PixelScale / zoom, 0, Renderer.ScreenSize.X / zoom),
            (int)MathHelper.Clamp(Mouse.GetState().Y / Renderer.PixelScale / zoom, 0, Renderer.ScreenSize.Y / zoom)
        )
        : new Point(
            (int)(Mouse.GetState().X / (Renderer.PixelScale * zoom)),
            (int)(Mouse.GetState().Y / (Renderer.PixelScale * zoom))
        );

    public static KeyboardState RefreshKeyboardState()
    {
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();

        return currentKeyboardState;
    }

    public static void UpdateTypingInput(GameTime gameTime)
    {
        var keysPressed = currentKeyboardState.GetPressedKeys();

        HashSet<Keys> _lastKeys = new(_lastPressedKeys);
        _textInput.Clear();
        var currSeconds = gameTime.TotalGameTime.TotalSeconds;

        foreach(var key in keysPressed)
        {
            char keyChar = ConvertKeyToChar(key, currentKeyboardState);
            int keyCharForDelay = key == Keys.Back ? 127 : keyChar.ToString().ToLower()[0];
            if(keyChar != '\0')
            {
                if ((currSeconds > _charsDelay[keyCharForDelay]) || (!_lastKeys.Contains(key)))
                {
                    _textInput.Add(keyChar);
                    _charsDelay[keyCharForDelay] = currSeconds + (_lastKeys.Contains(key) ? RepeatRate : InitialDelay);
                }
            }
        }
        _lastPressedKeys = keysPressed;
    }

    public static char[] GetTextInput()
    {
        return [.. _textInput];
    }

    private static MouseState currentMouseState;
    private static MouseState previousMouseState;

    public static MouseState MouseState => currentMouseState;

    public static MouseState RefreshMouseState()
    {
        previousMouseState = currentMouseState;
        currentMouseState = Mouse.GetState();

        return currentMouseState;
    }

    private static readonly GamePadState[] currentGamepadStates = new GamePadState[4];
    private static readonly GamePadState[] previousGamepadStates = new GamePadState[4];

    public static GamePadState RefreshGamePadState() => RefreshGamePadState(PlayerIndex.One);

    public static GamePadState RefreshGamePadState(PlayerIndex index)
    {
        for(var i = 0; i < 4; i++)
        {
            previousGamepadStates[i] = currentGamepadStates[i];
            currentGamepadStates[i] = GamePad.GetState((PlayerIndex)i);
        }

        return currentGamepadStates[(int)index];
    }

    public static bool GetDown(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !InputDisabled;
    }

    public static bool GetPressed(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key) && !InputDisabled;
    }

    public static bool GetReleased(Keys key)
    {
        return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key) && !InputDisabled;
    }

    public static bool GetDown(Buttons button, PlayerIndex index)
    {
        return currentGamepadStates[(int)index].IsConnected && currentGamepadStates[(int)index].IsButtonDown(button) && !InputDisabled;
    }

    public static bool GetPressed(Buttons button, PlayerIndex index)
    {
        return currentGamepadStates[(int)index].IsConnected && currentGamepadStates[(int)index].IsButtonDown(button) && !previousGamepadStates[(int)index].IsButtonDown(button) && !InputDisabled;
    }

    public static bool GetReleased(Buttons button, PlayerIndex index)
    {
        return currentGamepadStates[(int)index].IsConnected && !currentGamepadStates[(int)index].IsButtonDown(button) && previousGamepadStates[(int)index].IsButtonDown(button) && !InputDisabled;
    }

    public static bool GetDown(Buttons button) => GetDown(button, PlayerIndex.One);

    public static bool GetPressed(Buttons button) => GetPressed(button, PlayerIndex.One);

    public static bool GetReleased(Buttons button) => GetReleased(button, PlayerIndex.One);

    public static bool GetDown(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Pressed && !InputDisabled;
    }

    public static bool GetPressed(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Pressed && GetMouseButtonState(previousMouseState, button) == ButtonState.Released && !InputDisabled;
    }

    public static bool GetReleased(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Released && GetMouseButtonState(previousMouseState, button) == ButtonState.Pressed && !InputDisabled;
    }

    private static ButtonState GetMouseButtonState(MouseState state, MouseButtons button)
    {
        return button switch
        {
            MouseButtons.LeftButton => state.LeftButton,
            MouseButtons.RightButton => state.RightButton,
            MouseButtons.MiddleButton => state.MiddleButton,
            MouseButtons.XButton1 => state.XButton1,
            MouseButtons.XButton2 => state.XButton2,
            _ => ButtonState.Released,
        };
    }

    public static ButtonState GetMouseButtonState(MouseButtons button) => GetMouseButtonState(currentMouseState, button);

    public static int GetScrollDelta()
    {
        return InputDisabled ? 0 : System.Math.Sign(currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue);
    }

    public static bool GetAnyDown(InputType inputType)
    {
        if(InputDisabled) return false;

        switch(inputType)
        {
            case InputType.Keyboard:
                return currentKeyboardState.GetPressedKeyCount() > 0;
            case InputType.Mouse:
                for(int i = 0; i < 5; i++)
                    if(GetDown((MouseButtons)i)) return true;
                return false;
            case InputType.GamePad:
                foreach(var state in currentGamepadStates)
                {
                    if(!state.IsConnected) continue;
                    return state != GamePadState.Default;
                }
                return false;
            case InputType.Invalid:
                return GetAnyDown(InputType.Keyboard) || GetAnyDown(InputType.Mouse) || GetAnyDown(InputType.GamePad);
            default:
                throw new System.Exception($"Invalid {nameof(InputType)}: {inputType}");
        }
    }

    public static bool GetAnyPressed(InputType inputType)
    {
        if(InputDisabled) return false;

        switch(inputType)
        {
            case InputType.Keyboard:
                return currentKeyboardState.GetPressedKeyCount() > previousKeyboardState.GetPressedKeyCount();
            case InputType.Mouse:
                for(int i = 0; i < 5; i++)
                    if(GetPressed((MouseButtons)i)) return true;
                return false;
            case InputType.GamePad:
                foreach(var state in currentGamepadStates)
                {
                    if(!state.IsConnected) continue;

                    // fuck this bro
                    if(GetPressed(Buttons.A)
                    || GetPressed(Buttons.B)
                    || GetPressed(Buttons.X)
                    || GetPressed(Buttons.Y)
                    || GetPressed(Buttons.Back)
                    || GetPressed(Buttons.Start)
                    || GetPressed(Buttons.BigButton)
                    || GetPressed(Buttons.DPadDown)
                    || GetPressed(Buttons.DPadLeft)
                    || GetPressed(Buttons.DPadRight)
                    || GetPressed(Buttons.DPadUp)
                    || GetPressed(Buttons.LeftShoulder)
                    || GetPressed(Buttons.LeftStick)
                    || GetPressed(Buttons.LeftThumbstickDown)
                    || GetPressed(Buttons.LeftThumbstickLeft)
                    || GetPressed(Buttons.LeftThumbstickRight)
                    || GetPressed(Buttons.LeftThumbstickUp)
                    || GetPressed(Buttons.LeftTrigger)
                    || GetPressed(Buttons.RightShoulder)
                    || GetPressed(Buttons.RightStick)
                    || GetPressed(Buttons.RightThumbstickDown)
                    || GetPressed(Buttons.RightThumbstickLeft)
                    || GetPressed(Buttons.RightThumbstickRight)
                    || GetPressed(Buttons.RightThumbstickUp)
                    || GetPressed(Buttons.RightTrigger))
                        return true;
                }
                return false;
            case InputType.Invalid:
                return GetAnyPressed(InputType.Keyboard) || GetAnyPressed(InputType.Mouse) || GetAnyPressed(InputType.GamePad);
            default:
                throw new System.Exception($"Invalid {nameof(InputType)}: {inputType}");
        }
    }

    public static bool GetAnyReleased(InputType inputType)
    {
        if(InputDisabled) return false;

        switch(inputType)
        {
            case InputType.Keyboard:
                return currentKeyboardState.GetPressedKeyCount() < previousKeyboardState.GetPressedKeyCount();
            case InputType.Mouse:
                for(int i = 0; i < 5; i++)
                    if(GetReleased((MouseButtons)i)) return true;
                return false;
            case InputType.GamePad:
                foreach(var state in currentGamepadStates)
                {
                    if(!state.IsConnected) continue;

                    if(GetReleased(Buttons.A)
                    || GetReleased(Buttons.B)
                    || GetReleased(Buttons.X)
                    || GetReleased(Buttons.Y)
                    || GetReleased(Buttons.Back)
                    || GetReleased(Buttons.Start)
                    || GetReleased(Buttons.BigButton)
                    || GetReleased(Buttons.DPadDown)
                    || GetReleased(Buttons.DPadLeft)
                    || GetReleased(Buttons.DPadRight)
                    || GetReleased(Buttons.DPadUp)
                    || GetReleased(Buttons.LeftShoulder)
                    || GetReleased(Buttons.LeftStick)
                    || GetReleased(Buttons.LeftThumbstickDown)
                    || GetReleased(Buttons.LeftThumbstickLeft)
                    || GetReleased(Buttons.LeftThumbstickRight)
                    || GetReleased(Buttons.LeftThumbstickUp)
                    || GetReleased(Buttons.LeftTrigger)
                    || GetReleased(Buttons.RightShoulder)
                    || GetReleased(Buttons.RightStick)
                    || GetReleased(Buttons.RightThumbstickDown)
                    || GetReleased(Buttons.RightThumbstickLeft)
                    || GetReleased(Buttons.RightThumbstickRight)
                    || GetReleased(Buttons.RightThumbstickUp)
                    || GetReleased(Buttons.RightTrigger))
                        return true;
                }
                return false;
            case InputType.Invalid:
                return GetAnyReleased(InputType.Keyboard) || GetAnyReleased(InputType.Mouse) || GetAnyReleased(InputType.GamePad);
            default:
                throw new System.Exception($"Invalid {nameof(InputType)}: {inputType}");
        }
    }

    private static char ConvertKeyToChar(Keys key, KeyboardState state)
    {
        bool shift = state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);
        bool capsLock = state.CapsLock;

        return key switch
        {
            Keys.A => shift ^ capsLock ? 'A' : 'a',
            Keys.B => shift ^ capsLock ? 'B' : 'b',
            Keys.C => shift ^ capsLock ? 'C' : 'c',
            Keys.D => shift ^ capsLock ? 'D' : 'd',
            Keys.E => shift ^ capsLock ? 'E' : 'e',
            Keys.F => shift ^ capsLock ? 'F' : 'f',
            Keys.G => shift ^ capsLock ? 'G' : 'g',
            Keys.H => shift ^ capsLock ? 'H' : 'h',
            Keys.I => shift ^ capsLock ? 'I' : 'i',
            Keys.J => shift ^ capsLock ? 'J' : 'j',
            Keys.K => shift ^ capsLock ? 'K' : 'k',
            Keys.L => shift ^ capsLock ? 'L' : 'l',
            Keys.M => shift ^ capsLock ? 'M' : 'm',
            Keys.N => shift ^ capsLock ? 'N' : 'n',
            Keys.O => shift ^ capsLock ? 'O' : 'o',
            Keys.P => shift ^ capsLock ? 'P' : 'p',
            Keys.Q => shift ^ capsLock ? 'Q' : 'q',
            Keys.R => shift ^ capsLock ? 'R' : 'r',
            Keys.S => shift ^ capsLock ? 'S' : 's',
            Keys.T => shift ^ capsLock ? 'T' : 't',
            Keys.U => shift ^ capsLock ? 'U' : 'u',
            Keys.V => shift ^ capsLock ? 'V' : 'v',
            Keys.W => shift ^ capsLock ? 'W' : 'w',
            Keys.X => shift ^ capsLock ? 'X' : 'x',
            Keys.Y => shift ^ capsLock ? 'Y' : 'y',
            Keys.Z => shift ^ capsLock ? 'Z' : 'z',
            Keys.D0 => shift ? ')' : '0',
            Keys.D1 => shift ? '!' : '1',
            Keys.D2 => shift ? '@' : '2',
            Keys.D3 => shift ? '#' : '3',
            Keys.D4 => shift ? '$' : '4',
            Keys.D5 => shift ? '%' : '5',
            Keys.D6 => shift ? '^' : '6',
            Keys.D7 => shift ? '&' : '7',
            Keys.D8 => shift ? '*' : '8',
            Keys.D9 => shift ? '(' : '9',
            Keys.Space => ' ',
            Keys.OemPeriod => shift ? '>' : '.',
            Keys.OemComma => shift ? '<' : ',',
            Keys.OemQuestion => shift ? '?' : '/',
            Keys.OemSemicolon => shift ? ':' : ';',
            Keys.OemQuotes => shift ? '"' : '\'',
            Keys.OemBackslash => shift ? '|' : '\\',
            Keys.OemPipe => shift ? '|' : '\\',
            Keys.OemOpenBrackets => shift ? '{' : '[',
            Keys.OemCloseBrackets => shift ? '}' : ']',
            Keys.OemMinus => shift ? '_' : '-',
            Keys.OemPlus => shift ? '+' : '=',
            Keys.OemTilde => shift ? '~' : '`',
            Keys.Back => '\x127',
            _ => '\0',
        };
    }
}

public enum InputType
{
    Invalid = -1,
    Keyboard,
    Mouse,
    GamePad
}
