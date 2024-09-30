using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Jelly;

public abstract class MappedInput(InputType inputType)
{
    public InputType InputType => inputType;

    public abstract bool IsDown { get; }

    public abstract bool Pressed { get; }

    public abstract bool Released { get; }

    public class Keyboard(Keys key) : MappedInput(InputType.Keyboard)
    {
        public override bool IsDown => Input.GetDown(key);

        public override bool Pressed => Input.GetPressed(key);

        public override bool Released => Input.GetReleased(key);
    }

    public class Mouse(MouseButtons mouseButton) : MappedInput(InputType.Mouse)
    {
        public override bool IsDown => Input.GetDown(mouseButton);

        public override bool Pressed => Input.GetPressed(mouseButton);

        public override bool Released => Input.GetReleased(mouseButton);
    }

    public class GamePad(Buttons button, PlayerIndex playerIndex) : MappedInput(InputType.GamePad)
    {
        public override bool IsDown => Input.GetDown(button, playerIndex);

        public override bool Pressed => Input.GetPressed(button, playerIndex);

        public override bool Released => Input.GetReleased(button, playerIndex);
    }
}
