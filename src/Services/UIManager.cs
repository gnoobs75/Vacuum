using Godot;

namespace Vacuum.Services;

/// <summary>
/// Singleton UI coordination service for managing UI state and overlays.
/// </summary>
public partial class UIManager : BaseService
{
    public static UIManager? Instance { get; private set; }

    public bool IsMenuOpen { get; private set; }
    public bool IsHUDVisible { get; set; } = true;

    [Signal] public delegate void MenuOpenedEventHandler(string menuName);
    [Signal] public delegate void MenuClosedEventHandler(string menuName);

    protected override void InitializeService()
    {
        Instance = this;
    }

    public void OpenMenu(string menuName)
    {
        IsMenuOpen = true;
        EmitSignal(SignalName.MenuOpened, menuName);
    }

    public void CloseMenu(string menuName)
    {
        IsMenuOpen = false;
        EmitSignal(SignalName.MenuClosed, menuName);
    }

    public void ToggleHUD()
    {
        IsHUDVisible = !IsHUDVisible;
    }
}
