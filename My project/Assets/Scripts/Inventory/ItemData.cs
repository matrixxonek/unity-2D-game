using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Equipment/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public Type type;
    public enum Type
    {
        Equipment,
        Material
    }

    public enum EquipSlot
    {
        Head = 0,
        Chest = 1,
        Legs = 2,
        Boots = 3,
        Weapon = 4
    }
    public EquipSlot slot;
    
    [Header("Visuals")]
    public GameObject weaponPrefab; 
    public Sprite itemSprite;      
}