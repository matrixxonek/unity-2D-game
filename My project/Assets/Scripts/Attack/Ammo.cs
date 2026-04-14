using UnityEngine;
using UnityEngine.InputSystem; 


public class Ammo : MonoBehaviour
{
    public int magazineSize = 12;
    public int currentAmmo;
    public bool isMagazineEmpty => currentAmmo <= 0; // "Property" zamiast zmiennej - czytelniej!

    void Start() => Reload();

    public void Reload() => currentAmmo = magazineSize;

    public void DecreaseAmmo()
    {
        if (currentAmmo > 0) currentAmmo--;
    }
    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Reload();
        }
    }
}