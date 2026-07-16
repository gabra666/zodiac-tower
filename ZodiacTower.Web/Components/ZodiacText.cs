using ZodiacTower.Core.Battle;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Web.Components;

public static class ZodiacText
{
    private static readonly string[] Signs = { "Aries", "Tauro", "Geminis", "Cancer", "Leo", "Virgo", "Libra", "Escorpio", "Sagitario", "Capricornio", "Acuario", "Piscis" };
    private static readonly string[] Elements = { "Fuego", "Tierra", "Aire", "Agua" };
    private static readonly string[] Polarities = { "Luz", "Oscuridad" };
    private static readonly string[] Modalities = { "Cardinal", "Fijo", "Mutable" };

    public static string Sign(ZodiacSign value) => Signs[(int)value];
    public static string Element(ZodiacElement value) => Elements[(int)value];
    public static string Polarity(Polarity value) => Polarities[(int)value];
    public static string Modality(Modality value) => Modalities[(int)value];
    public static string Outcome(BattleOutcome value) => value switch
    {
        BattleOutcome.AttackerWins => "La unidad A captura",
        BattleOutcome.DefenderWins => "La unidad B captura",
        _ => "Empate"
    };
    public static string ElementClass(ZodiacElement value) => value.ToString().ToLowerInvariant();
}
