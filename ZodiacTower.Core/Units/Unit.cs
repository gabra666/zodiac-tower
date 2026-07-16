using System.Collections.Generic;
using System.Linq;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Core.Units
{

public sealed class PatternStep
{
    public int DistributedAt { get; }
    public int[] ActiveSides { get; }

    public PatternStep(int distributedAt, int[] activeSides)
    {
        DistributedAt = distributedAt;
        ActiveSides = activeSides;
    }
}

public sealed class Unit
{
    public int Seed { get; }
    public ZodiacSign Sign { get; }
    public int Floor { get; }
    public int Budget { get; }
    public int[] Sides { get; }
    public IReadOnlyList<PatternStep> PatternHistory { get; }
    public int TotalPower => Sides.Sum();
    public int Spread => Sides.Max() - Sides.Min();

    public Unit(int seed, ZodiacSign sign, int floor, int budget, int[] sides, IReadOnlyList<PatternStep> patternHistory)
    {
        Seed = seed;
        Sign = sign;
        Floor = floor;
        Budget = budget;
        Sides = sides;
        PatternHistory = patternHistory;
    }

    public int SideAtRotation(int rotation) => Sides[NormalizeSide(rotation)];

    public static int OppositeSide(int side) => NormalizeSide(side + 3);

    private static int NormalizeSide(int side) => ((side % 6) + 6) % 6;
}
}
