using UnityEngine;

/// Indeksy warstw używane przez grę. Builder poziomu nadaje im nazwy w TagManagerze.
public static class Layers
{
    public const int Ground = 6;
    public const int Player = 8;
    public const int Enemy = 9;
    public const int Hazard = 10;

    public static int GroundMask => 1 << Ground;
}
