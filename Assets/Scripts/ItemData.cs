using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum ItemType { Consumable, Weapon, Armor, General } // Eşya türleri

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Bilgileri")]
    public string itemName = "Yeni Eşya";
    public ItemType itemType; // ⭐ Eklendi
    public string itemDescription = "Bu eşya hakkında bilgi.";
    public Sprite itemIcon;
    public bool isStackable = false;
    public int maxStackSize = 1;

    [Header("Özellikler")]
    public float value; // Can yenileme miktarı veya Hasar/Zırh değeri

    public virtual void Use()
    {
        Debug.Log(itemName + " kullanıldı!");
        // Temel kullanım mantığı burada kalabilir
    }
}