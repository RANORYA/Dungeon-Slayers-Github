using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryService : MonoBehaviour
{
    // Singleton patterni: Diğer betiklerin kolayca erişebilmesi için
    public static InventoryService Instance; 

    // Envanterdeki eşyaları tutacak liste (Item, Miktar)
    // Basitlik için sadece ItemData kullanacağız. Gerçek bir sistemde Slot'lar kullanılır.
    public List<ItemData> items = new List<ItemData>(); 
    public int inventoryCapacity = 20; // Envanterin maksimum kapasitesi
    public InventorySlot[] slots; 

    // Unity Event'ler: Envanter değiştiğinde UI'ı güncellemek için
    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChangedCallback;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ⭐ KRİTİK DEĞİŞİKLİK: Veriyi Start yerine burada, Instance'tan hemen sonra hazırla.
        slots = new InventorySlot[inventoryCapacity];
        for (int i = 0; i < inventoryCapacity; i++)
        {
            slots[i] = new InventorySlot();
        }
    }

    
    // --- EŞYA EKLEME ---
    public bool Add(ItemData item, int amount = 1)
    {
        // 1. EŞYANIN YIĞINLANABİLİR OLUP OLMADIĞINI KONTROL ET
        if (item.isStackable)
        {
            // Mevcut slotları dolaş (Stack yapmaya çalış)
            for (int i = 0; i < slots.Length; i++)
            {
                // Slot aynı eşyayı içeriyor ve tam dolu değilse
                if (slots[i].item == item && slots[i].amount < item.maxStackSize)
                {
                    // Ekleyebileceğimiz maksimum miktarı hesapla
                    int canAdd = item.maxStackSize - slots[i].amount;
                    int actualAdd = Mathf.Min(amount, canAdd);

                    slots[i].AddAmount(actualAdd);
                    amount -= actualAdd;

                    // Eğer eklenecek miktar 0'a düştüyse (hepsi stacklendi)
                    if (amount <= 0)
                    {
                        onInventoryChangedCallback?.Invoke();
                        return true;
                    }
                }
            }
        }

        // 2. YENİ SLOT AÇ (Stacklenecek yer kalmadıysa veya stacklenemezse)
        if (amount > 0)
        {
            // Tüm slotları dolaş ve boş slot bul
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == null)
                {
                    // Yeni slota kalan eşyayı ekle (maxStackSize'dan fazlasını eklemez)
                    int toAdd = Mathf.Min(amount, item.maxStackSize);
                    slots[i].SetItem(item, toAdd);
                    amount -= toAdd;
                    
                    if (amount <= 0)
                    {
                        onInventoryChangedCallback?.Invoke();
                        return true;
                    }
                    // Eğer kalan miktar hala varsa, bu döngü devam edip bir sonraki boş slota geçmelidir.
                }
            }
        }
        
        // 3. ENVANTER DOLU VEYA EKLENECEK YER KALMADI
        if (amount > 0)
        {
            Debug.Log("Envanter Dolu Veya Stacklenecek Yer Kalmadı! Kalan Eşya Miktarı: " + amount);
            // Eklenemeyen eşya için buraya düşen eşyayı düşürme kodu yazılabilir.
            return false;
        }

        onInventoryChangedCallback?.Invoke();
        return true;
    }
    // --- EŞYA ÇIKARMA ---
    public void Remove(ItemData item, int amountToRemove = 1)
    {
        // Çıkarma mantığı da yığın kontrolü gerektirir. Basitçe miktar düşürülür.
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item)
            {
                slots[i].amount -= amountToRemove;
                if (slots[i].amount <= 0)
                {
                    slots[i].ClearSlot();
                }
                onInventoryChangedCallback?.Invoke();
                return;
            }
        }
    }
    // --- EŞYA KULLANMA ---
    public void UseItem(int slotIndex)
    {
        InventorySlot slot = slots[slotIndex];
        if (slot.item == null) return;

        // 1. Eşyanın kendi Use metodunu çağır (Can ekleme burada olur)
        slot.item.Use(); 

        // 2. Miktarı azalt (Kritik: onInventoryChanged burada tetiklenmeli)
        RemoveFromSlot(slotIndex, 1);
    }

    // --- SLOTUN BELİRLİ MİKTARINI SİLME/ATMA ---
    public void RemoveFromSlot(int slotIndex, int amountToRemove)
    {
        if (slots[slotIndex].item != null)
        {
            slots[slotIndex].amount -= amountToRemove;

            if (slots[slotIndex].amount <= 0)
            {
                slots[slotIndex].ClearSlot();
            }

            onInventoryChangedCallback?.Invoke();
        }
    }

    public ItemData GetItemInSlot(int index)
    {
        if (index >= 0 && index < slots.Length)
        {
            return slots[index].item;
        }
        return null;
    }

    public bool AddItem(ItemData item)
    {
        // Envanterdeki Add metodunu kullanarak eşyayı boş bir yuvaya ekle
        bool success = Add(item, 1); 
        
        if (success)
        {
            // Ekranı tazele komutu Add metodu içinde yoksa burada mutlaka çağırılmalı
            onInventoryChangedCallback?.Invoke();
        }
        
        return success;
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < slots.Length)
        {
            slots[index].ClearSlot();
            onInventoryChangedCallback?.Invoke();
        }
    }



}
