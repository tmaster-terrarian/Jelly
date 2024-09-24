using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Jelly.Components.Attributes;

namespace Jelly.Components;

[MutuallyExclusiveComponent(typeof(Actor), ExclusionKind = MutuallyExclusiveComponentKind.Default)]
[SingletonComponent]
public class Solid : Moveable, IWorldObject
{
    public bool DefaultBehavior { get; set; }

    public override void Update()
    {
        if(!DefaultBehavior) return;

        Move(velocity.X, velocity.Y);
    }

    public virtual void Move(float x, float y)
    {
        if(!float.IsNormal(x)) x = 0;
        if(!float.IsNormal(y)) y = 0;

        RemainderX += x;
        RemainderY += y;

        int moveX = (int)Math.Round(RemainderX);
        int moveY = (int)Math.Round(RemainderY);

        if(moveX != 0 || moveY != 0)
        {
            // Loop through every Actor in the Level, add it to
            // a list if actor.IsRiding(this) is true
            var riding = GetAllRidingActors();
            var allActors = Scene.Entities.FindAllWithComponent<Actor>();

            // Make this Solid non-collidable for Actors,
            // so that Actors moved by it do not get stuck on it
            Collidable = false;

            if(moveX != 0)
            {
                RemainderX -= moveX;
                Entity.X += moveX;
                if(moveX > 0)
                {
                    foreach(var entity in allActors)
                    {
                        var actor = entity.Components.Get<Actor>();
                        if(this.Intersects(actor.Hitbox))
                        {
                            // Push right
                            actor.MoveX(this.Hitbox.Right - actor.Hitbox.Left, actor.Squish);
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
                        if(this.Intersects(actor.Hitbox))
                        {
                            // Push left
                            actor.MoveX(this.Hitbox.Left - actor.Hitbox.Right, actor.Squish);
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
                RemainderY -= moveY;
                Entity.Y += moveY;
                if(moveY > 0)
                {
                    foreach(var entity in allActors)
                    {
                        var actor = entity.Components.Get<Actor>();
                        if(this.Intersects(actor.Hitbox))
                        {
                            // Push down
                            actor.MoveY(this.Hitbox.Bottom - actor.Hitbox.Top, actor.Squish);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry down
                            actor.MoveY(moveY, null);
                        }
                    }
                }
                else
                {
                    foreach(var entity in allActors)
                    {
                        var actor = entity.Components.Get<Actor>();
                        if(this.Intersects(actor.Hitbox))
                        {
                            // Push up
                            actor.MoveY(this.Hitbox.Top - actor.Hitbox.Bottom, actor.Squish);
                        }
                        else if(riding.Contains(actor))
                        {
                            // Carry up
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
        foreach(var entity in Scene.Entities.FindAllWithComponent<Actor>())
        {
            var actor = entity.Components.Get<Actor>();
            if(actor.IsRiding(this)) actors.Add(actor);
        }
        return actors;
    }
}
