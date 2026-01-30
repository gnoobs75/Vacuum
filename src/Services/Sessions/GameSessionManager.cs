using System;
using Godot;
using Vacuum.Data.Models;
using Vacuum.Data.Persistence;

namespace Vacuum.Services.Sessions;

/// <summary>
/// Manages active game session state and persistence.
/// </summary>
public partial class GameSessionManager : Node
{
    public static GameSessionManager? Instance { get; private set; }

    public SessionData? CurrentSession { get; private set; }
    public bool HasActiveSession => CurrentSession != null;

    [Signal] public delegate void SessionStartedEventHandler(string sessionId);
    [Signal] public delegate void SessionEndedEventHandler(string sessionId);

    public override void _Ready()
    {
        Instance = this;
    }

    public SessionData StartSession(string playerId, string characterId)
    {
        CurrentSession = new SessionData
        {
            PlayerId = playerId,
            CharacterId = characterId,
            StartedAt = DateTime.UtcNow
        };
        EmitSignal(SignalName.SessionStarted, CurrentSession.SessionId);
        GD.Print($"[Session] Started: {CurrentSession.SessionId}");
        return CurrentSession;
    }

    public void SaveSession()
    {
        if (CurrentSession == null) return;
        CurrentSession.LastSaved = DateTime.UtcNow;
        SaveGameManager.Instance?.Save("session", CurrentSession);
    }

    public void EndSession()
    {
        if (CurrentSession == null) return;
        var id = CurrentSession.SessionId;
        SaveSession();
        CurrentSession = null;
        EmitSignal(SignalName.SessionEnded, id);
        GD.Print($"[Session] Ended: {id}");
    }

    public SessionData? LoadLastSession()
    {
        return SaveGameManager.Instance?.Load<SessionData>("session");
    }
}
