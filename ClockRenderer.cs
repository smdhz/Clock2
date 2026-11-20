using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Clock2;

public sealed class ClockRenderer
{
    // Theme- and size-independent brushes.
    // Soft drop shadow: dark under the case, fading smoothly to nothing at the edge.
    private static readonly IImmutableBrush OuterShadow = new RadialGradientBrush
    {
        GradientStops =
        {
            new GradientStop(Color.FromArgb(70, 0, 0, 0), 0.00),
            new GradientStop(Color.FromArgb(62, 0, 0, 0), 0.86),
            new GradientStop(Color.FromArgb(26, 0, 0, 0), 0.93),
            new GradientStop(Color.FromArgb(0, 0, 0, 0), 1.00)
        }
    }.ToImmutable();
    private static readonly IImmutableBrush HandShadow = Brush(70, 0, 0, 0);
    private static readonly IImmutableBrush SecondShadow = Brush(75, 0, 0, 0);
    private static readonly IImmutableBrush SecondRed = Brush(245, 178, 14, 20);
    private static readonly IImmutableBrush CapShadow = Brush(62, 0, 0, 0);
    private static readonly IImmutableBrush CapGlint = Brush(115, 255, 255, 255);

    private const double HourLength = 0.52;
    private const double MinuteLength = 0.81;

    private Resources? _resources;

    public void Render(DrawingContext context, Size availableSize, ClockState state, bool showSecondHand, bool isDarkTheme)
    {
        if (Math.Min(availableSize.Width, availableSize.Height) <= 0.0)
        {
            return;
        }

        // Brushes, pens and geometry only change with size or theme, so build them once and reuse.
        if (_resources is null || _resources.Size != availableSize || _resources.IsDark != isDarkTheme)
        {
            _resources = new Resources(availableSize, isDarkTheme);
        }

        var r = _resources;
        DrawOuterShadow(context, r);
        DrawDial(context, r);
        DrawMetalCase(context, r);
        DrawTicks(context, r);
        DrawHands(context, r, state, showSecondHand);
        DrawCenterCap(context, r);
        DrawGlass(context, r);
    }

    private static void DrawOuterShadow(DrawingContext context, Resources r)
    {
        var radius = r.OuterRadius;
        var shadowCenter = new Point(r.Center.X + radius * 0.015, r.Center.Y + radius * 0.030);
        context.DrawEllipse(OuterShadow, null, shadowCenter, radius * 1.035, radius * 1.035);
    }

    private static void DrawMetalCase(DrawingContext context, Resources r)
    {
        var radius = r.OuterRadius;
        var bezelMid = (radius + r.GlassRadius) / 2.0;
        context.DrawEllipse(null, r.BezelPen, r.Center, bezelMid, bezelMid);
        context.DrawEllipse(null, r.CaseOuterPen, r.Center, radius * 0.997, radius * 0.997);
        context.DrawEllipse(null, r.BezelInnerShadePen, r.Center, r.GlassRadius, r.GlassRadius);
    }

    private static void DrawDial(DrawingContext context, Resources r)
    {
        var radius = r.FaceRadius;
        context.DrawEllipse(r.DialFace, null, r.Center, radius, radius);
        context.DrawEllipse(r.DialInnerGlow, null, r.Center, radius * 0.90, radius * 0.90);
    }

    private static void DrawTicks(DrawingContext context, Resources r)
    {
        var radius = r.FaceRadius;
        for (var i = 0; i < 60; i++)
        {
            var major = i % 5 == 0;
            var angle = i * 6.0;
            var outer = PointOnAngle(r.Center, radius * 0.86, angle);
            var inner = PointOnAngle(r.Center, radius * (major ? 0.72 : 0.79), angle);

            context.DrawLine(major ? r.MajorTickPen : r.MinorTickPen, inner, outer);
        }
    }

    private static void DrawHands(DrawingContext context, Resources r, ClockState state, bool showSecondHand)
    {
        var radius = r.FaceRadius;

        DrawLineHand(context, r.HourShadowOrigin, state.HourAngle, HourLength * radius, r.HourShadowPen);
        DrawLineHand(context, r.MinuteShadowOrigin, state.MinuteAngle, MinuteLength * radius, r.MinuteShadowPen);

        DrawLineHand(context, r.Center, state.HourAngle, HourLength * radius, r.HourPen);
        DrawLineHand(context, r.Center, state.MinuteAngle, MinuteLength * radius, r.MinutePen);

        if (showSecondHand)
        {
            DrawSecondHand(context, r, state.SecondAngle);
        }
    }

    private static void DrawLineHand(DrawingContext context, Point origin, double angle, double length, IPen pen)
    {
        context.DrawLine(pen, origin, PointOnAngle(origin, length, angle));
    }

    private static void DrawSecondHand(DrawingContext context, Resources r, double angle)
    {
        var radius = r.FaceRadius;
        var tip = PointOnAngle(r.Center, radius * 0.78, angle);
        var tail = PointOnAngle(r.Center, radius * 0.21, angle + 180.0);
        var shadowOffset = new Vector(radius * 0.010, radius * 0.014);

        context.DrawLine(r.SecondShadowPen, tail + shadowOffset, tip + shadowOffset);
        context.DrawLine(r.SecondPen, tail, tip);
        context.DrawEllipse(SecondRed, null, r.Center, radius * 0.035, radius * 0.035);
    }

    private static void DrawCenterCap(DrawingContext context, Resources r)
    {
        var c = r.Center;
        var radius = r.FaceRadius;

        context.DrawEllipse(CapShadow, null, new Point(c.X + radius * 0.014, c.Y + radius * 0.018), radius * 0.050, radius * 0.050);
        context.DrawEllipse(r.Cap, r.CapPen, c, radius * 0.043, radius * 0.043);
        context.DrawEllipse(CapGlint, null, new Point(c.X - radius * 0.012, c.Y - radius * 0.014), radius * 0.010, radius * 0.007);
    }

    private static void DrawGlass(DrawingContext context, Resources r)
    {
        using (context.PushGeometryClip(r.FaceClip))
        {
            context.DrawEllipse(r.UpperGlass, null, r.UpperGlassRect);
            context.DrawEllipse(r.LowerShade, null, r.LowerShadeRect);
        }
    }

    private static Point PointOnAngle(Point center, double length, double angle)
    {
        var radians = (angle - 90.0) * Math.PI / 180.0;
        return new Point(center.X + Math.Cos(radians) * length, center.Y + Math.Sin(radians) * length);
    }

    private static IImmutableBrush Brush(byte alpha, byte red, byte green, byte blue)
    {
        return new ImmutableSolidColorBrush(Color.FromArgb(alpha, red, green, blue));
    }

    private static ImmutablePen Pen(IImmutableBrush brush, double thickness)
    {
        return new ImmutablePen(brush, thickness);
    }

    private static ImmutablePen RoundPen(IImmutableBrush brush, double thickness)
    {
        return new ImmutablePen(brush, thickness, null, PenLineCap.Round, PenLineJoin.Round, 10.0);
    }

    private static RelativeScalar Relative(double scalar)
    {
        return new RelativeScalar(scalar, RelativeUnit.Relative);
    }

    private sealed class Resources
    {
        public Resources(Size size, bool isDark)
        {
            Size = size;
            IsDark = isDark;

            var side = Math.Min(size.Width, size.Height);
            Center = new Point(size.Width / 2.0, size.Height / 2.0);
            OuterRadius = side * 0.468;
            FaceRadius = OuterRadius * 0.994;

            var R = OuterRadius;
            var r = FaceRadius;

            // Metal case
            GlassRadius = R * 0.93;
            CaseOuterPen = Pen(isDark ? Brush(200, 40, 46, 52) : Brush(170, 112, 124, 132), Math.Max(1.0, R * 0.008));
            BezelPen = Pen(new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.15, 0.05, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.85, 0.95, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(isDark ? Color.FromRgb(196, 205, 211) : Color.FromRgb(250, 252, 253), 0.00),
                    new GradientStop(isDark ? Color.FromRgb(120, 130, 138) : Color.FromRgb(196, 206, 213), 0.45),
                    new GradientStop(isDark ? Color.FromRgb(66, 74, 81) : Color.FromRgb(150, 162, 170), 0.75),
                    new GradientStop(isDark ? Color.FromRgb(150, 160, 167) : Color.FromRgb(214, 222, 227), 1.00)
                }
            }.ToImmutable(), R - GlassRadius);
            BezelInnerShadePen = Pen(isDark ? Brush(150, 0, 0, 0) : Brush(110, 40, 52, 60), Math.Max(1.0, R * 0.012));

            // Dial
            DialFace = new RadialGradientBrush
            {
                Center = new RelativePoint(0.44, 0.34, RelativeUnit.Relative),
                GradientOrigin = new RelativePoint(0.30, 0.20, RelativeUnit.Relative),
                RadiusX = Relative(0.78),
                RadiusY = Relative(0.82),
                GradientStops =
                {
                    new GradientStop(isDark ? Color.FromRgb(75, 88, 98) : Color.FromRgb(248, 252, 254), 0.00),
                    new GradientStop(isDark ? Color.FromRgb(39, 50, 60) : Color.FromRgb(226, 236, 243), 0.48),
                    new GradientStop(isDark ? Color.FromRgb(17, 24, 31) : Color.FromRgb(159, 178, 190), 0.82),
                    new GradientStop(isDark ? Color.FromRgb(7, 10, 14) : Color.FromRgb(88, 105, 118), 1.00)
                }
            }.ToImmutable();
            DialInnerGlow = new RadialGradientBrush
            {
                Center = RelativePoint.Center,
                GradientOrigin = new RelativePoint(0.50, 0.42, RelativeUnit.Relative),
                RadiusX = Relative(0.62),
                RadiusY = Relative(0.62),
                GradientStops =
                {
                    new GradientStop(isDark ? Color.FromArgb(42, 255, 255, 255) : Color.FromArgb(58, 255, 255, 255), 0.0),
                    new GradientStop(isDark ? Color.FromArgb(10, 255, 255, 255) : Color.FromArgb(16, 255, 255, 255), 0.62),
                    new GradientStop(isDark ? Color.FromArgb(82, 0, 0, 0) : Color.FromArgb(54, 33, 45, 56), 1.0)
                }
            }.ToImmutable();

            // Ticks
            var majorTickBrush = isDark ? Brush(185, 220, 230, 236) : Brush(185, 28, 39, 48);
            var minorTickBrush = isDark ? Brush(88, 220, 230, 236) : Brush(88, 28, 39, 48);
            MajorTickPen = Pen(majorTickBrush, r * 0.030);
            MinorTickPen = Pen(minorTickBrush, r * 0.010);

            // Hands
            var handBrush = isDark ? Brush(245, 232, 238, 241) : Brush(245, 18, 23, 28);
            var hourWidth = r * 0.036;
            var minuteWidth = r * 0.024;
            var hourShadowWidth = r * 0.040;
            var minuteShadowWidth = r * 0.026;
            HourPen = RoundPen(handBrush, hourWidth);
            MinutePen = RoundPen(handBrush, minuteWidth);
            HourShadowPen = RoundPen(HandShadow, hourShadowWidth * 1.08);
            MinuteShadowPen = RoundPen(HandShadow, minuteShadowWidth * 1.08);
            HourShadowOrigin = ShadowOrigin(Center, hourShadowWidth);
            MinuteShadowOrigin = ShadowOrigin(Center, minuteShadowWidth);
            SecondShadowPen = Pen(SecondShadow, Math.Max(1.0, r * 0.018));
            SecondPen = Pen(SecondRed, Math.Max(1.0, r * 0.010));

            // Center cap
            Cap = new RadialGradientBrush
            {
                Center = new RelativePoint(0.42, 0.34, RelativeUnit.Relative),
                GradientOrigin = new RelativePoint(0.32, 0.24, RelativeUnit.Relative),
                RadiusX = Relative(0.82),
                RadiusY = Relative(0.82),
                GradientStops =
                {
                    new GradientStop(isDark ? Color.FromRgb(238, 244, 247) : Color.FromRgb(255, 255, 255), 0.00),
                    new GradientStop(isDark ? Color.FromRgb(126, 139, 148) : Color.FromRgb(180, 194, 202), 0.45),
                    new GradientStop(isDark ? Color.FromRgb(42, 50, 58) : Color.FromRgb(70, 84, 94), 1.00)
                }
            }.ToImmutable();
            CapPen = Pen(Brush(90, 255, 255, 255), r * 0.006);

            // Glass
            var g = GlassRadius;
            FaceClip = new EllipseGeometry(new Rect(Center.X - g, Center.Y - g, g * 2.0, g * 2.0));
            UpperGlass = new RadialGradientBrush
            {
                GradientOrigin = new RelativePoint(0.40, 0.22, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(isDark ? Color.FromArgb(64, 255, 255, 255) : Color.FromArgb(118, 255, 255, 255), 0.00),
                    new GradientStop(isDark ? Color.FromArgb(28, 255, 255, 255) : Color.FromArgb(52, 255, 255, 255), 0.55),
                    new GradientStop(Color.FromArgb(0, 255, 255, 255), 1.00)
                }
            }.ToImmutable();
            UpperGlassRect = new Rect(Center.X - g * 0.80, Center.Y - g * 0.98, g * 1.45, g * 1.05);
            LowerShade = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.50, 0.00, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.50, 1.00, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb(0, 10, 18, 24), 0.00),
                    new GradientStop(isDark ? Color.FromArgb(58, 0, 0, 0) : Color.FromArgb(42, 10, 18, 24), 1.00)
                }
            }.ToImmutable();
            LowerShadeRect = new Rect(Center.X - g, Center.Y - g * 0.05, g * 2.0, g * 1.05);
        }

        public Size Size { get; }
        public bool IsDark { get; }
        public Point Center { get; }
        public double OuterRadius { get; }
        public double FaceRadius { get; }

        public double GlassRadius { get; }
        public ImmutablePen CaseOuterPen { get; }
        public ImmutablePen BezelPen { get; }
        public ImmutablePen BezelInnerShadePen { get; }

        public IImmutableBrush DialFace { get; }
        public IImmutableBrush DialInnerGlow { get; }

        public ImmutablePen MajorTickPen { get; }
        public ImmutablePen MinorTickPen { get; }

        public ImmutablePen HourPen { get; }
        public ImmutablePen MinutePen { get; }
        public ImmutablePen HourShadowPen { get; }
        public ImmutablePen MinuteShadowPen { get; }
        public Point HourShadowOrigin { get; }
        public Point MinuteShadowOrigin { get; }
        public ImmutablePen SecondShadowPen { get; }
        public ImmutablePen SecondPen { get; }

        public IImmutableBrush Cap { get; }
        public ImmutablePen CapPen { get; }

        public Geometry FaceClip { get; }
        public IImmutableBrush UpperGlass { get; }
        public Rect UpperGlassRect { get; }
        public IImmutableBrush LowerShade { get; }
        public Rect LowerShadeRect { get; }

        private static Point ShadowOrigin(Point center, double handWidth)
        {
            var offset = Math.Max(1.0, handWidth * 0.75);
            return new Point(center.X + offset, center.Y + offset * 1.2);
        }
    }
}
