using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jelly.Graphics;

public static class SpriteBatchExtensions
{
	public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
	{
		var split = text.Split(' ');
		float x = 0;
		foreach(var word in split)
		{
			spriteBatch.DrawString(font, word, position + (Vector2.UnitX * x * scale.X), color, rotation, origin, scale, effects, layerDepth, rtl);
			x += font.MeasureString(word).X + spaceSize;
		}
	}

	public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize, float rotation, Vector2 origin, float scale = 1, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
	{
		spriteBatch.DrawStringSpacesFix(font, text, position, color, spaceSize, rotation, origin, new Vector2(scale), effects, layerDepth, rtl);
	}

	public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize)
	{
		spriteBatch.DrawStringSpacesFix(font, text, position, color, spaceSize, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
	}

	public static void Draw(this SpriteBatch spriteBatch, TextComponent textComponent, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
	{
		ArgumentNullException.ThrowIfNull(textComponent);
		textComponent.Draw(spriteBatch, position, color, rotation, origin, scale, effects, layerDepth);
	}

	public static void Draw(this SpriteBatch spriteBatch, TextComponent textComponent, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
	{
		ArgumentNullException.ThrowIfNull(textComponent);
		textComponent.Draw(spriteBatch, position, color, rotation, origin, scale, effects, layerDepth);
	}

	public static void Draw(this SpriteBatch spriteBatch, TextComponent textComponent, Vector2 position, Color color)
	{
		ArgumentNullException.ThrowIfNull(textComponent);
		textComponent.Draw(spriteBatch, position, color);
	}

	public static void DrawNineSlice(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Point topLeft, Point bottomRight, Color color, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
	{
		ArgumentNullException.ThrowIfNull(texture);

		var dest = destinationRectangle;
		var src = sourceRectangle ?? texture.Bounds;

		if(dest.Size == Point.Zero) return;
		if(src.Size == Point.Zero) return;

		// if the nineslice would end up being pointless, just draw the texture directly
		if(dest.Size == src.Size)
		{
			spriteBatch.Draw(texture, dest, src, color, 0, origin, effects, layerDepth);
			return;
		}

		int left = topLeft.X;
		int top = topLeft.Y;
		int right = bottomRight.X;
		int bottom = bottomRight.Y;

		#region Source Rectangles
		Rectangle topLeftSrc =		new(src.X,							src.Y,							left,						top);

		Rectangle topSrc =			new(src.X + left,					src.Y,							src.Width - left - right,	top);

		Rectangle topRightSrc =		new(src.X + src.Width - right,		src.Y,							right,						top);

		Rectangle leftSrc =			new(src.X,							src.Y + top,					left,						src.Height - top - bottom);

		Rectangle centerSrc =		new(src.X + left,					src.Y + top,					src.Width - left - right,	src.Height - top - bottom);

		Rectangle rightSrc =		new(src.X + src.Width - right,		src.Y + top,					right,						src.Height - top - bottom);

		Rectangle bottomLeftSrc =	new(src.X, 							src.Y + src.Height - bottom,	left,						bottom);

		Rectangle bottomSrc =		new(src.X + left,					src.Y + src.Height - bottom,	src.Width - left - right,	bottom);

		Rectangle bottomRightSrc =	new(src.X + src.Width - right,		src.Y + src.Height - bottom,	right,						bottom);
		#endregion

		#region Destination Rectangles
		Rectangle topLeftRect =		new(dest.X,							dest.Y,							left,						top);

		Rectangle topRect =			new(dest.X + left,					dest.Y,							dest.Width - left - right,	top);

		Rectangle topRightRect =	new(dest.X + dest.Width - right,	dest.Y,							right,						top);

		Rectangle leftRect =		new(dest.X,							dest.Y + top,					left,						dest.Height - top - bottom);

		Rectangle centerRect =		new(dest.X + left,					dest.Y + top,					dest.Width - left - right,	dest.Height - top - bottom);

		Rectangle rightRect =		new(dest.X + dest.Width - right,	dest.Y + top,					right,						dest.Height - top - bottom);

		Rectangle bottomLeftRect =	new(dest.X, 						dest.Y + dest.Height - bottom,	left,						bottom);

		Rectangle bottomRect =		new(dest.X + left,					dest.Y + dest.Height - bottom,	dest.Width - left - right,	bottom);

		Rectangle bottomRightRect =	new(dest.X + dest.Width - right,	dest.Y + dest.Height - bottom,	right,						bottom);
		#endregion

		#region Drawing
		if(topLeftRect.Size != Point.Zero)
			spriteBatch.Draw(texture, topLeftRect.Shift((-origin).ToPoint()),		topLeftSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(topRect.Size != Point.Zero)
			spriteBatch.Draw(texture, topRect.Shift((-origin).ToPoint()),			topSrc,			color, 0, Vector2.Zero, effects, layerDepth);

		if(topRightRect.Size != Point.Zero)
			spriteBatch.Draw(texture, topRightRect.Shift((-origin).ToPoint()),		topRightSrc,	color, 0, Vector2.Zero, effects, layerDepth);

		if(leftRect.Size != Point.Zero)
			spriteBatch.Draw(texture, leftRect.Shift((-origin).ToPoint()),			leftSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(centerRect.Size != Point.Zero)
			spriteBatch.Draw(texture, centerRect.Shift((-origin).ToPoint()),		centerSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(rightRect.Size != Point.Zero)
			spriteBatch.Draw(texture, rightRect.Shift((-origin).ToPoint()),		rightSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(bottomLeftRect.Size != Point.Zero)
			spriteBatch.Draw(texture, bottomLeftRect.Shift((-origin).ToPoint()),	bottomLeftSrc,	color, 0, Vector2.Zero, effects, layerDepth);

		if(bottomRect.Size != Point.Zero)
			spriteBatch.Draw(texture, bottomRect.Shift((-origin).ToPoint()),		bottomSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(bottomRightRect.Size != Point.Zero)
			spriteBatch.Draw(texture, bottomRightRect.Shift((-origin).ToPoint()),	bottomRightSrc,	color, 0, Vector2.Zero, effects, layerDepth);
		#endregion
	}

	public static void DrawNineSlice(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Point topLeft, Point bottomRight, Color color)
	{
		spriteBatch.DrawNineSlice(texture, destinationRectangle, sourceRectangle, topLeft, bottomRight, color, Vector2.Zero);
	}

	public static void DrawNineSlice(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Rectangle centerSliceBounds, Color color)
	{
		ArgumentNullException.ThrowIfNull(texture);

		spriteBatch.DrawNineSlice(texture, destinationRectangle, sourceRectangle, centerSliceBounds.Location, (sourceRectangle ?? texture.Bounds).Size - (centerSliceBounds.Location + centerSliceBounds.Size), color);
	}
}
