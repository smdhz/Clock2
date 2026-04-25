using Avalonia;
using Avalonia.Media;

namespace ClockApp;

public sealed class ClockRenderer
{
    public void Render(DrawingContext context, Size availableSize, ClockState state, bool isDarkTheme)
    {
        var side = Math.Min(availableSize.Width, availableSize.Height);
        if (side <= 0.0)
        {
            return;
        }

        var center = new Point(availableSize.Width / 2.0, availableSize.Height / 2.0);
        var outerRadius = side * 0.468;
        var faceRadius = outerRadius * 0.994;

        DrawOuterShadow(context, center, outerRadius);
        DrawDial(context, center, faceRadius, isDarkTheme);
        DrawMetalCase(context, center, outerRadius, isDarkTheme);
        DrawTicks(context, center, faceRadius, isDarkTheme);
        DrawHands(context, center, faceRadius, state, isDarkTheme);
        DrawCenterCap(context, center, faceRadius, isDarkTheme);
        DrawGlass(context, center, faceRadius, isDarkTheme);
    }

    private static void DrawOuterShadow(DrawingContext context, Point center, double radius)
    {
        context.DrawEllipse(
            Brush(44, 0, 0, 0),
            null,
            new Point(center.X + radius * 0.030, center.Y + radius * 0.050),
            radius * 1.012,
            radius * 1.012);

        context.DrawEllipse(
            Brush(28, 0, 0, 0),
            null,
            new Point(center.X + radius * 0.018, center.Y + radius * 0.030),
            radius * 1.045,
            radius * 1.045);

        context.DrawEllipse(
            Brush(54, 0, 0, 0),
            null,
            new Point(center.X + radius * 0.020, center.Y + radius * 0.030),
            radius * 0.998,
            radius * 0.998);

        context.DrawEllipse(
            Brush(18, 255, 255, 255),
            null,
            new Point(center.X - radius * 0.020, center.Y - radius * 0.025),
            radius * 0.985,
            radius * 0.985);
    }

    private static void DrawMetalCase(DrawingContext context, Point center, double radius, bool isDarkTheme)
    {
        var outer = isDarkTheme ? Brush(155, 166, 176, 182) : Brush(165, 142, 154, 160);
        var inner = isDarkTheme ? Brush(82, 255, 255, 255) : Brush(110, 255, 255, 255);
        var shade = isDarkTheme ? Brush(74, 0, 0, 0) : Brush(58, 50, 62, 68);

        context.DrawEllipse(null, new Pen(outer, Math.Max(1.0, radius * 0.006)), center, radius * 0.997, radius * 0.997);
        context.DrawEllipse(null, new Pen(inner, Math.Max(0.75, radius * 0.003)), center, radius * 0.981, radius * 0.981);
        context.DrawEllipse(null, new Pen(shade, Math.Max(0.75, radius * 0.003)), center, radius * 0.963, radius * 0.963);
    }

    private static void DrawDial(DrawingContext context, Point center, double radius, bool isDarkTheme)
    {
        var face = new RadialGradientBrush
        {
            Center = new RelativePoint(0.44, 0.34, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.30, 0.20, RelativeUnit.Relative),
            RadiusX = Relative(0.78),
            RadiusY = Relative(0.82),
            GradientStops =
            {
                new GradientStop(isDarkTheme ? Color.FromRgb(75, 88, 98) : Color.FromRgb(248, 252, 254), 0.00),
                new GradientStop(isDarkTheme ? Color.FromRgb(39, 50, 60) : Color.FromRgb(226, 236, 243), 0.48),
                new GradientStop(isDarkTheme ? Color.FromRgb(17, 24, 31) : Color.FromRgb(159, 178, 190), 0.82),
                new GradientStop(isDarkTheme ? Color.FromRgb(7, 10, 14) : Color.FromRgb(88, 105, 118), 1.00)
            }
        };

        context.DrawEllipse(face, null, center, radius, radius);
        context.DrawEllipse(null, new Pen(Brush(isDarkTheme ? (byte)70 : (byte)120, 255, 255, 255), radius * 0.012), center, radius * 0.985, radius * 0.985);

        var innerGlow = new RadialGradientBrush
        {
            Center = RelativePoint.Center,
            GradientOrigin = new RelativePoint(0.50, 0.42, RelativeUnit.Relative),
            RadiusX = Relative(0.62),
            RadiusY = Relative(0.62),
            GradientStops =
            {
                new GradientStop(isDarkTheme ? Color.FromArgb(42, 255, 255, 255) : Color.FromArgb(58, 255, 255, 255), 0.0),
                new GradientStop(isDarkTheme ? Color.FromArgb(10, 255, 255, 255) : Color.FromArgb(16, 255, 255, 255), 0.62),
                new GradientStop(isDarkTheme ? Color.FromArgb(82, 0, 0, 0) : Color.FromArgb(54, 33, 45, 56), 1.0)
            }
        };
        context.DrawEllipse(innerGlow, null, center, radius * 0.90, radius * 0.90);
    }

    private static void DrawTicks(DrawingContext context, Point center, double radius, bool isDarkTheme)
    {
        for (var i = 0; i < 60; i++)
        {
            var major = i % 5 == 0;
            var angle = i * 6.0;
            var outer = PointOnAngle(center, radius * 0.86, angle);
            var inner = PointOnAngle(center, radius * (major ? 0.72 : 0.79), angle);
            var width = major ? radius * 0.030 : radius * 0.010;
            var alpha = major ? (byte)185 : (byte)88;
            var color = isDarkTheme ? Brush(alpha, 220, 230, 236) : Brush(alpha, 28, 39, 48);

            context.DrawLine(new Pen(color, width), inner, outer);
            if (major)
            {
                var highlightInner = PointOnAngle(center, radius * 0.715, angle);
                var highlightOuter = PointOnAngle(center, radius * 0.805, angle);
                context.DrawLine(new Pen(Brush(isDarkTheme ? (byte)30 : (byte)75, 255, 255, 255), width * 0.42), highlightInner, highlightOuter);
            }
        }
    }

    private static void DrawHands(DrawingContext context, Point center, double radius, ClockState state, bool isDarkTheme)
    {
        var handBrush = isDarkTheme ? Brush(245, 232, 238, 241) : Brush(245, 18, 23, 28);

        DrawLineHandShadow(context, center, state.HourAngle, radius * 0.48, radius * 0.040);
        DrawLineHandShadow(context, center, state.MinuteAngle, radius * 0.68, radius * 0.026);

        DrawLineHand(context, center, state.HourAngle, radius * 0.48, radius * 0.036, handBrush);
        DrawLineHand(context, center, state.MinuteAngle, radius * 0.68, radius * 0.024, handBrush);

        if (state.ShowSecondHand)
        {
            DrawSecondHand(context, center, state.SecondAngle, radius);
        }
    }

    private static void DrawLineHandShadow(DrawingContext context, Point center, double angle, double length, double width)
    {
        var offset = Math.Max(1.0, width * 0.75);
        var shadowStart = new Point(center.X + offset, center.Y + offset * 1.2);
        var shadowEnd = PointOnAngle(shadowStart, length, angle);
        context.DrawLine(RoundPen(Brush(70, 0, 0, 0), width * 1.08), shadowStart, shadowEnd);
    }

    private static void DrawLineHand(DrawingContext context, Point center, double angle, double length, double width, IBrush brush)
    {
        var end = PointOnAngle(center, length, angle);
        context.DrawLine(RoundPen(brush, width), center, end);
    }

    private static void DrawSecondHand(DrawingContext context, Point center, double angle, double radius)
    {
        var tip = PointOnAngle(center, radius * 0.78, angle);
        var tail = PointOnAngle(center, radius * 0.21, angle + 180.0);
        var shadowTip = new Point(tip.X + radius * 0.010, tip.Y + radius * 0.014);
        var shadowTail = new Point(tail.X + radius * 0.010, tail.Y + radius * 0.014);

        context.DrawLine(new Pen(Brush(75, 0, 0, 0), Math.Max(1.0, radius * 0.018)), shadowTail, shadowTip);
        context.DrawLine(new Pen(Brush(245, 178, 14, 20), Math.Max(1.0, radius * 0.010)), tail, tip);
        context.DrawEllipse(Brush(245, 178, 14, 20), null, center, radius * 0.035, radius * 0.035);
    }

    private static void DrawCenterCap(DrawingContext context, Point center, double radius, bool isDarkTheme)
    {
        context.DrawEllipse(Brush(62, 0, 0, 0), null, new Point(center.X + radius * 0.014, center.Y + radius * 0.018), radius * 0.050, radius * 0.050);

        var cap = new RadialGradientBrush
        {
            Center = new RelativePoint(0.42, 0.34, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.32, 0.24, RelativeUnit.Relative),
            RadiusX = Relative(0.82),
            RadiusY = Relative(0.82),
            GradientStops =
            {
                new GradientStop(isDarkTheme ? Color.FromRgb(238, 244, 247) : Color.FromRgb(255, 255, 255), 0.00),
                new GradientStop(isDarkTheme ? Color.FromRgb(126, 139, 148) : Color.FromRgb(180, 194, 202), 0.45),
                new GradientStop(isDarkTheme ? Color.FromRgb(42, 50, 58) : Color.FromRgb(70, 84, 94), 1.00)
            }
        };

        context.DrawEllipse(cap, new Pen(Brush(90, 255, 255, 255), radius * 0.006), center, radius * 0.043, radius * 0.043);
        context.DrawEllipse(Brush(115, 255, 255, 255), null, new Point(center.X - radius * 0.012, center.Y - radius * 0.014), radius * 0.010, radius * 0.007);
    }

    private static void DrawGlass(DrawingContext context, Point center, double radius, bool isDarkTheme)
    {
        var faceRect = new Rect(center.X - radius, center.Y - radius, radius * 2.0, radius * 2.0);
        using (context.PushGeometryClip(new EllipseGeometry(faceRect)))
        {
            var upperGlass = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.35, 0.00, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.62, 1.00, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(isDarkTheme ? Color.FromArgb(72, 255, 255, 255) : Color.FromArgb(122, 255, 255, 255), 0.00),
                    new GradientStop(isDarkTheme ? Color.FromArgb(32, 255, 255, 255) : Color.FromArgb(58, 255, 255, 255), 0.40),
                    new GradientStop(Color.FromArgb(5, 255, 255, 255), 1.00)
                }
            };

            var highlightRect = new Rect(
                center.X - radius * 0.72,
                center.Y - radius * 0.91,
                radius * 1.28,
                radius * 0.95);
            context.DrawEllipse(upperGlass, null, highlightRect);

            var lowerShade = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.50, 0.00, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.50, 1.00, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb(0, 10, 18, 24), 0.00),
                    new GradientStop(isDarkTheme ? Color.FromArgb(58, 0, 0, 0) : Color.FromArgb(42, 10, 18, 24), 1.00)
                }
            };
            context.DrawEllipse(lowerShade, null, new Rect(center.X - radius, center.Y - radius * 0.05, radius * 2.0, radius * 1.05));
        }
    }

    private static Point PointOnAngle(Point center, double length, double angle)
    {
        var direction = Direction(angle);
        return new Point(center.X + direction.X * length, center.Y + direction.Y * length);
    }

    private static Point Direction(double angle)
    {
        var radians = (angle - 90.0) * Math.PI / 180.0;
        return new Point(Math.Cos(radians), Math.Sin(radians));
    }

    private static SolidColorBrush Brush(byte alpha, byte red, byte green, byte blue)
    {
        return new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
    }

    private static Pen RoundPen(IBrush brush, double thickness)
    {
        return new Pen(brush, thickness, null, PenLineCap.Round, PenLineJoin.Round, 10.0);
    }

    private static RelativeScalar Relative(double scalar)
    {
        return new RelativeScalar(scalar, RelativeUnit.Relative);
    }
}
