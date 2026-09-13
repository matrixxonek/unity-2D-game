using System;
using UnityEngine;

public enum Ability { DoubleJump, Pistol, Revolver }

/// Co postać już potrafi / posiada. Bramki metroidvanii w jednym miejscu.
public class PlayerAbilities : MonoBehaviour
{
    [Header("Odblokowane")]
    public bool doubleJump;
    public bool pistol;
    public bool revolver;

    public event Action<Ability> Unlocked;

    public bool Has(Ability ability)
    {
        switch (ability)
        {
            case Ability.DoubleJump: return doubleJump;
            case Ability.Pistol: return pistol;
            case Ability.Revolver: return revolver;
            default: return false;
        }
    }

    public void Unlock(Ability ability)
    {
        if (Has(ability)) return;

        switch (ability)
        {
            case Ability.DoubleJump: doubleJump = true; break;
            case Ability.Pistol: pistol = true; break;
            case Ability.Revolver: revolver = true; break;
        }
        Unlocked?.Invoke(ability);
    }
}
