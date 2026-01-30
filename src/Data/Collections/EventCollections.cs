using Vacuum.Data.Models;

namespace Vacuum.Data.Collections;

/// <summary>
/// Pre-configured collections for event system data types.
/// </summary>
public class EventCollections
{
    public DataCollection<GameEventDefinition> Definitions { get; } = new(e => e.EventId);
    public DataCollection<ActiveEventData> ActiveEvents { get; } = new(e => e.ActiveEventId);
    public DataCollection<PlayerDecisionData> Decisions { get; } = new(d => d.DecisionId);
    public DataCollection<EventConsequenceData> Consequences { get; } = new(c => c.ConsequenceId);
    public DataCollection<EventChainData> Chains { get; } = new(c => c.ChainId);
}
