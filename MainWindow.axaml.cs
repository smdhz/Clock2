using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace ClockApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = 0;
        ContextMenu = CreateContextMenu();
        PointerPressed += OnPointerPressed;
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
                exitItem
            }
        };
    }
}
