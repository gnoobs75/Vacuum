using System.Collections.Generic;
using Godot;
using Vacuum.Data;
using Vacuum.Data.Models;

namespace Vacuum.Services.Mining;

/// <summary>
/// Generates asteroid instances with randomized ore types, quantities, and positions.
/// </summary>
public static class AsteroidGenerator
{
    /// <summary>
    /// Generate a batch of asteroids around a center point, with ore types based on distance from star.
    /// </summary>
    public static List<AsteroidData> GenerateAsteroidBelt(
        string systemId, float beltRadius, int count,
        float spreadX = 200f, float spreadY = 30f, float spreadZ = 200f)
    {
        var asteroids = new List<AsteroidData>();

        for (int i = 0; i < count; i++)
        {
            float angle = (float)GD.RandRange(0, Mathf.Tau);
            float dist = beltRadius + (float)GD.RandRange(-spreadX * 0.5, spreadX * 0.5);

            float x = Mathf.Cos(angle) * dist;
            float y = (float)GD.RandRange(-spreadY, spreadY);
            float z = Mathf.Sin(angle) * dist;

            string[] oreTypes = OreDatabase.GetOresForDistance(dist, beltRadius);
            string oreType = oreTypes[(int)(GD.Randi() % oreTypes.Length)];

            int quantity = OreDatabase.Ores.TryGetValue(oreType, out var def)
                ? (int)(def.BaseYield * GD.RandRange(0.5, 2.0))
                : 100;

            asteroids.Add(new AsteroidData
            {
                SystemId = systemId,
                PositionX = x,
                PositionY = y,
                PositionZ = z,
                OreType = oreType,
                Quantity = quantity
            });
        }

        return asteroids;
    }
}
