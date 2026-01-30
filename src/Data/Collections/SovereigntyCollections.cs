using Vacuum.Data.Models;

namespace Vacuum.Data.Collections;

/// <summary>
/// Pre-configured collections for sovereignty data types.
/// </summary>
public class SovereigntyCollections
{
    public DataCollection<SovereigntyStructureData> Structures { get; } = new(s => s.StructureId);
    public DataCollection<TerritoryData> Territories { get; } = new(t => t.TerritoryId);
    public DataCollection<VulnerabilityTimerData> Timers { get; } = new(t => t.TimerId);
    public DataCollection<DefenseEventData> DefenseEvents { get; } = new(d => d.EventId);
    public DataCollection<StructureUpgradeData> Upgrades { get; } = new(u => u.UpgradeId);
}
