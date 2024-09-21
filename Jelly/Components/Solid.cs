using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Jelly.Components;

public class Solid : Component
{
    Rectangle bbox;

    float xRemainder;
    float yRemainder;

    Vector2 velocity = Vector2.Zero;

    public Rectangle Hitbox
    {
        get => bbox;
        set {
            this.bbox = new(value.Location - Entity.Pivot, value.Size);
        }
    }

    public Rectangle WorldHitbox => new(bbox.X + Entity.X, bbox.Y + Entity.Y, bbox.Width, bbox.Height);

    public Vector2 Velocity { get => velocity; set => velocity = value; }

    public bool Collidable { get; protected set; }

    public bool DefaultBehavior { get; set; }

    public override void Added(Entity entity)
    {
        base.Added(entity);

        Point origin = Entity.Pivot;
        bbox.X -= origin.X;
        bbox.Y -= origin.Y;
    }

    public override void Update()
    {
        if(!DefaultBehavior) return;

        Move(velocity.X, velocity.Y);
    }

    public void Move(float x, float y)
    {
        xRemainder += x;
        yRemainder += y;   
        int moveX = (int)Math.Round(xRemainder);
        int moveY = (int)Math.Round(yRemainder);
        if(moveX != 0 || moveY != 0)
        {
            // Loop through every Actor in the Level, add it to
            // a list if actor.IsRiding(this) is true
            var riding = GetAllRidingActors();
            var allActors = Scene.Entities.FindAllOfType<Actor>();

            // Make this Solid non-collidable for Actors,
            // so that Actors moved by it do not get stuck on it
            Collidable = false;

            if(moveX != 0)
            {
                xRemainder -= moveX;
                Entity.X += moveX;
                if(moveX > 0)
                {
                    foreach(var entity in allActors)
                    {
                        var actor = entity.Components.Get<Actor>();
                        if(this.WorldHitbox.Intersects(actor.Hitbox))
                        {
                            // Push right
                            actor.MoveX(this.WorldHitbox.Right - actor.Hitbox.Left, actor.Squish);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry right
                            actor.MoveX(moveX, null);
                        }
                    }
                }
                else
                {
                    foreach(var entity in allActors)
                    {
                        var actor = entity.Components.Get<Actor>();
                        if(this.WorldHitbox.Intersects(actor.Hitbox))
                        {
                            // Push left
                            actor.MoveX(this.WorldHitbox.Left - actor.Hitbox.Right, actor.Squish);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry left
                            actor.MoveX(moveX, null);
                        }
                    }
                }
            }

            if(moveY != 0)
            {
                yRemainder -= moveY;
                Entity.Y += moveY;
                if(moveY > 0)
                {
                    foreach(var entity in allActors)
                    {
                        var actor = entity.Components.Get<Actor>();
                        if(this.WorldHitbox.Intersects(actor.Hitbox))
                        {
                            // Push right
                            actor.MoveY(this.WorldHitbox.Bottom - actor.Hitbox.Top, actor.Squish);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry right
                            actor.MoveY(moveY, null);
                        }
                    }
                }
                else
                {
                    foreach(var entity in allActors)
                    {
                        var actor = entity.Components.Get<Actor>();
                        if(this.WorldHitbox.Intersects(actor.Hitbox))
                        {
                            // Push left
                            actor.MoveY(this.WorldHitbox.Top - actor.Hitbox.Bottom, actor.Squish);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry left
                            actor.MoveY(moveY, null);
                        }
                    }
                }
            }

            // Re-enable collisions for this Solid
            Collidable = true;
        }
    }

    private List<Actor> GetAllRidingActors()
    {
        List<Actor> actors = [];
        foreach(var entity in Scene.Entities.FindAllOfType<Actor>())
        {
            var actor = entity.Components.Get<Actor>();
            if(actor.IsRiding(this)) actors.Add(actor);
        }
        return actors;
    }

    public virtual bool Intersects(Rectangle rectangle)
    {
        return rectangle.Intersects(WorldHitbox);
    }
}
