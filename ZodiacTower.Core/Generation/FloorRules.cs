using System;
using System.Collections.Generic;

namespace ZodiacTower.Core.Generation
{

public sealed class FloorRules
{
    public int Floor { get; }
    public int MinimumBudget { get; }
    public int TargetBudget { get; }
    public int MaximumBudget { get; }
    public int MinimumSideValue { get; }
    public int MaximumSideValue { get; }

    public FloorRules(int floor, int minimumBudget, int targetBudget, int maximumBudget, int minimumSideValue, int maximumSideValue)
    {
        Floor = floor;
        MinimumBudget = minimumBudget;
        TargetBudget = targetBudget;
        MaximumBudget = maximumBudget;
        MinimumSideValue = minimumSideValue;
        MaximumSideValue = maximumSideValue;
    }
}

public static class FloorCatalog
{
    private static readonly FloorRules[] Floors =
    {
        new(1, 12, 14, 16, 1, 5),
        new(2, 15, 17, 19, 1, 5),
        new(3, 18, 20, 22, 1, 6),
        new(4, 21, 23, 25, 2, 6),
        new(5, 24, 26, 28, 2, 7),
        new(6, 27, 29, 31, 3, 7),
        new(7, 30, 32, 34, 3, 8),
        new(8, 33, 35, 37, 3, 8),
        new(9, 36, 38, 40, 4, 9),
        new(10, 39, 41, 43, 4, 9),
        new(11, 42, 44, 46, 4, 10),
        new(12, 45, 47, 49, 5, 10),
        new(13, 48, 50, 52, 5, 11)
    };

    public static IReadOnlyList<FloorRules> All => Floors;

    public static FloorRules Get(int floor)
    {
        if (floor < 1 || floor > Floors.Length)
            throw new ArgumentOutOfRangeException(nameof(floor), "Floor must be between 1 and 13.");

        return Floors[floor - 1];
    }
}
}
