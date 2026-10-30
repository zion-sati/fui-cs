using System;

using FuiCs;

namespace FuiCs.Demo.ImmediateDrawing;

/// <summary>
/// A strand drawn through 3D value noise that drifts on every tick; dragging pulls the strand toward the pointer, and
/// the pull eases off after release.
/// </summary>
internal sealed class DancingYarn : CustomDrawable
{
    private static readonly Color BeadPink = DrawingColors.Rgba(255, 114, 168, 230);
    private static readonly Color BeadGold = DrawingColors.Rgba(255, 216, 111, 230);

    private readonly TextLayout title;
    private float noiseX;
    private float noiseY = 2.7f;
    private float noiseZ = 5.1f;
    private float directionX = 1.0f;
    private float directionY = 1.0f;
    private float directionZ = -1.0f;
    private bool dragging;
    private float pointerX = 150.0f;
    private float pointerY = 150.0f;
    private float pull;

    public DancingYarn()
    {
        this.Width(300.0f).Height(300.0f)
            .SemanticRole(SemanticRole.Image)
            .SemanticLabel("Dancing yarn interactive noise panel")
            .NodeId("widget-yarn");
        title = PlotText.Title(this, "Dancing yarn");
        PointerDown += e => OnPointerDown(e.X, e.Y);
        PointerMove += e => OnPointerMove(e.X, e.Y);
        PointerUp += _ => OnPointerUp();
        PointerCancel += _ => OnPointerUp();
    }

    public void Tick()
    {
        noiseX += 0.032f * directionX;
        noiseY += 0.021f * directionY;
        noiseZ += 0.027f * directionZ;
        Bounce(ref noiseX, ref directionX);
        Bounce(ref noiseY, ref directionY);
        Bounce(ref noiseZ, ref directionZ);
        if (!dragging && pull > 0.0f)
        {
            pull *= 0.88f;
            if (pull < 0.02f)
            {
                pull = 0.0f;
            }
        }
        InvalidateVisual();
    }

    protected override void OnRender(DrawContext context)
    {
        var size = GetBounds().Width;
        var centerX = size / 2.0f;
        var centerY = (size / 2.0f) + 8.0f;
        const int steps = 132;
        var pointerBiasY = ((pointerX / size) - 0.5f) * 1.7f * pull;
        var pointerBiasZ = ((pointerY / size) - 0.5f) * 1.7f * pull;

        context.DrawRoundRect(0.0f, 0.0f, size, size, 12.0f, 12.0f, Paint.Fill(DrawingColors.Card));
        PlotText.DrawTitle(context, title);
        context.Save();
        context.ClipRoundRect(0.0f, 0.0f, size, size, 12.0f);
        var ringAlpha = (byte)Math.Round(36.0 + (70.0 * pull));
        context.DrawCircle(pointerX, pointerY, 22.0f + (10.0f * pull), Paint.Stroke(DrawingColors.Rgba(255, 255, 255, ringAlpha), 1.0f));

        var previousX = 0.0f;
        var previousY = 0.0f;
        for (var i = 0; i < steps; i += 1)
        {
            var t = (float)i / (steps - 1);
            var xBase = 28.0f + (t * (size - 56.0f));
            var centered = (t - 0.5f) * 2.0f;
            var envelope = 1.0f - (centered * centered);
            var n0 = YarnNoise.Value(t * 4.2f + noiseX, noiseY + pointerBiasY, noiseZ + pointerBiasZ);
            var n1 = YarnNoise.Value(t * 5.6f + noiseX + 6.0f, noiseY + 3.0f + pointerBiasY, noiseZ + 1.4f + pointerBiasZ);
            var n2 = YarnNoise.Value(t * 7.3f + noiseX + 2.0f, noiseY + 8.0f + pointerBiasY, noiseZ + 4.0f + pointerBiasZ);
            var angle = ((n0 * 2.0f) - 1.0f) * MathF.PI * 1.35f;
            var radius = (24.0f + (n1 * 46.0f)) * (0.35f + (envelope * 0.95f));
            var x = xBase + ((float)Math.Cos(angle) * radius * 0.72f);
            var y = centerY + ((float)Math.Sin(angle) * radius) + ((n2 - 0.5f) * 42.0f);

            var toPointerX = pointerX - x;
            var toPointerY = pointerY - y;
            var distance = (float)Math.Sqrt((toPointerX * toPointerX) + (toPointerY * toPointerY));
            var influence = Math.Max(0.0f, 1.0f - (distance / 145.0f));
            influence = influence * influence * pull;
            x += toPointerX * influence * 0.62f;
            y += toPointerY * influence * 0.62f;
            x += (float)Math.Sin((t * 18.0f) + noiseZ) * influence * 20.0f;
            y += (float)Math.Cos((t * 17.0f) + noiseY) * influence * 20.0f;

            if (i > 0)
            {
                var stroke = 1.3f + (n2 * 2.2f) + (influence * 1.4f);
                context.DrawLine(previousX, previousY, x, y, YarnNoise.Color(t + (noiseX * 0.07f)), stroke);
            }
            if (i % 19 == 0)
            {
                context.DrawCircle(x, y, 2.2f + (n1 * 2.2f), Paint.Fill(i % 38 == 0 ? BeadGold : BeadPink));
            }
            previousX = x;
            previousY = y;
        }

        context.DrawCircle(centerX, centerY, 76.0f, Paint.Stroke(DrawingColors.Rgba(255, 255, 255, 22), 1.0f));
        context.Restore();
    }

    private static void Bounce(ref float value, ref float direction)
    {
        if (value > 9.0f)
        {
            value = 9.0f;
            direction = -1.0f;
        }
        if (value < 0.0f)
        {
            value = 0.0f;
            direction = 1.0f;
        }
    }

    private void OnPointerDown(float x, float y)
    {
        CapturePointer();
        dragging = true;
        pointerX = x;
        pointerY = y;
        pull = 1.0f;
        InvalidateVisual();
    }

    private void OnPointerMove(float x, float y)
    {
        if (!dragging)
        {
            return;
        }
        pointerX = x;
        pointerY = y;
        pull = 1.0f;
        InvalidateVisual();
    }

    private void OnPointerUp()
    {
        dragging = false;
        ReleasePointer();
        InvalidateVisual();
    }
}
