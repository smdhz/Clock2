using System.Diagnostics;

namespace Clock2;

public sealed class ClockState
{
    private static readonly TimeSpan SecondJumpDuration = TimeSpan.FromMilliseconds(85);

    private long _jumpStartedAt;
    private double _jumpFrom;
    private double _jumpTo;
    private int _lastSecond = -1;

    public double HourAngle { get; private set; }

    public double MinuteAngle { get; private set; }

    public double SecondAngle { get; private set; }

    /// <summary>True while the second hand is mid-jump and needs per-frame updates.</summary>
    public bool IsAnimating { get; private set; }

    public void Update(DateTime now)
    {
        HourAngle = (now.Hour % 12) * 30.0 + now.Minute * 0.5;
        MinuteAngle = now.Minute * 6.0 + now.Second * 0.1;

        if (now.Second != _lastSecond)
        {
            var isNextSecond = _lastSecond >= 0 && now.Second == (_lastSecond + 1) % 60;
            _lastSecond = now.Second;

            if (!isNextSecond)
            {
                // First update, skipped seconds, or the clock was adjusted: snap without animating.
                SecondAngle = now.Second * 6.0;
                IsAnimating = false;
                return;
            }

            // Start from where the hand currently is (even mid-jump) and take the short way round.
            _jumpFrom = SecondAngle;
            _jumpTo = SecondAngle + ShortestDelta(SecondAngle, now.Second * 6.0);
            _jumpStartedAt = Stopwatch.GetTimestamp();
            IsAnimating = true;
        }

        if (!IsAnimating)
        {
            return;
        }

        // Monotonic time, so wall-clock adjustments can't stall the animation.
        var progress = Stopwatch.GetElapsedTime(_jumpStartedAt) / SecondJumpDuration;
        if (progress >= 1.0)
        {
            SecondAngle = NormalizeAngle(_jumpTo);
            IsAnimating = false;
            return;
        }

        SecondAngle = NormalizeAngle(_jumpFrom + (_jumpTo - _jumpFrom) * EaseOutBack(progress));
    }

    private static double EaseOutBack(double t)
    {
        const double overshoot = 0.45;
        var shifted = t - 1.0;
        return 1.0 + shifted * shifted * ((overshoot + 1.0) * shifted + overshoot);
    }

    private static double ShortestDelta(double from, double to)
    {
        var delta = (to - from) % 360.0;
        if (delta < -180.0)
        {
            delta += 360.0;
        }
        else if (delta >= 180.0)
        {
            delta -= 360.0;
        }

        return delta;
    }

    private static double NormalizeAngle(double angle)
    {
        angle %= 360.0;
        return angle < 0.0 ? angle + 360.0 : angle;
    }
}
