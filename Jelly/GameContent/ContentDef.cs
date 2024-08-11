using System;

namespace Jelly.GameContent;

[Serializable]
public abstract class ContentDef
{
    public string Name { get; set; }

    public ContentDef()
    {
        Name = GetType().Name;
    }
}
