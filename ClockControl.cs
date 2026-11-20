using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Clock2;

public sealed class ClockControl : Control
{
    public static readonly StyledProperty<bool> ShowSecondHandProperty =
        AvaloniaProperty.Register<ClockControl, bool>(nameof(ShowSecondHand), true);

    private static readonly TimeSpan FrameInterval = TimeSpan.FromMilliseconds(16);

    // Wake slightly after the second boundary so DateTime.Now has already ticked over.
    private static readonly TimeSpan SecondBoundarySlack = TimeSpan.FromMilliseconds(2);

    private readonly ClockRenderer _renderer = new();
    private readonly ClockState _state = new();
    private readonly DispatcherTimer _timer;

    static ClockControl()
    {
        AffectsRender<ClockControl>(ShowSecondHandProperty);
    }

    public ClockControl()
    {
        ClipToBounds = false;

        _timer = new DispatcherTimer(DispatcherPriority.Render);
        _timer.Tick += (_, _) => Tick();

        ActualThemeVariantChanged += (_, _) => InvalidateVisual();
    }

    public bool ShowSecondHand
    {
        get => GetValue(ShowSecondHandProperty);
        set => SetValue(ShowSecondHandProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        _renderer.Render(context, Bounds.Size, _state, ShowSecondHand, ActualThemeVariant == ThemeVariant.Dark);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Tick();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? 210.0 : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? 210.0 : availableSize.Height;
        var side = Math.Min(width, height);
        return new Size(side, side);
    }

    private void Tick()
    {
        var now = DateTime.Now;
        _state.Update(now);
        InvalidateVisual();

        // Run at frame rate only during the short second-hand jump; otherwise sleep until the next second.
        _timer.Stop();
        _timer.Interval = _state.IsAnimating && ShowSecondHand
            ? FrameInterval
            : TimeSpan.FromTicks(TimeSpan.TicksPerSecond - now.Ticks % TimeSpan.TicksPerSecond) + SecondBoundarySlack;
        _timer.Start();
    }
}
