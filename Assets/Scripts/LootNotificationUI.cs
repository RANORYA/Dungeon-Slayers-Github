using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LootNotificationUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText; // İsmi 'quantityText' olarak güncelledik

    public void Setup(ItemData item, int amount)
    {
        if (quantityText != null)
        {
            icon.sprite = item.itemIcon;
            // Sadece adet bilgisini yazıyoruz (Örn: x5 veya 5)
            quantityText.text = "x" + amount.ToString(); 
        }
    }
}