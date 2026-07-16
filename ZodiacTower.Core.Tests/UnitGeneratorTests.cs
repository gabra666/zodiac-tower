using ZodiacTower.Core.Generation;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Core.Tests;

public sealed class UnitGeneratorTests
{
    private readonly UnitGenerator _generator = new();

    [Fact]
    public void SameInputProducesSameUnit()
    {
        var floor = FloorCatalog.Get(7);

        var first = _generator.Generate(floor, ZodiacSign.Scorpio, 184729);
        var second = _generator.Generate(floor, ZodiacSign.Scorpio, 184729);

        Assert.Equal(first.Budget, second.Budget);
        Assert.Equal(first.Sides, second.Sides);
        Assert.Equal(first.PatternHistory.Count, second.PatternHistory.Count);
    }

    [Fact]
    public void GeneratedUnitsRespectEveryFloorRule()
    {
        foreach (var floor in FloorCatalog.All)
        {
            for (int seed = 1; seed <= 250; seed++)
            {
                var unit = _generator.Generate(floor, (ZodiacSign)(seed % 12), seed);

                Assert.InRange(unit.TotalPower, floor.MinimumBudget, floor.MaximumBudget);
                Assert.Equal(unit.Budget, unit.TotalPower);
                Assert.All(unit.Sides, value => Assert.InRange(value, floor.MinimumSideValue, floor.MaximumSideValue));
                int pointsToDistribute = unit.Budget - floor.MinimumSideValue * 6;
                Assert.Equal(pointsToDistribute, unit.DistributionHistory.Sum(step => step.PointsAdded));
                Assert.All(unit.DistributionHistory, step => Assert.True(step.PointsAdded > 0));
                Assert.Equal(unit.Sides, unit.DistributionHistory[^1].SidesAfter);
                Assert.All(unit.PatternHistory, step => Assert.True(step.DistributedAt < pointsToDistribute));
            }
        }
    }

    [Fact]
    public void MutableUnitsChangePatternMoreOftenThanCardinalUnits()
    {
        var floor = FloorCatalog.Get(8);
        var cardinal = _generator.Generate(floor, ZodiacSign.Aries, 91234);
        var mutable = _generator.Generate(floor, ZodiacSign.Gemini, 91234);

        Assert.True(mutable.PatternHistory.Count > cardinal.PatternHistory.Count);
    }

    [Fact]
    public void DistributionHistoryContainsOnlyAppliedSteps()
    {
        var floor = FloorCatalog.Get(1);
        var unit = _generator.Generate(floor, ZodiacSign.Virgo, 184728);
        int pointsToDistribute = unit.Budget - floor.MinimumSideValue * 6;

        Assert.Equal(15, unit.Budget);
        Assert.Equal(new[] { 4, 2, 1, 1, 4, 3 }, unit.Sides);
        Assert.Equal(3, unit.DistributionHistory.Count);
        Assert.Equal(pointsToDistribute, unit.DistributionHistory.Sum(step => step.PointsAdded));
        Assert.All(unit.DistributionHistory, step => Assert.True(step.PointsAdded > 0));
    }
}
