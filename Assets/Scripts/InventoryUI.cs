using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel; // Unity Editor'da atayacağımız ana panel
    public Transform itemsParent;     // Slotları yerleştireceğimiz parent Transform
    
    // ⭐ EKLENDİ: Slot Prefab ve Slot Dizisi tanımları
    public GameObject slotPrefab;
    public InventorySlotUI[] slots; 
    
    private StarterAssetsInputs _input;

    // InventoryService'e abone olacağız
    private InventoryService _inventory; 

    void Start()
    {
        // InventoryService'in Awake metodu çoktan bittiği için Instance artık garanti altındadır.
        _inventory = InventoryService.Instance;

        if (_inventory != null)
        {
            // 1. Önce UI dizisini hazırlıyoruz
            slots = new InventorySlotUI[_inventory.inventoryCapacity];
            
            // 2. Slotları fiziksel olarak oluşturuyoruz
            SetupSlots();

            // 3. Değişikliklere abone oluyoruz
            _inventory.onInventoryChangedCallback += UpdateUI;

            // 4. Panel içindeki verileri bir kez tazeliyoruz
            UpdateUI();
        }
        else
        {
            Debug.LogError("InventoryUI: InventoryService bulunamadı! Script Execution Order'ı kontrol edin.");
        }

        // En son paneli kapatıyoruz (Slotlar oluştuktan sonra)
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    // Input System'den gelen Toggle metodunu buraya taşıdık.
    public void ToggleInventoryUI()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        // Sadece envanter açıldığında GameManager üzerinden HUD'ı kontrol et
        if (GameManager.Instance != null && GameManager.Instance.inGameHUD != null)
        {
            GameManager.Instance.inGameHUD.SetActive(!isActive);
        }

        // Zaman ve Mouse ayarları
        Time.timeScale = isActive ? 0f : 1f;
        Cursor.visible = isActive;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;

        if (isActive) UpdateUI();
    }

    void SetupSlots()
    {
        // Referans kontrolü yapıyoruz. itemsParent de atanmış olmalı.
        if (_inventory == null || slotPrefab == null || itemsParent == null) 
        {
            Debug.LogError("SetupSlots hata: Referanslardan biri eksik (InventoryService, SlotPrefab veya ItemsParent).");
            return; 
        }
        
        // Slotları sadece bir kez oluştur
        for (int i = 0; i < _inventory.inventoryCapacity; i++)
        {
            // Slot prefab'ı itemsParent altına çoğalt
            GameObject slotObj = Instantiate(slotPrefab, itemsParent);
            
            // InventorySlotUI betiğini yakala
            slots[i] = slotObj.GetComponent<InventorySlotUI>(); 
            
            if (slots[i] != null)
            {   
                slots[i].SetIndex(i);
                slots[i].UpdateSlot(null); // Başlangıçta boş slot olarak ayarla
            }
            else
            {
                // Bu hata, InventorySlot_Prefab üzerine InventorySlotUI betiğinin atanmadığını gösterir.
                Debug.LogError("SetupSlots hata: Slot Prefab üzerinde InventorySlotUI.cs betiği eksik!");
            }
        }
    }

    // Envanter değiştiğinde (eşya eklendi/çıkarıldı) çağrılır
    void UpdateUI()
    {
        // InventoryService'deki slot verilerini al
        InventorySlot[] inventoryData = _inventory.slots;

        if (slots == null || inventoryData == null) return;
        
        // UI'daki slotlar üzerinde döngü yap
        for (int i = 0; i < slots.Length; i++)
        {
            // i: UI Slotunun indeksi
            // inventoryData[i]: InventoryService'deki i. slotun verisi
            
            // Kapasitenin dışına çıkmadığımızı kontrol etmeye gerek yok, 
            // çünkü slots dizisi zaten kapasiteye göre oluşturuldu.
            
            InventorySlot slotData = inventoryData[i];

            if (slotData.item != null)
            {
                // Slotta eşya varsa, itemData ve miktarı ile güncelle
                slots[i].UpdateSlot(slotData.item, slotData.amount);
            }
            else
            {
                // Slotta eşya yoksa slotu boşalt
                slots[i].UpdateSlot(null);
            }
        }
    }
} // ⭐ YENİ KONUM: Bu parantez tüm sınıfı kapatır.