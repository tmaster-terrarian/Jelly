using System;
using System.Collections.Generic;
using Jelly.Graphics;

using Microsoft.Xna.Framework.Graphics;

namespace Jelly;

public abstract class ContentProvider
{
    public abstract Texture2D? GetTexture(string pathName);
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
            var texture = Texture2D.FromFile(Renderer.GraphicsDevice, pathName);
            loadedTextures.Add(pathName, texture);
            return texture;
        }
        catch(Exception e)
        {
            pathsThatDontWork.Add(pathName);
            Logger.JellyLogger.Error(e);
            return null;
        }
    }
}
