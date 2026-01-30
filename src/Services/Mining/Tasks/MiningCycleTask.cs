using System;
using System.Threading;
using System.Threading.Tasks;
using Vacuum.Data;
using Vacuum.Services.Mining.Config;
using Vacuum.Tasks;

namespace Vacuum.Services.Mining.Tasks;

/// <summary>
/// Background task for calculating mining cycle results including yield,
/// heat management, and equipment degradation.
/// </summary>
public class MiningCycleTask : BaseTask
{
    private readonly string _operationId;
    private readonly string _oreType;
    private readonly int _laserCount;
    private readonly float _skillMultiplier;
    private readonly float _currentHeat;

    public float CalculatedYield { get; private set; }
    public float HeatAfterCycle { get; private set; }
    public bool IsOverheated { get; private set; }
    public float EquipmentDegradation { get; private set; }

    public MiningCycleTask(string operationId, string oreType, int laserCount,
        float skillMultiplier = 1f, float currentHeat = 0f)
    {
        Name = "MiningCycleCalc";
        _operationId = operationId;
        _oreType = oreType;
        _laserCount = laserCount;
        _skillMultiplier = skillMultiplier;
        _currentHeat = currentHeat;
    }

    public override Task<TaskResult> ExecuteAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ReportProgress(0.2f);

        // Calculate yield
        float baseYield = MiningConfig.DefaultYieldPerCycle;
        if (OreDatabase.Ores.TryGetValue(_oreType, out var oreDef))
            baseYield = oreDef.BaseYield / 10f; // scale down for per-cycle

        CalculatedYield = YieldCalculator.CalculateCycleYield(baseYield, _laserCount, _skillMultiplier);

        ReportProgress(0.5f);

        // Heat management
        float heatAdded = MiningConfig.HeatPerCycle * _laserCount;
        float heatDissipated = MiningConfig.HeatDissipationRate * MiningConfig.DefaultCycleTimeSeconds;
        HeatAfterCycle = Math.Clamp(_currentHeat + heatAdded - heatDissipated, 0f, MiningConfig.MaxHeat);
        IsOverheated = HeatAfterCycle >= MiningConfig.OverheatThreshold;

        ReportProgress(0.8f);

        // Equipment degradation (minor per cycle)
        EquipmentDegradation = 0.01f * _laserCount;
        if (IsOverheated)
            EquipmentDegradation *= 3f; // faster degradation when overheated

        ReportProgress(1f);

        // Record yield in service
        MiningService.Instance?.RecordYield(_operationId, (int)CalculatedYield);

        return Task.FromResult(TaskResult.Ok(
            $"Cycle complete: {CalculatedYield:F0} yield, heat {HeatAfterCycle:F0}/{MiningConfig.MaxHeat}"));
    }
}
