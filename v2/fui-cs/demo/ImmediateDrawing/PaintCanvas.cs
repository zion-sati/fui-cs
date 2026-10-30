using System;

using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>
/// A bitmap at the display's pixel density: dragging paints round dabs into it, over a hint rendered into it from rich
/// text once the text is ready.
/// </summary>
internal sealed class PaintCanvas : CustomDrawable
{
    private const float LogicalSize = 300.0f;
    private const float BrushRadius = 8.0f;

    private readonly Bitmap bitmap;
    private readonly float bitmapScale;
    private Node? hintLabel;
    private bool hintBaked;
    private bool painting;

    public PaintCanvas()
    {
        this.Width(LogicalSize).Height(LogicalSize)
            .SemanticRole(SemanticRole.Image)
            .SemanticLabel("Paint canvas — drag to draw")
            .NodeId("widget-paint");
        bitmapScale = Application.DevicePixelRatio;
        if (bitmapScale <= 0.0f)
        {
            bitmapScale = 1.0f;
        }
        var backingSize = (int)Math.Max(1.0, Math.Ceiling(LogicalSize * bitmapScale));
        // A new bitmap is transparent: the card is painted beneath it on each frame.
        bitmap = new Bitmap(backingSize, backingSize);
        bitmap.Commit();
        PointerDown += e => BrushDown(e.X, e.Y);
        PointerMove += e => BrushMove(e.X, e.Y);
        PointerUp += _ => BrushUp();
        PointerCancel += _ => BrushUp();
    }

    public void SetHintLabel(Node label)
    {
        hintLabel = label;
        hintBaked = false;
        InvalidateVisual();
    }

    public override void Dispose()
    {
        bitmap.Dispose();
        base.Dispose();
    }

    protected override void OnRender(DrawContext context)
    {
        var size = GetBounds().Width;
        context.DrawRoundRect(0.0f, 0.0f, size, size, 12.0f, 12.0f, Paint.Fill(DrawingColors.Card));
        context.DrawImage(bitmap.TextureId, 0.0f, 0.0f, size, size);
        if (!hintBaked && hintLabel is not null)
        {
            if (bitmap.Render(hintLabel, 16.0f, 32.0f, bitmapScale))
            {
                bitmap.Commit();
                hintBaked = true;
            }
            else
            {
                InvalidateVisual();
            }
        }
        // Corner brackets mark the drawable area.
        var bracket = DrawingColors.Rgba(150, 150, 170, 120);
        const float length = 16.0f;
        const float margin = 8.0f;
        context.DrawLine(margin, length, margin, margin, bracket, 1.5f);
        context.DrawLine(margin, margin, length, margin, bracket, 1.5f);
        context.DrawLine(size - length, margin, size - margin, margin, bracket, 1.5f);
        context.DrawLine(size - margin, margin, size - margin, length, bracket, 1.5f);
        context.DrawLine(margin, size - length, margin, size - margin, bracket, 1.5f);
        context.DrawLine(margin, size - margin, length, size - margin, bracket, 1.5f);
        context.DrawLine(size - length, size - margin, size - margin, size - margin, bracket, 1.5f);
        context.DrawLine(size - margin, size - margin, size - margin, size - length, bracket, 1.5f);
    }

    private void BrushDown(float x, float y)
    {
        CapturePointer();
        painting = true;
        Dab(x, y);
    }

    private void BrushMove(float x, float y)
    {
        if (painting)
        {
            Dab(x, y);
        }
    }

    private void BrushUp()
    {
        painting = false;
        ReleasePointer();
    }

    private void Dab(float x, float y)
    {
        var centerX = (int)Math.Round(x * bitmapScale);
        var centerY = (int)Math.Round(y * bitmapScale);
        var radius = (int)Math.Max(1.0, Math.Round(BrushRadius * bitmapScale));
        var color = DrawingColors.Needle;
        var pixels = bitmap.Pixels;
        var width = bitmap.Width;
        var height = bitmap.Height;
        for (var dy = -radius; dy <= radius; dy += 1)
        {
            for (var dx = -radius; dx <= radius; dx += 1)
            {
                var px = centerX + dx;
                var py = centerY + dy;
                if ((dx * dx) + (dy * dy) > radius * radius || px < 0 || px >= width || py < 0 || py >= height)
                {
                    continue;
                }
                var index = ((py * width) + px) * 4;
                pixels[index] = color.R;
                pixels[index + 1] = color.G;
                pixels[index + 2] = color.B;
                pixels[index + 3] = 0xFF;
            }
        }
        bitmap.ClearDirtyRects();
        bitmap.Commit();
        InvalidateVisual();
    }
}
