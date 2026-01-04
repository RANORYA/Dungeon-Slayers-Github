using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI stackText;
    public int slotIndex;
    
    private ItemData item;
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    // ⭐ YENİ: Başlangıç ayarlarını saklayacağımız değişkenler
    private Vector3 defaultLocalPosition;
    private Vector3 defaultScale;

    private void Awake()
    {
        canvasGroup = icon.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
        
        icon.raycastTarget = true; 

        // ⭐ KRİTİK EKLEME: Oyun başlarken senin Editör'de ayarladığın konumu ve boyutu hafızaya atıyoruz.
        // Böylece sürükleme bitince tam olarak buraya dönecek, 0,0,0'a değil.
        defaultLocalPosition = icon.transform.localPosition;
        defaultScale = icon.transform.localScale;
    }

    public void SetIndex(int index)
    {
        slotIndex = index;
    }

    public void UpdateSlot(ItemData newItem, int quantity = 1)
    {
        item = newItem;
        if (item == null)
        {
            icon.enabled = false;
            if (stackText != null) stackText.enabled = false;
            return;
        }
        icon.sprite = item.itemIcon;
        icon.enabled = true;
        if (stackText != null)
        {
            stackText.text = quantity > 1 ? "x" + quantity.ToString() : "";
            stackText.enabled = quantity > 1; 
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return; 

        originalParent = icon.transform.parent;
        icon.transform.SetParent(transform.root); 
        
        canvasGroup.blocksRaycasts = false; 
        icon.color = new Color(1, 1, 1, 0.6f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item != null) icon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        icon.transform.SetParent(originalParent); 
        
        // ⭐ DÜZELTME: Artık Vector3.zero demiyoruz.
        // Başlangıçta kaydettiğimiz orijinal konuma ve boyuta döndürüyoruz.
        icon.transform.localPosition = defaultLocalPosition;
        icon.transform.localScale = defaultScale;

        canvasGroup.blocksRaycasts = true; 
        icon.color = Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item != null)
        {
            InventoryService.Instance.UseItem(slotIndex);
            Debug.Log(item.itemName + " kullanıldı.");
        }
    }
}