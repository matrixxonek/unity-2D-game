using UnityEngine;

public class Gun : Weapon
{
    private Ammo ammo;

    void Start()
    {
        ammo = GetComponent<Ammo>();
    }

    public override void Attack()
    {
        if (!isEquipped) return;

        if (!ammo.isMagazineEmpty){
            ammo.DecreaseAmmo();
            Debug.Log("Attacking");
        }
        else
            Debug.Log("Out of ammo! Press R to reload.");
    }
    
}