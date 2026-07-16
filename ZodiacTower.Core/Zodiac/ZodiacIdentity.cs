using System.Collections.Generic;

namespace ZodiacTower.Core.Zodiac
{

public enum ZodiacSign
{
    Aries,
    Taurus,
    Gemini,
    Cancer,
    Leo,
    Virgo,
    Libra,
    Scorpio,
    Sagittarius,
    Capricorn,
    Aquarius,
    Pisces
}

public enum ZodiacElement { Fire, Earth, Air, Water }
public enum Polarity { Light, Dark }
public enum Modality { Cardinal, Fixed, Mutable }

public sealed class ZodiacProfile
{
    public ZodiacSign Sign { get; }
    public ZodiacElement Element { get; }
    public Polarity Polarity { get; }
    public Modality Modality { get; }

    public ZodiacProfile(ZodiacSign sign, ZodiacElement element, Polarity polarity, Modality modality)
    {
        Sign = sign;
        Element = element;
        Polarity = polarity;
        Modality = modality;
    }
}

public static class ZodiacCatalog
{
    private static readonly ZodiacProfile[] Profiles =
    {
        new(ZodiacSign.Aries, ZodiacElement.Fire, Polarity.Light, Modality.Cardinal),
        new(ZodiacSign.Taurus, ZodiacElement.Earth, Polarity.Dark, Modality.Fixed),
        new(ZodiacSign.Gemini, ZodiacElement.Air, Polarity.Light, Modality.Mutable),
        new(ZodiacSign.Cancer, ZodiacElement.Water, Polarity.Dark, Modality.Cardinal),
        new(ZodiacSign.Leo, ZodiacElement.Fire, Polarity.Light, Modality.Fixed),
        new(ZodiacSign.Virgo, ZodiacElement.Earth, Polarity.Dark, Modality.Mutable),
        new(ZodiacSign.Libra, ZodiacElement.Air, Polarity.Light, Modality.Cardinal),
        new(ZodiacSign.Scorpio, ZodiacElement.Water, Polarity.Dark, Modality.Fixed),
        new(ZodiacSign.Sagittarius, ZodiacElement.Fire, Polarity.Light, Modality.Mutable),
        new(ZodiacSign.Capricorn, ZodiacElement.Earth, Polarity.Dark, Modality.Cardinal),
        new(ZodiacSign.Aquarius, ZodiacElement.Air, Polarity.Light, Modality.Fixed),
        new(ZodiacSign.Pisces, ZodiacElement.Water, Polarity.Dark, Modality.Mutable)
    };

    public static IReadOnlyList<ZodiacProfile> All => Profiles;

    public static ZodiacProfile Get(ZodiacSign sign) => Profiles[(int)sign];
}
}
