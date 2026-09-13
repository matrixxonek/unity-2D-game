using UnityEngine;

public class WeaponPickup : Pickup
{
    public Ability weapon = Ability.Pistol;
    public int ammo = 12;

    protected override bool Apply(GameObject player)
    {
        var abilities = player.GetComponent<PlayerAbilities>();
        var pouch = player.GetComponent<AmmoPouch>();
        if (abilities == null) return false;

        abilities.Unlock(weapon);
        if (pouch != null)
        {
            if (weapon == Ability.Pistol) pouch.AddPistol(ammo);
            else pouch.AddRevolver(ammo);
        }

        HUD.Message(weapon == Ability.Pistol ? "PISTOLET — K strzela, R przeładowuje" : "REWOLWER — TAB przełącza broń", 2.6f);
        CameraShake.Shake(0.15f, 0.2f);
        return true;
    }

    protected override string SfxKey => "unlock";
    protected override Color ParticleColor => new Color(0.8f, 0.85f, 1f);
}
