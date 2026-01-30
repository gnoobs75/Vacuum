using System.Threading;
using System.Threading.Tasks;
using Vacuum.Services.Mining.Config;
using Vacuum.Tasks;

namespace Vacuum.Services.Mining.Tasks;

/// <summary>
/// Background task for procedural asteroid generation with randomized ore types and quantities.
/// </summary>
public class AsteroidGenerationTask : BaseTask
{
    private readonly string _systemId;
    private readonly float _beltRadius;
    private readonly int _count;

    public AsteroidGenerationTask(string systemId, float beltRadius = 0, int count = 0)
    {
        Name = "AsteroidGeneration";
        _systemId = systemId;
        _beltRadius = beltRadius > 0 ? beltRadius : MiningConfig.DefaultBeltRadius;
        _count = count > 0 ? count : MiningConfig.DefaultAsteroidCount;
    }

    public override Task<TaskResult> ExecuteAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        ReportProgress(0.1f);

        // Generate asteroids (AsteroidGenerator uses Godot RNG so actual generation
        // must happen on the main thread; this task prepares the parameters)
        var asteroids = AsteroidGenerator.GenerateAsteroidBelt(
            _systemId, _beltRadius, _count,
            MiningConfig.AsteroidSpreadX, MiningConfig.AsteroidSpreadY, MiningConfig.AsteroidSpreadZ);

        ReportProgress(0.8f);

        // Register with mining service if available
        var service = MiningService.Instance;
        if (service != null)
        {
            foreach (var asteroid in asteroids)
            {
                ct.ThrowIfCancellationRequested();
                service.RegisterAsteroid(asteroid);
            }
        }

        ReportProgress(1f);
        return Task.FromResult(TaskResult.Ok($"Generated {asteroids.Count} asteroids in system {_systemId}"));
    }
}
