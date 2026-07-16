using ZodiacTower.Core.Battle;
using ZodiacTower.Core.Generation;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Core.Tests;

public sealed class ZodiacAndBattleTests
{
    [Theory]
    [InlineData(ZodiacSign.Aries, ZodiacElement.Fire, Polarity.Light, Modality.Cardinal)]
    [InlineData(ZodiacSign.Taurus, ZodiacElement.Earth, Polarity.Dark, Modality.Fixed)]
    [InlineData(ZodiacSign.Pisces, ZodiacElement.Water, Polarity.Dark, Modality.Mutable)]
    public void SignAlwaysDerivesTheSameIdentity(ZodiacSign sign, ZodiacElement element, Polarity polarity, Modality modality)
    {
        var profile = ZodiacCatalog.Get(sign);

        Assert.Equal(element, profile.Element);
        Assert.Equal(polarity, profile.Polarity);
        Assert.Equal(modality, profile.Modality);
    }

    [Fact]
    public void FireReceivesElementBonusAgainstAir()
    {
        var generator = new UnitGenerator();
        var floor = FloorCatalog.Get(1);
        var fire = generator.Generate(floor, ZodiacSign.Aries, 101);
        var air = generator.Generate(floor, ZodiacSign.Gemini, 202);

        var result = new BattleService().Resolve(fire, 0, air, 0, new ZodiacBattleRules { UseElement = true });

        Assert.Equal(1, result.AttackerZodiacBonus);
        Assert.Equal(0, result.DefenderZodiacBonus);
    }
}
