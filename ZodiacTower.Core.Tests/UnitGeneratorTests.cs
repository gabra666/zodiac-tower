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
}
