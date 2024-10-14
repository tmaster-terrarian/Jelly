// Modified from source: https://github.com/IrishBruse/LDtkMonogame/blob/main/LDtk.LevelViewer/Camera.cs
// License: MIT
// Licensed to: Ethan Conneely - IrishBruse

using System;
using Microsoft.Xna.Framework;

namespace Jelly.Graphics;

public class Camera
{
    readonly Random random = new();
    float currentShake;
    float shakeMagnitude;
    int shakeTime;

    public Vector2 Position { get; set; } = Vector2.Zero;

    public float Zoom { get; set; } = 1;

    public Matrix Transform { get; private set; } = new();

    public Point MousePositionInWorld => Input.GetMousePositionWithZoom(Zoom, clamp: true) + Position.ToPoint();

    public void SetShake(float shakeMagnitude, int shakeTime)
    {
        if(shakeMagnitude > this.shakeMagnitude)
        {
            this.currentShake = shakeMagnitude;
            this.shakeMagnitude = shakeMagnitude;
        }
        if(shakeTime > this.shakeTime)
        {
            this.shakeTime = shakeTime;
        }
    }

    public void AddShake(float shakeMagnitude, int shakeTime)
    {
        this.currentShake += shakeMagnitude;
        if(shakeMagnitude > this.shakeMagnitude)
        {
            this.shakeMagnitude = shakeMagnitude;
        }
        if(shakeTime > this.shakeTime)
        {
            this.shakeTime = shakeTime;
        }
    }

    public void Update()
    {
        Vector2 basePosition = Position;

        Vector2 shakePosition = basePosition - new Vector2(
            (random.NextSingle() - 0.5f) * 2 * currentShake,
            (random.NextSingle() - 0.5f) * 2 * currentShake
        );

        if(shakeTime > 0)
            currentShake = MathHelper.Max(0, currentShake - ((1f / shakeTime) * shakeMagnitude));
        else
            currentShake = 0;

        Vector2 finalPosition = Vector2.Round(shakePosition);

        Transform = Matrix.CreateTranslation(new Vector3(-finalPosition, 0)) * Matrix.CreateScale(Zoom);
    }
}
