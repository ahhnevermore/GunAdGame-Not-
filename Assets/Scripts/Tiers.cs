using System.Collections.Generic;
using UnityEngine;
public enum Tier
{
    Normal,
    Ember,
    Ice,
    Arcane,
    Legendary
}
public static class Tiers
{

    public static readonly Dictionary<Tier, Color[]> UTColours =
     new Dictionary<Tier, Color[]>
     {
         [Tier.Normal] = new[] { Hex("#8A8F91"), Hex("#8A8F91"), Hex("#A4A4A4") },
         [Tier.Ember] = new[] { Hex("#FF4A3D"), Hex("#FF8A1E"), Hex("#FFD34A") },
         [Tier.Ice] = new[] { Hex("#4DD8FF"), Hex("#1FA2FF"), Hex("#D9F6FF") },
         [Tier.Arcane] = new[] { Hex("#C03CFF"), Hex("#FF4DD8"), Hex("#F8CCFF") },
         [Tier.Legendary] = new[] { Hex("#FFE45E"), Hex("#FFFFFF"), Hex("#C9F4FF") },

     };

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}