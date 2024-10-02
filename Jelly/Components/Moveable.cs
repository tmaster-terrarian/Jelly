using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jelly.Components;

public class Moveable : Component
{
    private Rectangle bbox = new Rectangle(0, 0, 8, 8);
    private Vector2 rPos = Vector2.Zero;

    public virtual int Facing { get; set; } = 1;

	public virtual bool Collidable { get; set; } = true;

	[JsonIgnore]
    public SpriteEffects SpriteEffects => Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

	[JsonInclude]
    public Vector2 velocity;

	[JsonInclude]
	protected Point bboxOffset;

    protected Vector2 RemainderPosition
    {
        get => rPos;
        set => rPos = value;
    }

	[JsonIgnore]
    protected float RemainderX
    {
        get => rPos.X;
        set => rPos.X = value;
    }

	[JsonIgnore]
    protected float RemainderY
    {
        get => rPos.Y;
        set => rPos.Y = value;
    }

    public int Width
    {
        get => bbox.Width;
        set => bbox.Width = MathHelper.Max(0, value);
    }

	public int Height
    {
        get => bbox.Height;
        set => bbox.Height = MathHelper.Max(0, value);
    }

	[JsonIgnore]
    public Rectangle Hitbox
    {
        get => ((Entity is not null && bbox.Location != Entity.Position) ? (bbox = new Rectangle(Entity.Position, bbox.Size)) : bbox).Shift(bboxOffset);
        set {
            bbox = value.Shift(Point.Zero-bboxOffset);
			if(Entity is not null)
            	Entity.Position = value.Location - bboxOffset;
        }
    }

	[JsonIgnore]
    public int Left => Entity.X + bboxOffset.X;

	[JsonIgnore]
    public int Right => Entity.X + bboxOffset.X + Width;

	[JsonIgnore]
    public int Top => Entity.Y + bboxOffset.Y;

	[JsonIgnore]
    public int Bottom => Entity.Y + bboxOffset.Y + Height;

	[JsonIgnore]
	public Point Center {
		get => new(Left + (Width/2), Top + (Height/2));
		set {
			Entity.Position = new Point(value.X - (Width/2), value.Y - (Height/2)) - bboxOffset;
		}
	}

	[JsonIgnore]
	public Point TopLeft {
		get => Entity.Position + bboxOffset;
		set {
			Entity.Position = value - bboxOffset;
		}
	}

	[JsonIgnore]
	public Point TopRight {
		get => new(Right, Top);
		set {
			Entity.Position = new Point(value.X - Width, value.Y) - bboxOffset;
		}
	}

	[JsonIgnore]
	public Point BottomLeft {
		get => new(Left, Bottom);
		set {
			Entity.Position = new Point(value.X, value.Y - Height) - bboxOffset;
		}
	}

	[JsonIgnore]
	public Point BottomRight {
		get => new(Right, Bottom);
		set {
			Entity.Position = new Point(value.X - Width, value.Y - Height) - bboxOffset;
		}
	}

	[JsonIgnore]
    public Rectangle RightEdge => new(Right - 1, Top, 1, Height);

	[JsonIgnore]
	public Rectangle LeftEdge => new(Left, Top, 1, Height);

	[JsonIgnore]
	public Rectangle TopEdge => new(Left, Top, Width, 1);

	[JsonIgnore]
	public Rectangle BottomEdge => new(Left, Bottom - 1, Width, 1);

	[JsonIgnore]
	public Point LeftMiddle {
		get => new(Left, Top + (Height/2));
		set {
			Entity.Position = new Point(value.X, value.Y - (Height/2)) - bboxOffset;
		}
	}

	[JsonIgnore]
	public Point RightMiddle {
		get => new(Right, Top + (Height/2));
		set {
			Entity.Position = new Point(value.X - Width, value.Y - (Height/2)) - bboxOffset;
		}
	}

	[JsonIgnore]
	public Point TopMiddle {
		get => new(Left + (Width/2), Top);
		set {
			Entity.Position = new Point(value.X - (Width/2), value.Y) - bboxOffset;
		}
	}

	[JsonIgnore]
	public Point BottomMiddle {
		get => new(Left + (Width/2), Bottom);
		set {
			Entity.Position = new Point(value.X - (Width/2), value.Y - Height) - bboxOffset;
		}
	}

	public virtual bool Intersects(Rectangle rectangle)
    {
        return rectangle.Intersects(Hitbox);
    }
}
