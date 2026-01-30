using System;
using Vacuum.Data.Enums;

namespace Vacuum.Data.Models;

/// <summary>
/// WO-157: Structure placed in space (station, gate, etc).
/// </summary>
public class StructureData
{
    public string StructureId { get; set; } = Guid.NewGuid().ToString();
    public string StructureTypeId { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Health { get; set; } = 100f;
}

/// <summary>
/// WO-157: Territory controlled by a faction, anchored to a structure.
/// </summary>
public class TerritoryControlData
{
    public string TerritoryId { get; set; } = Guid.NewGuid().ToString();
    public string StructureId { get; set; } = string.Empty;
    public string ControllingFactionId { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// WO-157: Corporation member record.
/// </summary>
public class CorporationMemberData
{
    public string MemberId { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public CorporationRole Role { get; set; } = CorporationRole.Member;
    public float Morale { get; set; } = 1.0f;
}

/// <summary>
/// WO-157: Save file record for a player.
/// </summary>
public class SaveFileData
{
    public string SaveId { get; set; } = Guid.NewGuid().ToString();
    public string PlayerId { get; set; } = string.Empty;
    public string SaveName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
