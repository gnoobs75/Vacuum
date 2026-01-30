using Vacuum.Data.Models;

namespace Vacuum.Data.Collections;

/// <summary>
/// Pre-configured collections for mining data types.
/// </summary>
public class MiningCollections
{
    public DataCollection<AsteroidData> Asteroids { get; } = new(a => a.AsteroidId);
    public DataCollection<MiningOperationData> Operations { get; } = new(o => o.OperationId);
    public DataCollection<MiningLaserData> Lasers { get; } = new(l => l.LaserId);
    public DataCollection<ReprocessingJobData> ReprocessingJobs { get; } = new(j => j.JobId);
}
