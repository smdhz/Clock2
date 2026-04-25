using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Clock2;

public sealed class ClockControl : Control
{
    private static readonly TimeSpan FrameInterval = TimeSpan.FromMilliseconds(16);

    private readonly ClockRenderer _renderer = new();
    private readonly ClockState _state = new();
    private readonly DispatcherTimer _timer;

    public ClockControl()
    {
        ClipToBounds = false;

        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = FrameInterval
        };
        _timer.Tick += OnFrame;

        AttachedToVisualTree += (_, _) =>
        {
            _state.Update(DateTime.Now);
            _timer.Start();
            InvalidateVisual();
        };

        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    public bool ShowSecondHand
    {
        get => _state.ShowSecondHand;
        set
        {
            _state.ShowSecondHand = value;
            InvalidateVisual();
        }
    }

    public override void Render(Avalonia.Media.DrawingContext context)
    {
        base.Render(context);
        _renderer.Render(context, Bounds.Size, _state, ActualThemeVariant == ThemeVariant.Dark);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? 210.0 : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? 210.0 : availableSize.Height;
        var side = Math.Min(width, height);
        return new Size(side, side);
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        _state.Update(DateTime.Now);
        InvalidateVisual();
    }
}
