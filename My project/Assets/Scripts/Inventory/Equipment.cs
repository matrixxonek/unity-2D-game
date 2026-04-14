using UnityEngine;

public class Equipment : MonoBehaviour
{
    [Header("Equipment Settings")]
    public ItemData[] equipment = new ItemData[5];

    [Header("Visuals")]
    public Transform handPoint;
    private GameObject currentWeaponVisual;

    public void Equip(ItemData newItem)
    {
        if (newItem.type != ItemData.Type.Equipment)
        {
            Debug.Log("This item can not be equiped.");
            return;
        }

        int index = (int)newItem.slot;

        if (equipment[index] != null)
        {
            Unequip(index);
        }

        equipment[index] = newItem;
        Debug.Log($"Założono {newItem.itemName} na slot {newItem.slot}");

        if (newItem.slot == ItemData.EquipSlot.Weapon)
        {
            SpawnWeaponVisual(newItem);
            newItem.weaponPrefab.GetComponent<Weapon>().ToggleEquip(true);

        }
    }

    public void Unequip(int index)
    {
        ItemData itemToRemove = equipment[index];
        if (itemToRemove == null) return;

        if ((ItemData.EquipSlot)index == ItemData.EquipSlot.Weapon && currentWeaponVisual != null)
        {
            Destroy(currentWeaponVisual);
        }
        if (itemToRemove.slot == ItemData.EquipSlot.Weapon)
        {
            itemToRemove.weaponPrefab.GetComponent<Weapon>().ToggleEquip(false);

        }

        equipment[index] = null;
    }

    private void SpawnWeaponVisual(ItemData weaponData)
    {
        if (weaponData.weaponPrefab != null)
        {
            currentWeaponVisual = Instantiate(weaponData.weaponPrefab, handPoint);
        }
    }
}