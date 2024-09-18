using System;
using System.Collections.Generic;
using Jelly.Components;
using Jelly.Utilities;
using Microsoft.Xna.Framework;

namespace Jelly;

public class CollisionWorld
{
    private Scene scene;

    public Scene Scene => scene;

    private Rectangle[,] _collisions = null;

    private readonly int[,] _tiles;

    public const int TileSize = 16;

    public bool Visible { get; set; } = true;

    public int NumCollisionChecks { get; set; }

    public EntityList Entities => Scene.Entities;

    public Rectangle Bounds {
        get {
            return new Rectangle(0, 0, Width, Height);
        }
    }
    public Point Size {
        get {
            return new Point(Width, Height);
        }
    }

    public Rectangle[,] Collisions {
        get {
            if(_collisions != null) return _collisions;

            Rectangle[,] rectangles = new Rectangle[Width, Height];

            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    var tile = _tiles[x, y];
                    Rectangle rect = Rectangle.Empty;

                    if(tile != 0)
                        rect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);

                    rectangles[x, y] = rect;
                }
            }

            _collisions = rectangles;
            return rectangles;
        }
    }

    public List<Rectangle> JumpThroughs { get; } = [];
    public List<Line> JumpThroughSlopes { get; } = [];
    public List<Line> Slopes { get; } = [];

    public List<Rectangle> SuccessfulCollisions { get; } = [];

    public int Width => Scene.Width;
    public int Height => Scene.Height;

    public CollisionWorld(Scene scene)
    {
        this.scene = scene;
        _tiles = new int[this.Width, this.Height];
        _collisions = new Rectangle[this.Width, this.Height];
    }

    public Rectangle ValidateArea(Rectangle rectangle)
    {
        return new(
            Math.Clamp(rectangle.X, 0, Width - 1),
            Math.Clamp(rectangle.Y, 0, Height - 1),
            Math.Clamp(rectangle.X + rectangle.Width, rectangle.X + 1, Width) - rectangle.X,
            Math.Clamp(rectangle.Y + rectangle.Height, rectangle.Y + 1, Height) - rectangle.Y
        );
    }

    public void RefreshTileShapes(Rectangle area)
    {
        var _area = ValidateArea(area);

        for(int x = _area.X; x < _area.X + _area.Width; x++)
        {
            for(int y = _area.Y; y < _area.Y + _area.Height; y++)
            {
                int tile = _tiles[x, y];

                Rectangle rect = Rectangle.Empty;
                if(tile != 0)
                    rect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
                _collisions[x, y] = rect;
            }
        }
    }

    public void Update()
    {
        ComponentSystems.Update();
    }

    public void Draw()
    {
        for(int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                int tile = _tiles[x, y];
                if(tile == 0) continue;

                if(Main.Debug.Enabled)
                {
                    NineSlice.DrawNineSlice(Main.LoadContent<Texture2D>("Images/Other/tileOutline"), _collisions[x, y], null, new Point(1), new Point(1), Color.Red * 0.5f);
                }
            }
        }

        if(!Visible) return;

        if(Main.Debug.Enabled)
        {
            foreach(var rect in JumpThroughs)
            {
                SpriteBatch.Draw(Main.OnePixel, rect, Color.LimeGreen * 0.5f);
            }

            foreach(var line in JumpThroughSlopes)
            {
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(line.P1 - new Point(1), new(2)), Color.LimeGreen * 0.75f);
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(line.P2 - new Point(1), new(2)), Color.LimeGreen * 0.75f);

                Point min = new(MathHelper.Min(line.P1.X, line.P2.X), MathHelper.Min(line.P1.Y, line.P2.Y));
                Point max = new(MathHelper.Max(line.P1.X, line.P2.X), MathHelper.Max(line.P1.Y, line.P2.Y));

                for(int x = 0; x < max.X - min.X; x++)
                {
                    float y = (float)(line.P2.Y - line.P1.Y) / (line.P2.X - line.P1.X) * x;

                    if(line.P2.Y < line.P1.Y) y--;

                    SpriteBatch.Draw(Main.OnePixel, new Rectangle(new Point(x + line.P1.X, (int)y + line.P1.Y), new(1)), Color.LimeGreen * 0.5f);
                }
            }

            foreach(var line in Slopes)
            {
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(line.P1 - new Point(1), new(2)), Color.Red * 0.75f);
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(line.P2 - new Point(1), new(2)), Color.Red * 0.75f);

                Point min = new(MathHelper.Min(line.P1.X, line.P2.X), MathHelper.Min(line.P1.Y, line.P2.Y));
                Point max = new(MathHelper.Max(line.P1.X, line.P2.X), MathHelper.Max(line.P1.Y, line.P2.Y));

                for(int x = 0; x < max.X - min.X; x++)
                {
                    float y = (float)(line.P2.Y - line.P1.Y) / (line.P2.X - line.P1.X) * x;

                    if(line.P2.Y < line.P1.Y) y--;

                    SpriteBatch.Draw(Main.OnePixel, new Rectangle(new Point(x + line.P1.X, (int)y + line.P1.Y), new(1)), Color.Red * 0.5f);
                }
            }

            foreach(var entity in GetAllEntitiesWithComponent<Solid>())
            {
                SpriteBatch.Draw(Main.OnePixel, entity.GetComponent<Solid>().WorldBoundingBox, Color.Orange * 0.5f);
            }

            foreach(var actor in GetAllEntitiesWithComponent<Actor>())
            {
                SpriteBatch.Draw(Main.OnePixel, actor.GetComponent<Actor>().WorldBoundingBox, Color.Red * 0.5f);
            }
        }

        ComponentSystems.Draw();
    }

    public void DrawSprite(Sprite sprite, Transform transform)
    {
        SpriteBatch.Draw(sprite.texture, transform.position.ToVector2(), sprite.sourceRectangle, sprite.color, transform.rotation, sprite.origin.ToVector2(), transform.scale, sprite.spriteEffects, sprite.LayerDepth);
    }

    public void Dispose()
    {
        _entityWorld.Dispose();
        _entityWorld = null;

        GC.SuppressFinalize(this);
    }

    public Ecs.Entity? GetEntityWithId(uint id) => _entityWorld.GetEntityWithId(id);

    public List<Actor> GetAllActorComponents()
    {
        List<Actor> actors = [];
        foreach(var actor in ActorSystem.Components)
        {
            if(!actor.IsEnabled) continue;

            actors.Add(actor);
        }
        return actors;
    }

    public List<Ecs.Entity> GetAllEntitiesWithComponent<T>() where T : Component
    {
        List<Ecs.Entity> entities = [];
        foreach(var entity in _entityWorld.Entities)
        {
            if(!entity.IsEnabled) continue;

            if(entity.HasComponent<T>()) entities.Add(entity);
        }
        return entities;
    }

    public bool TileMeeting(Rectangle rect, bool checkSlopes = true)
    {
        if(SuccessfulCollisions.Contains(checkSlopes ? rect.Shift(-100000 * TileSize, 0) : rect)) return true;

        Rectangle[,] cols = Collisions;

        Rectangle newRect = rect;
        newRect.X = MathUtil.FloorToInt(rect.X / (float)TileSize);
        newRect.Y = MathUtil.FloorToInt(rect.Y / (float)TileSize);
        newRect.Width = MathHelper.Max(1, MathUtil.CeilToInt((rect.X + rect.Width) / (float)TileSize) - newRect.X);
        newRect.Height = MathHelper.Max(1, MathUtil.CeilToInt((rect.Y + rect.Height) / (float)TileSize) - newRect.Y);

        for(int x = newRect.X; x < newRect.X + newRect.Width; x++)
        {
            for(int y = newRect.Y; y < newRect.Y + newRect.Height; y++)
            {
                if(!InWorld(x, y)) continue;
                if(_tiles[x, y] <= 0) continue;

                NumCollisionChecks++;
                if(rect.Intersects(cols[x, y]))
                {
                    SuccessfulCollisions.Add(rect);
                    return true;
                }
            }
        }

        if(checkSlopes)
        {
            foreach(var line in Slopes)
            {
                if(line.Intersects(rect))
                {
                    SuccessfulCollisions.Add(rect.Shift(-100000 * TileSize, 0));
                    return true;
                }
            }
        }

        return false;
    }

    public Rectangle? JumpThroughPlace(Rectangle bbox)
    {
        foreach(var rect in JumpThroughs)
        {
            if(bbox.Right <= rect.Left) continue;
            if(bbox.Bottom <= rect.Top) continue;
            if(bbox.Left >= rect.Right) continue;
            if(bbox.Top >= rect.Bottom) continue;

            NumCollisionChecks++;
            if(rect.Intersects(bbox)) return rect;
        }
        return null;
    }

    public Line? JumpThroughSlopePlace(Rectangle bbox)
    {
        foreach(var line in JumpThroughSlopes)
        {
            if(line.Intersects(bbox)) return line;
        }
        return null;
    }

    public bool JumpThroughMeeting(Rectangle rect, bool checkSlopes = true) => JumpThroughPlace(rect) is not null || (checkSlopes && JumpThroughSlopePlace(rect) is not null);

    public Solid? SolidPlace(Rectangle bbox)
    {
        foreach(var entity in Entities)
        {
            if(!entity.IsEnabled) continue;

            Solid solid = entity.GetComponent<Solid>();
            if(solid is not null)
            {
                if(!solid.Enabled) continue;
                if(!solid.Collidable) continue;

                if(Vector2.DistanceSquared(bbox.Center.ToVector2(), solid.WorldHitbox.Center.ToVector2()) > 1024) continue;

                if(bbox.Right <= solid.WorldHitbox.Left) continue;
                if(bbox.Bottom <= solid.WorldHitbox.Top) continue;
                if(bbox.Left >= solid.WorldHitbox.Right) continue;
                if(bbox.Top >= solid.WorldHitbox.Bottom) continue;

                NumCollisionChecks++;
                if(solid.WorldHitbox.Intersects(bbox)) return solid;
            }
        }
        return null;
    }

    public bool SolidMeeting(Rectangle bbox) => SolidPlace(bbox) is not null;

    public void SetTile(int id, Point position)
    {
        if(!InWorld(position.X, position.Y)) return;

        _tiles[position.X, position.Y] = id;

        RefreshTileShapes(new Rectangle(position.X - 1, position.Y - 1, 3, 3));
    }

    public static bool InWorld(CollisionWorld level, int x, int y)
    {
        return x >= 0 && x < level.Width && y >= 0 && y < level.Height;
    }

    public static bool InWorld(CollisionWorld level, Point pos)
    {
        return InWorld(level, pos.X, pos.Y);
    }

    public bool InWorld(int x, int y)
    {
        return InWorld(this, x, y);
    }

    public bool InWorld(Point pos)
    {
        return InWorld(pos.X, pos.Y);
    }
}
