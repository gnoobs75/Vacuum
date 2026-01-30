using System;
using System.Collections.Generic;

namespace Vacuum.AI.Mining;

/// <summary>
/// Defines claim jumper behavior patterns and faction-based decision making.
/// </summary>
public static class ClaimJumperBehaviors
{
    public enum TacticType { Direct, Flanking, Ambush, Swarm }

    /// <summary>Select a tactic based on difficulty and jumper count.</summary>
    public static TacticType SelectTactic(float difficulty, int jumperCount)
    {
        if (jumperCount >= 3) return TacticType.Swarm;
        if (difficulty > 1.5f) return TacticType.Ambush;
        if (difficulty > 1.0f) return TacticType.Flanking;
        return TacticType.Direct;
    }

    /// <summary>Get spawn positions for a group of jumpers using the given tactic.</summary>
    public static List<(float offsetX, float offsetY, float offsetZ)> GetFormationOffsets(
        TacticType tactic, int count)
    {
        var offsets = new List<(float, float, float)>();
        float spacing = 40f;

        for (int i = 0; i < count; i++)
        {
            float angle = i * (MathF.Tau / count);
            float radius = tactic switch
            {
                TacticType.Swarm => spacing * 0.5f,
                TacticType.Flanking => spacing * 1.5f,
                TacticType.Ambush => spacing * 2f,
                _ => spacing
            };

            offsets.Add((
                MathF.Cos(angle) * radius,
                (i % 2 == 0 ? 1f : -1f) * 10f,
                MathF.Sin(angle) * radius
            ));
        }

        return offsets;
    }

    /// <summary>Calculate loot quality based on difficulty.</summary>
    public static float GetLootQualityMultiplier(float difficulty) =>
        Math.Clamp(0.5f + difficulty * 0.5f, 0.5f, 3f);
}
