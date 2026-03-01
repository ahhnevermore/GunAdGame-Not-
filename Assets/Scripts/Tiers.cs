using System.Collections.Generic;
using UnityEngine;
public enum Tier
{
    Normal,
    Ice,
    Arcane,
    Legendary
}
public static class Tiers
{

    public static readonly Dictionary<Tier, Color[]> UTColours =
     new Dictionary<Tier, Color[]>
     {
         [Tier.Normal] = new[] { Hex("#8A8F91"), Hex("#515154"), Hex("#ffffff") },
         [Tier.Ice] = new[] { Hex("#008B8B"), Hex("#8fffea"), Hex("#05f5f5") },
         [Tier.Arcane] = new[] { Hex("#6c34dc"), Hex("#C03CFF"), Hex("#FF4DD8") },
         [Tier.Legendary] = new[] { Hex("#ff4a3d"), Hex("#ff9b3d"), Hex("#ffec3d") },

     };

    public static readonly Dictionary<GunType, double> GunPower =
    new Dictionary<GunType, double>
    {
        [GunType.Pistol] = 1d,
        [GunType.Rifle] = 1d,
        [GunType.Sniper] = 4d,

    };

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}