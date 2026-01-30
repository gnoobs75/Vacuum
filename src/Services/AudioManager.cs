using System.Collections.Generic;
using Godot;

namespace Vacuum.Services;

/// <summary>
/// Singleton audio management service for sound effects, music, and ambient audio.
/// </summary>
public partial class AudioManager : BaseService
{
    public static AudioManager? Instance { get; private set; }

    private readonly Dictionary<string, AudioStreamPlayer> _players = new();

    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    public float SfxVolume { get; set; } = 1.0f;

    protected override void InitializeService()
    {
        Instance = this;
    }

    public void PlaySfx(string name, AudioStream stream, float volumeDb = 0f)
    {
        var player = new AudioStreamPlayer();
        player.Stream = stream;
        player.VolumeDb = volumeDb;
        player.Finished += () => { player.QueueFree(); _players.Remove(name); };
        AddChild(player);
        _players[name] = player;
        player.Play();
    }

    public void StopAll()
    {
        foreach (var player in _players.Values)
            player.Stop();
        _players.Clear();
    }

    public void SetBusVolume(string busName, float linearVolume)
    {
        int idx = AudioServer.GetBusIndex(busName);
        if (idx >= 0)
            AudioServer.SetBusVolumeDb(idx, Mathf.LinearToDb(linearVolume));
    }
}
