using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Jelly;

public static class Input
{
    public static bool IgnoreInput { get; set; }

    private static KeyboardState currentKeyboardState;
    private static KeyboardState previousKeyboardState;

    public static KeyboardState KeyboardState => currentKeyboardState;

    public static KeyboardState RefreshKeyboardState()
    {
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();

        return currentKeyboardState;
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
        return currentKeyboardState.IsKeyDown(key) && !IgnoreInput;
    }

    public static bool GetPressed(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key) && !IgnoreInput;
    }

    public static bool GetReleased(Keys key)
    {
        return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key) && !IgnoreInput;
    }

    public static bool GetDown(Buttons button, PlayerIndex index)
    {
        return currentGamepadStates[(int)index].IsButtonDown(button) && !IgnoreInput;
    }

    public static bool GetPressed(Buttons button, PlayerIndex index)
    {
        return currentGamepadStates[(int)index].IsButtonDown(button) && !previousGamepadStates[(int)index].IsButtonDown(button) && !IgnoreInput;
    }

    public static bool GetReleased(Buttons button, PlayerIndex index)
    {
        return !currentGamepadStates[(int)index].IsButtonDown(button) && previousGamepadStates[(int)index].IsButtonDown(button) && !IgnoreInput;
    }

    public static bool GetDown(Buttons button) => GetDown(button, PlayerIndex.One);

    public static bool GetPressed(Buttons button) => GetPressed(button, PlayerIndex.One);

    public static bool GetReleased(Buttons button) => GetReleased(button, PlayerIndex.One);

    public static bool GetDown(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Pressed && !IgnoreInput;
    }

    public static bool GetPressed(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Pressed && GetMouseButtonState(previousMouseState, button) == ButtonState.Released && !IgnoreInput;
    }

    public static bool GetReleased(MouseButtons button)
    {
        return GetMouseButtonState(currentMouseState, button) == ButtonState.Released && GetMouseButtonState(previousMouseState, button) == ButtonState.Pressed && !IgnoreInput;
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
        return IgnoreInput ? 0 : System.Math.Sign(currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue);
    }
}

public enum InputType
{
    Invalid = -1,
    Keyboard,
    Mouse,
    GamePad
}

public class PlayerInputMapping
{
    public MappedInput Right { get; set; } = new(Keys.D);
    public MappedInput Left { get; set; } = new(Keys.A);
    public MappedInput Down { get; set; } = new(Keys.S);
    public MappedInput Up { get; set; } = new(Keys.W);
    public MappedInput Jump { get; set; } = new(Keys.Space);
    public MappedInput Fire { get; set; } = new(MouseButtons.LeftButton);
}

public class MappedInput
{
    public int ButtonIndex { get; }

    public InputType InputType { get; }

    public bool IsDown => InputType switch
    {
        InputType.Keyboard => Input.GetDown((Keys)ButtonIndex),
        InputType.Mouse => Input.GetDown((MouseButtons)ButtonIndex),
        InputType.GamePad => Input.GetDown((Buttons)ButtonIndex),
        InputType.Invalid => false,
        _ => throw new System.Exception($"Invalid {nameof(InputType)}: {InputType}"),
    };

    public bool Pressed => InputType switch
    {
        InputType.Keyboard => Input.GetPressed((Keys)ButtonIndex),
        InputType.Mouse => Input.GetPressed((MouseButtons)ButtonIndex),
        InputType.GamePad => Input.GetPressed((Buttons)ButtonIndex),
        InputType.Invalid => false,
        _ => throw new System.Exception($"Invalid {nameof(InputType)}: {InputType}"),
    };

    public bool Released => InputType switch
    {
        InputType.Keyboard => Input.GetReleased((Keys)ButtonIndex),
        InputType.Mouse => Input.GetReleased((MouseButtons)ButtonIndex),
        InputType.GamePad => Input.GetReleased((Buttons)ButtonIndex),
        InputType.Invalid => false,
        _ => throw new System.Exception($"Invalid {nameof(InputType)}: {InputType}"),
    };

    public MappedInput(int index, InputType type)
    {
        ButtonIndex = index;
        InputType = type;
    }

    public MappedInput(Keys keyboardKey)
    {
        ButtonIndex = (int)keyboardKey;
        InputType = InputType.Keyboard;
    }

    public MappedInput(Buttons gamePadButton)
    {
        ButtonIndex = (int)gamePadButton;
        InputType = InputType.GamePad;
    }

    public MappedInput(MouseButtons mouseButton)
    {
        ButtonIndex = (int)mouseButton;
        InputType = InputType.Mouse;
    }
}
