using Godot;
using Vacuum.Data.Models;

namespace Vacuum.Core;

/// <summary>
/// Root game manager autoload. Holds current player/character state.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }

    public PlayerData CurrentPlayer { get; set; } = new() { Username = "Pilot" };
    public CharacterData CurrentCharacter { get; set; } = new() { Name = "Commander" };

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[GameManager] Vacuum initialized.");
    }
}
