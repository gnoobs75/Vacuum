using System;
using System.Collections.Generic;
using Godot;

namespace Vacuum.Core;

/// <summary>
/// Central event bus for decoupled communication between game systems.
/// Adapted from WO-33 (WebSocket comms) and WO-70 (task processing) into
/// a signal/event-based architecture for single-player Godot.
/// </summary>
public partial class GameEventBus : Node
{
    public static GameEventBus? Instance { get; private set; }

    // Navigation events
    [Signal] public delegate void ShipPositionUpdatedEventHandler(string shipId, Vector3 position, Vector3 velocity);
    [Signal] public delegate void WarpInitiatedEventHandler(string shipId, Vector3 destination);
    [Signal] public delegate void WarpCompletedEventHandler(string shipId, Vector3 position);
    [Signal] public delegate void BookmarkCreatedEventHandler(string bookmarkId, string name, Vector3 position);
    [Signal] public delegate void BookmarkDeletedEventHandler(string bookmarkId);
    [Signal] public delegate void AutopilotToggledEventHandler(string shipId, bool enabled);
    [Signal] public delegate void RouteCalculatedEventHandler(string routeId);

    // Ship events
    [Signal] public delegate void ShipDamagedEventHandler(string shipId, float amount, string damageType);
    [Signal] public delegate void ShipDestroyedEventHandler(string shipId);
    [Signal] public delegate void ShieldDepletedEventHandler(string shipId);
    [Signal] public delegate void ModuleFittedEventHandler(string shipId, string slotId, string moduleId);
    [Signal] public delegate void ModuleActivatedEventHandler(string shipId, string moduleId);

    // Power events
    [Signal] public delegate void CapacitorDepletedEventHandler(string shipId);
    [Signal] public delegate void OverloadStartedEventHandler(string shipId);
    [Signal] public delegate void OverloadEndedEventHandler(string shipId);

    // Wormhole events
    [Signal] public delegate void WormholeDiscoveredEventHandler(string wormholeId);
    [Signal] public delegate void WormholeCollapsingEventHandler(string wormholeId, float timeRemaining);
    [Signal] public delegate void WormholeCollapsedEventHandler(string wormholeId);
    [Signal] public delegate void WormholeTransitEventHandler(string shipId, string wormholeId);

    // Faction events
    [Signal] public delegate void StandingChangedEventHandler(string characterId, string factionId, float newValue);

    // Game state events
    [Signal] public delegate void GameSavedEventHandler(string saveId);
    [Signal] public delegate void GameLoadedEventHandler(string saveId);

    public override void _Ready()
    {
        Instance = this;
    }
}
