using System;

namespace ZodiacTower.Core.Random
{

public interface IRandomSource
{
    int Next(int minimumInclusive, int maximumExclusive);
}

public sealed class StableRandom : IRandomSource
{
    private uint _state;

    public StableRandom(int seed)
    {
        _state = unchecked((uint)seed);
        if (_state == 0)
            _state = 0x6D2B79F5u;
    }

    public int Next(int minimumInclusive, int maximumExclusive)
    {
        if (maximumExclusive <= minimumInclusive)
            throw new ArgumentOutOfRangeException(nameof(maximumExclusive));

        uint value = NextUInt();
        uint range = (uint)(maximumExclusive - minimumInclusive);
        return minimumInclusive + (int)(value % range);
    }

    private uint NextUInt()
    {
        uint x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        _state = x;
        return x;
    }
}
}
