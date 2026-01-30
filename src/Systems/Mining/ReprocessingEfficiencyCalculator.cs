using System;
using Vacuum.Services.Mining.Config;

namespace Vacuum.Systems.Mining;

/// <summary>
/// Calculates reprocessing efficiency based on skills, station facilities, and equipment.
/// </summary>
public static class ReprocessingEfficiencyCalculator
{
    /// <summary>
    /// Calculate total reprocessing efficiency from contributing factors.
    /// </summary>
    public static float Calculate(
        float baseEfficiency = 0,
        float skillBonus = 0f,
        float facilityBonus = 0f,
        float implantBonus = 0f)
    {
        if (baseEfficiency <= 0)
            baseEfficiency = MiningConfig.BaseReprocessingEfficiency;

        float total = baseEfficiency + skillBonus + facilityBonus + implantBonus;
        return Math.Clamp(total, 0f, 1f);
    }

    /// <summary>Breakdown of efficiency factors for display.</summary>
    public static EfficiencyBreakdown GetBreakdown(
        float baseEfficiency = 0,
        float skillBonus = 0f,
        float facilityBonus = 0f,
        float implantBonus = 0f)
    {
        if (baseEfficiency <= 0)
            baseEfficiency = MiningConfig.BaseReprocessingEfficiency;

        return new EfficiencyBreakdown
        {
            BaseEfficiency = baseEfficiency,
            SkillBonus = skillBonus,
            FacilityBonus = facilityBonus,
            ImplantBonus = implantBonus,
            TotalEfficiency = Calculate(baseEfficiency, skillBonus, facilityBonus, implantBonus)
        };
    }

    public class EfficiencyBreakdown
    {
        public float BaseEfficiency { get; set; }
        public float SkillBonus { get; set; }
        public float FacilityBonus { get; set; }
        public float ImplantBonus { get; set; }
        public float TotalEfficiency { get; set; }
    }
}
