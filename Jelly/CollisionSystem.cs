using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Jelly.Components;
using Jelly.Graphics;
using Jelly.Utilities;

namespace Jelly;

public class CollisionSystem
{
    [JsonIgnore]
    public Scene Scene { get; set; }

    private Rectangle[,] _collisions = null;

    private int[,] _tiles;

    [JsonInclude]
    [JsonPropertyName("tiles")]
    public int[] JsonTiles {
        get {
            int[] value = new int[_tiles.LongLength];
            for(long x = 0; x < _tiles.LongLength; x++)
            {
                value[x] = _tiles[x % Width, x / Width];
            }
            return value;
        }
        set {
            for(long x = 0; x < value.LongLength; x++)
            {
                _tiles[x % Width, x / Width] = value[x];
            }
        }
    }

    public const int TileSize = 16;

    public bool Visible { get; set; } = true;

    [JsonIgnore]
    public Rectangle Bounds {
        get {
            return new Rectangle(Point.Zero, Size);
        }
    }

    [JsonIgnore]
    public Point Size {
        get {
            return new Point(Width, Height);
        }
    }

    [JsonIgnore]
    public Rectangle[,] Collisions {
        get {
            if(_collisions != null && _collisions.LongLength == _tiles.LongLength) return _collisions;

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

    [JsonIgnore] private List<Rectangle> SuccessfulCollisions { get; } = [];

    [JsonIgnore] public int Width => Scene.Width / TileSize;
    [JsonIgnore] public int Height => Scene.Height / TileSize;

    public CollisionSystem(Scene scene)
    {
        this.Scene = scene;
        _tiles = new int[Width, Height];
    }

    public void Resize(int newWidth, int newHeight)
    {
        _collisions = null;

        _tiles = new int[newWidth, newHeight];
    }

    public Rectangle ValidateArea(Rectangle rectangle)
    {
        if(rectangle.Location == Point.Zero)
        {
            return new(
                Point.Zero,
                new(
                    Math.Clamp(rectangle.Width, 1, Width),
                    Math.Clamp(rectangle.Height, 1, Height)
                )
            );
        }

        return new(
            Math.Clamp(rectangle.X, 0, Width - 1),
            Math.Clamp(rectangle.Y, 0, Height - 1),
            Math.Clamp(rectangle.Width, 1, Width - rectangle.X),
            Math.Clamp(rectangle.Height, 1, Height - rectangle.Y)
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
                Collisions[x, y] = rect;
            }
        }
    }

    public void DrawUI()
    {
        if(JellyBackend.DebugEnabled)
        {
            var tex = JellyBackend.ContentProvider.GetTexture("Images/Debug/tileOutline");
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    int tile = _tiles[x, y];
                    if(tile == 0) continue;

                    if(tex is not null)
                        Renderer.SpriteBatch.DrawNineSlice(tex, Collisions[x, y], null, new Point(1), new Point(1), Color.Red * 0.5f);
                    else
                        Renderer.SpriteBatch.Draw(Renderer.PixelTexture, Collisions[x, y], null, Color.Red * 0.5f);
                }
            }
        }

        if(!Visible) return;

        if(JellyBackend.DebugEnabled)
        {
            foreach(var rect in JumpThroughs)
            {
                Renderer.SpriteBatch.Draw(Renderer.PixelTexture, rect, Color.LimeGreen * 0.5f);
            }

            foreach(var line in JumpThroughSlopes)
            {
                Renderer.SpriteBatch.Draw(Renderer.PixelTexture, new Rectangle(line.P1 - new Point(1), new(2)), Color.LimeGreen * 0.75f);
                Renderer.SpriteBatch.Draw(Renderer.PixelTexture, new Rectangle(line.P2 - new Point(1), new(2)), Color.LimeGreen * 0.75f);

                Point min = new(MathHelper.Min(line.P1.X, line.P2.X), MathHelper.Min(line.P1.Y, line.P2.Y));
                Point max = new(MathHelper.Max(line.P1.X, line.P2.X), MathHelper.Max(line.P1.Y, line.P2.Y));

                for(int x = 0; x < max.X - min.X; x++)
                {
                    float y = (float)(line.P2.Y - line.P1.Y) / (line.P2.X - line.P1.X) * x;

                    if(line.P2.Y < line.P1.Y) y--;

                    Renderer.SpriteBatch.Draw(Renderer.PixelTexture, new Rectangle(new Point(x + line.P1.X, (int)y + line.P1.Y), new(1)), Color.LimeGreen * 0.5f);
                }
            }

            foreach(var line in Slopes)
            {
                Renderer.SpriteBatch.Draw(Renderer.PixelTexture, new Rectangle(line.P1 - new Point(1), new(2)), Color.Red * 0.75f);
                Renderer.SpriteBatch.Draw(Renderer.PixelTexture, new Rectangle(line.P2 - new Point(1), new(2)), Color.Red * 0.75f);

                Point min = new(MathHelper.Min(line.P1.X, line.P2.X), MathHelper.Min(line.P1.Y, line.P2.Y));
                Point max = new(MathHelper.Max(line.P1.X, line.P2.X), MathHelper.Max(line.P1.Y, line.P2.Y));

                for(int x = 0; x < max.X - min.X; x++)
                {
                    float y = (float)(line.P2.Y - line.P1.Y) / (line.P2.X - line.P1.X) * x;

                    if(line.P2.Y < line.P1.Y) y--;

                    Renderer.SpriteBatch.Draw(Renderer.PixelTexture, new Rectangle(new Point(x + line.P1.X, (int)y + line.P1.Y), new(1)), Color.Red * 0.5f);
                }
            }

            foreach(var entity in Scene.Entities.FindAllWithComponent<Solid>())
            {
                Renderer.SpriteBatch.Draw(Renderer.PixelTexture, entity.Components.Get<Solid>().Hitbox, Color.Orange * 0.5f);
            }

            foreach(var actor in Scene.Entities.FindAllWithComponent<Actor>())
            {
                Renderer.SpriteBatch.Draw(Renderer.PixelTexture, actor.Components.Get<Actor>().Hitbox, Color.Red * 0.5f);
            }
        }
    }

    public List<Actor> GetAllActorComponents()
    {
        List<Actor> actors = [];
        foreach(var actor in Scene.Entities.FindAllWithComponent<Actor>())
        {
            if(!actor.Enabled) continue;

            actors.Add(actor.Components.Get<Actor>());
        }
        return actors;
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
                if(_tiles[x, y] == 0) continue;

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
            if(bbox.Right <= 0) continue;
            if(bbox.Bottom <= 0) continue;

            rect.Intersects(ref bbox, out bool result);
            if(result) return rect;
        }
        return null;
    }

    public Line? JumpThroughSlopePlace(Rectangle bbox)
    {
        foreach(var line in JumpThroughSlopes)
        {
            if(bbox.Right <= 0) continue;
            if(bbox.Bottom <= 0) continue;

            if(line.Intersects(bbox)) return line;
        }
        return null;
    }

    public bool JumpThroughMeeting(Rectangle rect, bool checkSlopes = true) => JumpThroughPlace(rect) is not null || (checkSlopes && JumpThroughSlopePlace(rect) is not null);

    public Solid? SolidPlace(Rectangle bbox)
    {
        foreach(var entity in Scene.Entities.FindAllWithComponent<Solid>())
        {
            if(!entity.Enabled) continue;

            if(entity.Components.Get<Solid>() is Solid solid)
            {
                if(!solid.Enabled) continue;
                if(!solid.Collidable) continue;

                if(bbox.Right <= 0) continue;
                if(bbox.Bottom <= 0) continue;

                if(Vector2.DistanceSquared(bbox.Center.ToVector2(), solid.Hitbox.Center.ToVector2()) > 25600) continue;

                if(solid.Intersects(bbox)) return solid;
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

    public int GetTile(Point position)
    {
        if(!InWorld(position.X, position.Y)) return 0;

        return _tiles[position.X, position.Y];
    }

    public static bool InWorld(CollisionSystem level, int x, int y)
    {
        return x >= 0 && x < level.Width && y >= 0 && y < level.Height;
    }

    public static bool InWorld(CollisionSystem level, Point pos)
    {
        return InWorld(level, pos.X, pos.Y);
    }

    public bool InWorld(int x, int y)
    {
        return InWorld(this, x, y);
    }

    public bool InWorld(Point pos)
    {
        return InWorld(this, pos.X, pos.Y);
    }
}
