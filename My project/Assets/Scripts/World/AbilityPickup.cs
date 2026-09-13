using UnityEngine;

public class AbilityPickup : Pickup
{
    public Ability ability = Ability.DoubleJump;

    protected override bool Apply(GameObject player)
    {
        var abilities = player.GetComponent<PlayerAbilities>();
        if (abilities == null) return false;

        abilities.Unlock(ability);
        HUD.Message("PODWÓJNY SKOK — wciśnij skok w powietrzu", 2.6f);
        CameraShake.Shake(0.15f, 0.2f);
        HitStop.Do(0.1f);
        return true;
    }

    protected override string SfxKey => "unlock";
    protected override Color ParticleColor => new Color(0.5f, 0.95f, 1f);
}
