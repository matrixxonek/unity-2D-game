using Unity.VisualScripting;
using UnityEngine;

public abstract class Weapon : MonoBehaviour 
{
    public bool isEquipped = false;
    public abstract void Attack();

    public void ToggleEquip(bool state) => isEquipped = state;
}
