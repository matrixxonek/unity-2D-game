using UnityEngine;

public class AmmoPickup : Pickup
{
    public enum Kind { Pistol, Revolver }

    public Kind kind = Kind.Pistol;
    public int amount = 6;

    protected override bool Apply(GameObject player)
    {
        var pouch = player.GetComponent<AmmoPouch>();
        if (pouch == null) return false;

        if (kind == Kind.Pistol)
        {
            pouch.AddPistol(amount);
            HUD.Message($"+{amount} naboi do pistoletu", 1.1f);
        }
        else
        {
            pouch.AddRevolver(amount);
            HUD.Message($"+{amount} {(amount == 1 ? "nabój" : "naboje")} do rewolweru", 1.3f);
        }
        return true;
    }

    protected override Color ParticleColor =>
        kind == Kind.Pistol ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.55f, 0.25f);
}
