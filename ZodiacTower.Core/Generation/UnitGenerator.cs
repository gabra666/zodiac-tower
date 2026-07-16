using System;
using System.Collections.Generic;
using System.Linq;
using ZodiacTower.Core.Random;
using ZodiacTower.Core.Units;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Core.Generation
{

public interface IUnitGenerator
{
    Unit Generate(FloorRules floor, ZodiacSign sign, int seed);
}

public sealed class UnitGenerator : IUnitGenerator
{
    private const double CardinalChangePoint = 0.78;
    private const double FixedChangePoint = 0.50;

    public Unit Generate(FloorRules floor, ZodiacSign sign, int seed)
    {
        Validate(floor);
        var random = new StableRandom(seed);
        int budget = random.Next(floor.MinimumBudget, floor.MaximumBudget + 1);
        var sides = Enumerable.Repeat(floor.MinimumSideValue, 6).ToArray();
        int remaining = budget - sides.Sum();
        int totalToDistribute = remaining;
        int distributed = 0;
        int iteration = 0;
        bool thresholdChanged = false;
        var profile = ZodiacCatalog.Get(sign);
        var history = new List<PatternStep>();
        int[] pattern = PickPattern(random, sides, floor.MaximumSideValue);
        history.Add(new PatternStep(0, pattern));

        while (remaining > 0)
        {
            int added = DistributeOnce(pattern, sides, floor.MaximumSideValue, ref remaining);
            distributed += added;
            iteration++;

            if (added == 0 || ShouldChange(profile.Modality, distributed, totalToDistribute, iteration, ref thresholdChanged))
            {
                pattern = PickPattern(random, sides, floor.MaximumSideValue);
                history.Add(new PatternStep(distributed, pattern));
            }
        }

        return new Unit(seed, sign, floor.Floor, budget, sides, history);
    }

    private static bool ShouldChange(Modality modality, int distributed, int total, int iteration, ref bool thresholdChanged)
    {
        if (modality == Modality.Mutable)
            return true;

        double changePoint = modality == Modality.Cardinal ? CardinalChangePoint : FixedChangePoint;
        if (!thresholdChanged && distributed >= Math.Ceiling(total * changePoint))
        {
            thresholdChanged = true;
            return true;
        }

        return false;
    }

    private static int DistributeOnce(int[] pattern, int[] sides, int maximum, ref int remaining)
    {
        int added = 0;
        foreach (int side in pattern)
        {
            if (remaining == 0)
                break;
            if (sides[side] >= maximum)
                continue;

            sides[side]++;
            remaining--;
            added++;
        }

        return added;
    }

    private static int[] PickPattern(IRandomSource random, int[] sides, int maximum)
    {
        var available = Enumerable.Range(0, 6).Where(index => sides[index] < maximum).ToList();
        if (available.Count == 0)
            throw new InvalidOperationException("No side can receive the remaining budget.");

        for (int i = available.Count - 1; i > 0; i--)
        {
            int swap = random.Next(0, i + 1);
            (available[i], available[swap]) = (available[swap], available[i]);
        }

        return available.Take(Math.Min(3, available.Count)).OrderBy(index => index).ToArray();
    }

    private static void Validate(FloorRules floor)
    {
        int minimumTotal = floor.MinimumSideValue * 6;
        int maximumTotal = floor.MaximumSideValue * 6;
        if (floor.MinimumBudget < minimumTotal || floor.MaximumBudget > maximumTotal)
            throw new ArgumentException("The floor budget cannot be represented by its side limits.", nameof(floor));
    }
}
}
