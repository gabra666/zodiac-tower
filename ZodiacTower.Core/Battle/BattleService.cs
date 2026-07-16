using System;
using ZodiacTower.Core.Units;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Core.Battle
{

public enum BattleOutcome { AttackerWins, Draw, DefenderWins }

public sealed class ZodiacBattleRules
{
    public bool UseElement { get; set; } = true;
    public bool UsePolarity { get; set; }
    public bool UseModality { get; set; }
    public int AdvantageBonus { get; set; } = 1;
}

public sealed class BattleResult
{
    public int AttackerBaseValue { get; }
    public int DefenderBaseValue { get; }
    public int AttackerZodiacBonus { get; }
    public int DefenderZodiacBonus { get; }
    public int AttackerFinalValue => AttackerBaseValue + AttackerZodiacBonus;
    public int DefenderFinalValue => DefenderBaseValue + DefenderZodiacBonus;
    public BattleOutcome Outcome => AttackerFinalValue == DefenderFinalValue
        ? BattleOutcome.Draw
        : AttackerFinalValue > DefenderFinalValue ? BattleOutcome.AttackerWins : BattleOutcome.DefenderWins;

    public BattleResult(int attackerBaseValue, int defenderBaseValue, int attackerZodiacBonus, int defenderZodiacBonus)
    {
        AttackerBaseValue = attackerBaseValue;
        DefenderBaseValue = defenderBaseValue;
        AttackerZodiacBonus = attackerZodiacBonus;
        DefenderZodiacBonus = defenderZodiacBonus;
    }
}

public sealed class BattleService
{
    public BattleResult Resolve(Unit attacker, int attackerSide, Unit defender, int defenderSide, ZodiacBattleRules rules)
    {
        ValidateSide(attackerSide);
        ValidateSide(defenderSide);
        var attackerProfile = ZodiacCatalog.Get(attacker.Sign);
        var defenderProfile = ZodiacCatalog.Get(defender.Sign);
        int attackerBonus = CalculateBonus(attackerProfile, defenderProfile, rules);
        int defenderBonus = CalculateBonus(defenderProfile, attackerProfile, rules);

        return new BattleResult(attacker.Sides[attackerSide], defender.Sides[defenderSide], attackerBonus, defenderBonus);
    }

    private static int CalculateBonus(ZodiacProfile source, ZodiacProfile target, ZodiacBattleRules rules)
    {
        int bonus = 0;
        if (rules.UseElement && HasElementAdvantage(source.Element, target.Element))
            bonus += rules.AdvantageBonus;
        if (rules.UsePolarity && source.Polarity != target.Polarity)
            bonus += rules.AdvantageBonus;
        if (rules.UseModality && HasModalityAdvantage(source.Modality, target.Modality))
            bonus += rules.AdvantageBonus;
        return bonus;
    }

    private static bool HasElementAdvantage(ZodiacElement source, ZodiacElement target) =>
        (source == ZodiacElement.Fire && target == ZodiacElement.Air) ||
        (source == ZodiacElement.Air && target == ZodiacElement.Earth) ||
        (source == ZodiacElement.Earth && target == ZodiacElement.Water) ||
        (source == ZodiacElement.Water && target == ZodiacElement.Fire);

    private static bool HasModalityAdvantage(Modality source, Modality target) =>
        (source == Modality.Cardinal && target == Modality.Mutable) ||
        (source == Modality.Mutable && target == Modality.Fixed) ||
        (source == Modality.Fixed && target == Modality.Cardinal);

    private static void ValidateSide(int side)
    {
        if (side < 0 || side > 5)
            throw new ArgumentOutOfRangeException(nameof(side), "Side must be between 0 and 5.");
    }
}
}
