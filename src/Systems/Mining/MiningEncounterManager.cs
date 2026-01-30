using System.Collections.Generic;
using Godot;
using Vacuum.AI.Mining;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Coordinates combat encounters during mining: spawning, loot drops, and resolution.
/// </summary>
public partial class MiningEncounterManager : Node
{
    public static MiningEncounterManager? Instance { get; private set; }

    private readonly List<ClaimJumperAI> _activeEncounters = new();

    public int ActiveEncounterCount => _activeEncounters.Count;

    [Signal] public delegate void EncounterStartedEventHandler(int jumperCount);
    [Signal] public delegate void EncounterEndedEventHandler(int jumpersDefeated);
    [Signal] public delegate void LootDroppedEventHandler(string itemId, int quantity);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[MiningEncounterManager] Ready.");
    }

    public override void _Process(double delta)
    {
        // Clean up destroyed encounters
        for (int i = _activeEncounters.Count - 1; i >= 0; i--)
        {
            if (!IsInstanceValid(_activeEncounters[i]) ||
                _activeEncounters[i].CurrentState == ClaimJumperAI.BehaviorState.Destroyed)
            {
                _activeEncounters.RemoveAt(i);
                MiningStatistics.Instance?.RecordClaimJumperDefeated();
            }
        }
    }

    /// <summary>Spawn a group of claim jumpers near a target.</summary>
    public void SpawnEncounter(Node3D target, float difficulty, int count)
    {
        var tactic = ClaimJumperBehaviors.SelectTactic(difficulty, count);
        var offsets = ClaimJumperBehaviors.GetFormationOffsets(tactic, count);

        var jumpers = new List<ClaimJumperAI>();

        for (int i = 0; i < count && i < offsets.Count; i++)
        {
            var ai = new ClaimJumperAI();
            ai.Initialize(target, target.GlobalPosition, difficulty);
            ai.GlobalPosition = target.GlobalPosition + new Vector3(
                offsets[i].offsetX, offsets[i].offsetY, offsets[i].offsetZ);

            if (tactic == ClaimJumperBehaviors.TacticType.Ambush)
                ai.GlobalPosition += new Vector3(0, 0, 200f); // spawn far for ambush

            ai.Destroyed += () => OnJumperDestroyed(difficulty);
            AddChild(ai);
            _activeEncounters.Add(ai);
            jumpers.Add(ai);
        }

        MiningStatistics.Instance?.RecordClaimJumperEncounter();
        EmitSignal(SignalName.EncounterStarted, count);
    }

    private void OnJumperDestroyed(float difficulty)
    {
        float lootQuality = ClaimJumperBehaviors.GetLootQualityMultiplier(difficulty);
        int lootQuantity = (int)(10 * lootQuality);
        EmitSignal(SignalName.LootDropped, "salvage", lootQuantity);

        if (_activeEncounters.Count == 0)
            EmitSignal(SignalName.EncounterEnded, 0);
    }
}
