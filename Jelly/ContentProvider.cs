using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework.Graphics;

using Jelly.Graphics;
using System.IO;

namespace Jelly;

public abstract class ContentProvider
{
    public abstract Texture2D? GetTexture(string pathName);

    public virtual bool TryGetTexture(string pathName, [NotNullWhen(true)] out Texture2D? texture)
    {
        texture = GetTexture(pathName);
        return texture is not null;
    }
}

public class BasicContentProvider() : ContentProvider
{
    private static readonly Dictionary<string, Texture2D> loadedTextures = [];
    private static readonly List<string> pathsThatDontWork = [];

    public override Texture2D GetTexture(string pathName)
    {
        if(pathsThatDontWork.Contains(pathName))
        {
            return null;
        }

        if(loadedTextures.TryGetValue(pathName, out Texture2D value))
        {
            return value;
        }

        try
        {
            var texture = Texture2D.FromFile(Renderer.GraphicsDevice, Path.Combine("Content", pathName, ".png"));
            loadedTextures.Add(pathName, texture);
            return texture;
        }
        catch(Exception e)
        {
            pathsThatDontWork.Add(pathName);
            JellyBackend.Logger.LogError(e);
            return null;
        }
    }
}
