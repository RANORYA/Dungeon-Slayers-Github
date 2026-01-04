using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets; // ThirdPersonController erişimi için

public class UniversalLootChest : Interactable
{
    [Header("Ganimet Listesi (El ile Eklemek İçin)")]
    public List<LootItem> lootList = new List<LootItem>();
    
    [Header("Rastgele Eşya Havuzu (Otomatik Dolum İçin)")]
    public List<ItemData> possibleItems = new List<ItemData>();
    
    [Header("Görsel Ayarlar")]
    public Animator animator;
    public string openAnimationName = "Open";

    private bool isOpened = false;

    [System.Serializable]
    public class LootItem {
        public ItemData item;
        public int amount = 1;
    }

    void Start()
    {
        // Eğer liste boşsa havuzdan rastgele eşyalar seç
        if (lootList.Count == 0 && possibleItems.Count > 0)
        {
            GenerateRandomLoot();
        }
    }

    // ⭐ TUŞA BASILDIĞINDA ÇALIŞACAK KISIM (OnTriggerEnter yerine)
    public override void Interact(ThirdPersonController characterController)
    {
        if (!isOpened)
        {
            isOpened = true;
            StartCoroutine(OpenAndLootRoutine());
        }
    }

    IEnumerator OpenAndLootRoutine()
    {
        // 1. Fizik engelini kaldır (Karakter içinden geçebilsin)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Animasyonu Başlat
        if (animator != null) 
        {
            animator.SetTrigger(openAnimationName);
        }

        // 3. Eşyaları Envantere Aktar ve Bildirimleri Göster
        TransferItemsToInventory();

        // 4. BEKLEME SÜRESİ: Animasyon için 3 saniye bekle
        yield return new WaitForSeconds(3.0f);

        // 5. SİLME
        Destroy(gameObject);
    }

    void TransferItemsToInventory()
    {
        for (int i = lootList.Count - 1; i >= 0; i--)
        {
            bool added = InventoryService.Instance.Add(lootList[i].item, lootList[i].amount);
            if (added)
            {
                if (LootNotificationManager.Instance != null)
                {
                    LootNotificationManager.Instance.ShowLoot(lootList[i].item, lootList[i].amount);
                }
                lootList.RemoveAt(i);
            }
        }
    }

    void GenerateRandomLoot()
    {
        int randomCount = Random.Range(2, 6);
        for (int i = 0; i < randomCount; i++)
        {
            ItemData randomData = possibleItems[Random.Range(0, possibleItems.Count)];
            LootItem newItem = new LootItem {
                item = randomData,
                amount = Random.Range(1, 5)
            };
            lootList.Add(newItem);
        }
    }
}