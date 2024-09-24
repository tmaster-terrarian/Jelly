using System;

using Microsoft.Xna.Framework;

using Jelly.Components.Attributes;
using System.Text.Json.Serialization;

namespace Jelly.Components;

[MutuallyExclusiveComponent(typeof(Solid), ExclusionKind = MutuallyExclusiveComponentKind.Default)]
[SingletonComponent]
public class Actor : Moveable, IWorldObject
{
    public static float MaxContinousMovementThreshold { get; set; } = 32f;

	public bool Collides { get; set; } = true;
	public bool CollidesWithSolids { get; set; } = true;
	public bool CollidesWithJumpthroughs { get; set; } = true;

	public bool NudgeOnMove { get; set; } = true;

	[JsonIgnore]
	public bool OnGround { get; protected set; }

    public bool CheckOnGround()
	{
		return CheckColliding(Hitbox.Shift(0, 1));
	}

	public bool CheckColliding(Rectangle rectangle, bool ignoreJumpThroughs = false)
	{
		if(!Collides) return false;

		if(Scene.CollisionSystem.TileMeeting(rectangle)) return true;
		if(CollidesWithSolids && Scene.CollisionSystem.SolidMeeting(rectangle)) return true;

		if(!ignoreJumpThroughs && CollidesWithJumpthroughs)
		{
			return CheckCollidingJumpthrough(rectangle);
		}

		return false;
	}

	public bool CheckCollidingJumpthrough(Rectangle rectangle)
	{
		if(!Collides) return false;
		if(!CollidesWithJumpthroughs) return false;

		Rectangle newRect = new(rectangle.Left, rectangle.Bottom - 1, rectangle.Width, 1);

		Rectangle rect = Scene.CollisionSystem.JumpThroughPlace(newRect) ?? Rectangle.Empty;
		Rectangle rect2 = Scene.CollisionSystem.JumpThroughPlace(newRect.Shift(0, -1)) ?? Rectangle.Empty;

		if(rect != Rectangle.Empty) return rect != rect2;

		Line line = Scene.CollisionSystem.JumpThroughSlopePlace(newRect) ?? Line.Empty;
		Line line2 = Scene.CollisionSystem.JumpThroughSlopePlace(newRect.Shift(0, -1)) ?? Line.Empty;

		if(line != Line.Empty) return line != line2;

		return false;
	}

    public virtual void MoveX(float amount, Action? onCollide)
    {
		if(!float.IsNormal(amount)) amount = 0;

        RemainderX += amount;
        int move = (int)Math.Round(RemainderX);
        RemainderX -= move;

        if(move != 0)
        {
			if(!Collidable || Math.Abs(amount) > MaxContinousMovementThreshold)
			{
				Entity.X += move;
				return;
			}

            int sign = Math.Sign(move);
            while(move != 0)
            {
                bool col1 = CheckColliding((sign >= 0 ? RightEdge : LeftEdge).Shift(new(sign, 0)));
                if(NudgeOnMove && col1 && !CheckColliding((sign >= 0 ? RightEdge : LeftEdge).Shift(new(sign, -1)), true))
                {
                    // slope up
                    Entity.X += sign;
                    Entity.Y -= 1;
                    move -= sign;
                }
                else if(!col1)
                {
                    if(NudgeOnMove && OnGround)
                    {
                        // slope down
                        if(!CheckColliding(BottomEdge.Shift(new(sign, 1))) && CheckColliding(BottomEdge.Shift(new(sign, 2))))
                            Entity.Y += 1;
                    }
                    Entity.X += sign;
                    move -= sign;
                }
                else
                {
                    onCollide?.Invoke();
                    break;
                }
            }
        }
    }

    public virtual void MoveY(float amount, Action? onCollide)
    {
		if(!float.IsNormal(amount)) amount = 0;

        RemainderY += amount;
        int move = (int)Math.Round(RemainderY);
        RemainderY -= move;

        if(move != 0)
        {
			if(!Collidable || Math.Abs(amount) > MaxContinousMovementThreshold)
			{
				Entity.Y += move;
				return;
			}

            int sign = Math.Sign(move);
            while(move != 0)
            {
                if(!CheckColliding((sign >= 0 ? BottomEdge : TopEdge).Shift(new(0, sign)), sign < 0))
                {
                    Entity.Y += sign;
                    move -= sign;
                    continue;
                }

                onCollide?.Invoke();
                break;
            }
        }
    }

    public virtual bool IsRiding(Solid solid)
    {
		ArgumentNullException.ThrowIfNull(solid, nameof(solid));

        return Collides && CollidesWithSolids && solid.Collidable
			&& !solid.Intersects(Hitbox) && solid.Intersects(Hitbox.Shift(0, 1));
    }

    public virtual void Squish()
    {
        
    }
}
