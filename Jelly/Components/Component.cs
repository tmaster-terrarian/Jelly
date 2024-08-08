using Microsoft.Xna.Framework;

namespace Jelly.Components;

public abstract class Component
{
    private Entity entity;
    private bool enabled = true;

    public Entity Entity
    {
        get => entity;

        set
        {
            if(value is not null)
            {
                if(ReferenceEquals(value, entity) || value == entity)
                {
                    return;
                }
            }
            else if(entity is null)
            {
                return;
            }

            if(value is not null)
            {
                entity = value;
                Added(value);
            }
            else
            {
                Removed(entity);
                entity = value;
            }
        }
    }

    public bool Enabled
    {
        get => enabled;
        set {
            if(enabled == value)
                return;

            enabled = value;

            if(enabled)
                OnEnable();
            else
                OnDisable();
        }
    }

    public bool Visible { get; set; } = true;

    public Component(Entity entity)
    {
        Entity = entity;
    }

    public virtual void Added(Entity entity) {}

    public virtual void Removed(Entity entity) {}

    public virtual void OnEnable() {}

    public virtual void OnDisable() {}

    public virtual void Update(GameTime gameTime) {}

    /// <summary>
    /// Will not be called if the running instance is headless or if the component is not visible
    /// </summary>
    /// <param name="gameTime"></param>
    public virtual void Draw(GameTime gameTime) {}

    public void Remove()
    {
        Entity?.RemoveComponent(this);
    }
}
