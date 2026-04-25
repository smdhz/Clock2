namespace ClockApp;

public sealed class ClockState
{
    private static readonly TimeSpan SecondJumpDuration = TimeSpan.FromMilliseconds(85);

    private DateTime _secondJumpStartedAt;
    private double _secondStartAngle;
    private double _secondTargetAngle;
    private double _settledSecondAngle;
    private int _lastSecond = -1;

    public double HourAngle { get; private set; }

    public double MinuteAngle { get; private set; }

    public double SecondAngle { get; private set; }

    public bool ShowSecondHand { get; set; } = true;

    public void Update(DateTime now)
    {
        HourAngle = (now.Hour % 12) * 30.0 + now.Minute * 0.5;
        MinuteAngle = now.Minute * 6.0 + now.Second * 0.1;

        if (_lastSecond < 0)
        {
            _lastSecond = now.Second;
            _settledSecondAngle = now.Second * 6.0;
            _secondStartAngle = _settledSecondAngle;
            _secondTargetAngle = _settledSecondAngle;
            _secondJumpStartedAt = now;
            SecondAngle = _settledSecondAngle;
            return;
        }

        if (now.Second != _lastSecond)
        {
            StartSecondJump(now);
        }

        SecondAngle = GetCurrentSecondAngle(now);
    }

    private void StartSecondJump(DateTime now)
    {
        var previousSecond = _lastSecond;
        _lastSecond = now.Second;

        var expectedNextSecond = (previousSecond + 1) % 60;
        if (now.Second != expectedNextSecond)
        {
            _settledSecondAngle = now.Second * 6.0;
            _secondStartAngle = _settledSecondAngle;
            _secondTargetAngle = _settledSecondAngle;
            _secondJumpStartedAt = now;
            SecondAngle = _settledSecondAngle;
            return;
        }

        _secondStartAngle = _settledSecondAngle;
        _secondTargetAngle = now.Second * 6.0;
        if (previousSecond == 59 && now.Second == 0)
        {
            _secondTargetAngle = 360.0;
        }

        _secondJumpStartedAt = now;
    }

    private double GetCurrentSecondAngle(DateTime now)
    {
        var elapsed = now - _secondJumpStartedAt;
        var progress = Math.Clamp(elapsed.TotalMilliseconds / SecondJumpDuration.TotalMilliseconds, 0.0, 1.0);

        if (progress >= 1.0)
        {
            _settledSecondAngle = NormalizeAngle(_lastSecond * 6.0);
            return _settledSecondAngle;
        }

        var eased = EaseOutBack(progress);
        return NormalizeAngle(_secondStartAngle + (_secondTargetAngle - _secondStartAngle) * eased);
    }

    private static double EaseOutBack(double t)
    {
        const double overshoot = 0.45;
        var shifted = t - 1.0;
        return 1.0 + shifted * shifted * ((overshoot + 1.0) * shifted + overshoot);
    }

    private static double NormalizeAngle(double angle)
    {
        angle %= 360.0;
        return angle < 0.0 ? angle + 360.0 : angle;
    }
}
