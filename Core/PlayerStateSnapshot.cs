using System;

/// <summary>A completed scan for presentation; individual memory reads are not atomic.</summary>
public sealed class PlayerStateSnapshot
{
    public string Name { get; }
    public ushort X { get; }
    public ushort Y { get; }
    public long Life { get; }
    public long MaxLife { get; }
    public long Mana { get; }
    public long MaxMana { get; }
    public long AreaId { get; }
    public ushort Difficulty { get; }
    public uint MapSeed { get; }
    public DateTime CapturedAtUtc { get; }

    public PlayerStateSnapshot(string name, ushort x, ushort y, long life, long maxLife,
        long mana, long maxMana, long areaId, ushort difficulty, uint mapSeed)
    {
        Name = name ?? string.Empty;
        X = x;
        Y = y;
        Life = life;
        MaxLife = maxLife;
        Mana = mana;
        MaxMana = maxMana;
        AreaId = areaId;
        Difficulty = difficulty;
        MapSeed = mapSeed;
        CapturedAtUtc = DateTime.UtcNow;
    }
}
