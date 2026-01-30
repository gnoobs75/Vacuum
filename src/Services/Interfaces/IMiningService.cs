using System.Collections.Generic;
using Vacuum.Data.Models;

namespace Vacuum.Services.Interfaces;

public interface IMiningService
{
    // Asteroid discovery
    List<AsteroidData> GetNearbyAsteroids(float x, float y, float z, float radius, string? oreTypeFilter = null);

    // Mining operations
    MiningOperationData StartMiningOperation(string characterId, string asteroidId, string laserId);
    MiningOperationData? GetActiveOperation(string characterId);
    List<MiningOperationData> GetOperationHistory(string characterId, int limit = 50);
    void StopMiningOperation(string operationId);

    // Yield calculations
    float CalculateYield(string oreType, float laserYield, float efficiency);

    // Reprocessing
    ReprocessingJobData StartReprocessingJob(string characterId, string oreType, int quantity, float efficiency);
    List<ReprocessingJobData> GetActiveReprocessingJobs(string characterId);
}
