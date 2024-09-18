using System;
using Jelly.Utilities;
using Microsoft.Xna.Framework;

namespace Jelly.Components;

public class Actor : Component
{
    private float yRemainder;
    private float xRemainder;

    protected Vector2 RemainderPosition => new(xRemainder, yRemainder);

	public int Width { get; set; } = 8;
	public int Height { get; set; } = 8;

    public Rectangle Hitbox => new(Entity.X, Entity.Y, Width, Height);

	public bool NudgeOnMove { get; set; } = true;

	public virtual bool NoCollide { get; set; }
	public virtual bool CollidesWithSolids { get; set; } = true;
	public virtual bool CollidesWithJumpthroughs { get; set; } = true;

	public bool OnGround { get; protected set; }

    public int Facing { get; set; } = 1;

	public Point Center {
		get => new(Entity.X + (Width/2), Entity.Y + (Height/2));
		set {
			Entity.Position = new(value.X - (Width/2), value.Y - (Height/2));
		}
	}

	public Point TopLeft {
		get => Entity.Position;
		set {
			Entity.Position = value;
		}
	}

	public Point TopRight {
		get => new(Entity.X + Width, Entity.Y);
		set {
			Entity.Position = new(value.X - Width, value.Y);
		}
	}

	public Point BottomLeft {
		get => new(Entity.X, Entity.Y + Height);
		set {
			Entity.Position = new(value.X, value.Y - Height);
		}
	}

	public Point BottomRight {
		get => new(Entity.X + Width, Entity.Y + Height);
		set {
			Entity.Position = new(value.X - Width, value.Y - Height);
		}
	}

    public Rectangle RightEdge => new(Right.X - 1, Top.Y, 1, Height);

	public Rectangle LeftEdge => new(Left.X, Top.Y, 1, Height);

	public Rectangle TopEdge => new(Left.X, Top.Y, Width, 1);

	public Rectangle BottomEdge => new(Left.X, Bottom.Y - 1, Width, 1);

	public Point Left {
		get => new(Entity.X, Entity.Y + (Height/2));
		set {
			Entity.Position = new(value.X, value.Y - (Height/2));
		}
	}

	public Point Right {
		get => new(Entity.X + Width, Entity.Y + (Height/2));
		set {
			Entity.Position = new(value.X - Width, value.Y - (Height/2));
		}
	}

	public Point Top {
		get => new(Entity.X + (Width/2), Entity.Y);
		set {
			Entity.Position = new(value.X - (Width/2), value.Y);
		}
	}

	public Point Bottom {
		get => new(Entity.X + (Width/2), Entity.Y + Height);
		set {
			Entity.Position = new(value.X - (Width/2), value.Y - Height);
		}
	}

	public bool CheckOnGround()
	{
		return CheckColliding(Hitbox.Shift(0, 1));
	}

	public bool CheckColliding(Rectangle rectangle, bool ignoreJumpThroughs = false)
	{
		if(NoCollide) return false;

		if(Scene.CollisionWorld.TileMeeting(rectangle)) return true;
		if(CollidesWithSolids && Scene.CollisionWorld.SolidMeeting(rectangle)) return true;

		if(!ignoreJumpThroughs)
		{
			return CheckCollidingJumpthrough(rectangle);
		}

		return false;
	}

	public bool CheckCollidingJumpthrough(Rectangle rectangle)
	{
		if(NoCollide) return false;
		if(!CollidesWithJumpthroughs) return false;

		Rectangle newRect = new(rectangle.Left, rectangle.Bottom - 1, rectangle.Width, 1);

		Rectangle rect = Scene.CollisionWorld.JumpThroughPlace(newRect) ?? Rectangle.Empty;
		Rectangle rect2 = Scene.CollisionWorld.JumpThroughPlace(newRect.Shift(0, -1)) ?? Rectangle.Empty;

		if(rect != Rectangle.Empty) return rect != rect2;

		Line line = Scene.CollisionWorld.JumpThroughSlopePlace(newRect) ?? Line.Empty;
		Line line2 = Scene.CollisionWorld.JumpThroughSlopePlace(newRect.Shift(0, -1)) ?? Line.Empty;

		if(line != Line.Empty) return line != line2;

		return false;
	}

    public virtual void MoveX(float amount, Action? onCollide)
    {
        xRemainder += amount;
        int move = (int)Math.Round(xRemainder);
        xRemainder -= move;

        if(move != 0)
        {
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
        yRemainder += amount;
        int move = (int)Math.Round(yRemainder);
        yRemainder -= move;

        if(move != 0)
        {
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
        return solid.Collidable && new Rectangle(Hitbox.Location + new Point(0, 1), Hitbox.Size).Intersects(solid.Hitbox);
    }

    public virtual void Squish()
    {
        
    }
}
