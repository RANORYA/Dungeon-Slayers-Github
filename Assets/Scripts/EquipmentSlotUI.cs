using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public EquipSlot slotType;
    
    [Header("UI Bileşenleri")]
    public TMP_Text statText;
    public Image iconDisplay;
    [HideInInspector] public EquippableItem currentItem;

    private Vector3 originalPosition; // Bu senin eski koddan kalma (World Pos), dokunmadım.
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    // ⭐ YENİ: İkonun varsayılan yerel ayarları
    private Vector3 defaultLocalPosition;
    private Vector3 defaultScale;

    private void Awake()
    {
        canvasGroup = iconDisplay.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = iconDisplay.gameObject.AddComponent<CanvasGroup>();

        // ⭐ KRİTİK EKLEME: İkonun Editör'deki duruşunu kaydediyoruz.
        defaultLocalPosition = iconDisplay.transform.localPosition;
        defaultScale = iconDisplay.transform.localScale;
    }

    private void Start()
    {
        UpdateSlotUI();
    }

    // --- EKİPMANDAN ENVANTERE ÇIKARMA ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        
        originalPosition = iconDisplay.transform.position;
        originalParent = iconDisplay.transform.parent;
        
        iconDisplay.transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem != null) iconDisplay.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        bool addedBack = InventoryService.Instance.Add(currentItem, 1);
        
        if (addedBack)
        {
            EquipmentManager.Instance.Unequip((int)slotType);
        }

        // Görseli eski yerine çek
        iconDisplay.transform.SetParent(originalParent);
        
        // ⭐ DÜZELTME: Vector3.zero yerine kaydettiğimiz orijinal konumu kullanıyoruz.
        // Böylece senin Inspector'da ayarladığın X, Y, Z değerleri korunuyor.
        iconDisplay.transform.localPosition = defaultLocalPosition;
        iconDisplay.transform.localScale = defaultScale;

        canvasGroup.blocksRaycasts = true;
    }

    // --- ENVANTERDEN GELENİ KUŞANMA ---
    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI sourceSlot = eventData.pointerDrag.GetComponent<InventorySlotUI>();

        if (sourceSlot != null)
        {
            ItemData draggedItem = InventoryService.Instance.slots[sourceSlot.slotIndex].item;

            if (draggedItem is EquippableItem equippable)
            {
                if (equippable.equipSlot == slotType)
                {
                    EquipmentManager.Instance.Equip(equippable);
                    InventoryService.Instance.RemoveFromSlot(sourceSlot.slotIndex, 1);
                    Debug.Log("Kuşanıldı: " + equippable.itemName);
                }
            }
        }
    }

    public void SetEquipment(EquippableItem newItem)
    {
        currentItem = newItem;
        if (newItem != null)
        {
            iconDisplay.sprite = newItem.itemIcon;
            iconDisplay.enabled = true;
            iconDisplay.color = Color.white;
            
            // Eğer statik bir pozisyon/scale sıfırlaması gerekirse buraya da eklenebilir
            // Ama Awake'te aldığımız değerler sabit olduğu için, sadece Drag bitişinde düzeltmek yeterli.
            
            if (statText != null) 
            {
                statText.text = newItem.GetStatsText();
            }
        }
        else
        {
            iconDisplay.enabled = false;
            iconDisplay.sprite = null;
            if (statText != null) statText.text = ""; 
        }
    }

    private void UpdateSlotUI()
    {
        if (currentItem != null)
        {
            iconDisplay.sprite = currentItem.itemIcon;
            iconDisplay.enabled = true;
            if (statText != null) statText.text = currentItem.GetStatsText(); 
        }
        else
        {
            iconDisplay.enabled = false;
            if (statText != null) statText.text = "";
            iconDisplay.sprite = null;
        }
    }
}