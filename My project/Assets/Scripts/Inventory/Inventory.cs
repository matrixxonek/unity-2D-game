using UnityEngine;
using UnityEngine.InputSystem; 


public class Inventory : MonoBehaviour
{
    public Equipment playerEquipment;
    public static int inventorySize = 20; 
    public ItemData[] items = new ItemData[inventorySize];


    public bool AddItem(ItemData newItem)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) 
            {
                items[i] = newItem;
                Debug.Log($"Added {newItem.itemName} to slot number:  {i}");
                return true; 
            }
        }

        Debug.Log("Equipment is full!");
        return false;
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < items.Length)
        {
            items[index] = null;
        }
    }
    public void UseItem(int index)
    {
        ItemData itemToUse = items[index];

        if (itemToUse == null) return;

        if (itemToUse.type == ItemData.Type.Equipment)
        {
            ItemData oldItem = playerEquipment.equipment[(int)itemToUse.slot];

            playerEquipment.Equip(itemToUse);

            items[index] = oldItem; 
            
            Debug.Log($"Items on slot {index} swapped");
        }
    }
    void Update()
    {
        if (Keyboard.current.uKey.wasPressedThisFrame) 
        {
            UseItem(0); 
        }
    }
}