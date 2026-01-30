using System;
using Godot;
using Vacuum.Data.Models;

namespace Vacuum.Services;

/// <summary>
/// Coordinates global game state: current player, character, session, game mode.
/// </summary>
public partial class GameStateManager : BaseService
{
    public static GameStateManager? Instance { get; private set; }

    public enum GameMode { MainMenu, Playing, Paused, Loading }

    public GameMode CurrentMode { get; private set; } = GameMode.Playing;
    public PlayerData CurrentPlayer { get; set; } = new() { Username = "Pilot" };
    public CharacterData CurrentCharacter { get; set; } = new() { Name = "Commander" };
    public string CurrentSystemId { get; set; } = "sol";
    public DateTime SessionStarted { get; private set; }

    [Signal] public delegate void GameModeChangedEventHandler(int mode);

    protected override void InitializeService()
    {
        Instance = this;
        SessionStarted = DateTime.UtcNow;
    }

    public void SetGameMode(GameMode mode)
    {
        if (mode == CurrentMode) return;
        CurrentMode = mode;
        EmitSignal(SignalName.GameModeChanged, (int)mode);
        Log($"Mode -> {mode}");
    }
}
