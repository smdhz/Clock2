using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Clock2;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings = AppSettings.Load();
    private readonly ClockControl _clock;

    public MainWindow()
    {
        InitializeComponent();
        _clock = this.FindControl<ClockControl>("Clock")!;

        Topmost = _settings.Topmost;
        _clock.ShowSecondHand = _settings.ShowSecondHand;
        if (_settings is { X: int x, Y: int y })
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = new PixelPoint(x, y);
        }

        ContextMenu = CreateContextMenu();
        PointerPressed += OnPointerPressed;
        KeyDown += OnKeyDown;
        Opened += (_, _) => EnsureOnScreen();
        Closing += (_, _) => SaveSettings();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    // A saved position can point at a monitor that is no longer connected.
    private void EnsureOnScreen()
    {
        var size = PixelSize.FromSize(ClientSize, DesktopScaling);
        var middle = new PixelPoint(Position.X + size.Width / 2, Position.Y + size.Height / 2);
        if (Screens.ScreenFromPoint(middle) is not null || Screens.Primary is not { } primary)
        {
            return;
        }

        var area = primary.WorkingArea;
        Position = new PixelPoint(
            area.X + (area.Width - size.Width) / 2,
            area.Y + (area.Height - size.Height) / 2);
    }

    private void SaveSettings()
    {
        _settings.X = Position.X;
        _settings.Y = Position.Y;
        _settings.Topmost = Topmost;
        _settings.ShowSecondHand = _clock.ShowSecondHand;
        _settings.Save();
    }

    private ContextMenu CreateContextMenu()
    {
        var topmostItem = new MenuItem
        {
            Header = "Always on Top",
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = Topmost
        };
        topmostItem.Click += (_, _) =>
        {
            Topmost = topmostItem.IsChecked;
            SaveSettings();
        };

        var secondHandItem = new MenuItem
        {
            Header = "Show Second Hand",
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = _clock.ShowSecondHand
        };
        secondHandItem.Click += (_, _) =>
        {
            _clock.ShowSecondHand = secondHandItem.IsChecked;
            SaveSettings();
        };

        var exitItem = new MenuItem
        {
            Header = "Exit"
        };
        exitItem.Click += (_, _) => Close();

        return new ContextMenu
        {
            Items =
            {
                topmostItem,
                secondHandItem,
                new Separator(),
                exitItem
            }
        };
    }
}
