using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Inspector'da görünmesi için
public class InventorySlot
{
    public ItemData item;
    public int amount;

    // Başlangıçta slotu boş ayarlamak için yapıcı metot
    public InventorySlot()
    {
        item = null;
        amount = 0;
    }

    // Slota ilk kez eşya eklemek için
    public void SetItem(ItemData newItem, int newAmount)
    {
        item = newItem;
        amount = newAmount;
    }

    // Slottaki eşya miktarını artırmak için
    public void AddAmount(int quantity)
    {
        amount += quantity;
    }

    // Slotu temizlemek için
    public void ClearSlot()
    {
        item = null;
        amount = 0;
    }
}