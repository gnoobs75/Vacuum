using System;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-103: Graphics settings.
/// </summary>
public class GraphicsSettingsData
{
    public string CharacterId { get; set; } = string.Empty;
    public int ResolutionWidth { get; set; } = 1920;
    public int ResolutionHeight { get; set; } = 1080;
    public bool Fullscreen { get; set; } = true;
    public QualityLevel QualityLevel { get; set; } = QualityLevel.High;
    public bool VSync { get; set; } = true;
    public int FpsLimit { get; set; } = 60;
    public float RenderScale { get; set; } = 1.0f;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-103: Audio settings.
/// </summary>
public class AudioSettingsData
{
    public string CharacterId { get; set; } = string.Empty;
    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    public float SfxVolume { get; set; } = 1.0f;
    public float UiVolume { get; set; } = 0.8f;
    public float VoiceVolume { get; set; } = 1.0f;
    public bool Muted { get; set; } = false;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-103: Input mapping for a control action.
/// </summary>
public class InputMappingData
{
    public string CharacterId { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public string? SecondaryKey { get; set; }
    public string? GamepadButton { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-103: UI preference settings.
/// </summary>
public class UiPreferencesData
{
    public string CharacterId { get; set; } = string.Empty;
    public float HudScale { get; set; } = 1.0f;
    public bool ShowTooltips { get; set; } = true;
    public bool ShowTutorialHints { get; set; } = true;
    public string ChatFontSize { get; set; } = "Medium";
    public float MinimapScale { get; set; } = 1.0f;
    public bool ShowDamageNumbers { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-103: Accessibility settings.
/// </summary>
public class AccessibilitySettingsData
{
    public string CharacterId { get; set; } = string.Empty;
    public ColorblindMode ColorblindMode { get; set; } = ColorblindMode.None;
    public bool ScreenShakeEnabled { get; set; } = true;
    public float TextScale { get; set; } = 1.0f;
    public bool HighContrastUi { get; set; } = false;
    public bool ReducedMotion { get; set; } = false;
    public bool SubtitlesEnabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
