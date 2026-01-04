using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConsumableType { Health, Mana, Stamina, Buff } // İksir türleri

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableItem : ItemData
{
    [Header("Tüketilebilir Özellikleri")]
    public ConsumableType type;
    public float restoreValue = 20f; // Ne kadar dolduracağı

    public override void Use()
    {
        base.Use();

        // Karakteri bul
        StarterAssets.ThirdPersonController player = GameObject.FindObjectOfType<StarterAssets.ThirdPersonController>();
        if (player == null) return;

        // Türüne göre işlem yap
        switch (type)
        {
            case ConsumableType.Health:
                ApplyHealing(player);
                break;
            case ConsumableType.Mana:
                Debug.Log("Mana dolduruldu (Henüz sistem yok)");
                break;
            case ConsumableType.Stamina:
                Debug.Log("Enerji dolduruldu");
                break;
        }
    }

    private void ApplyHealing(StarterAssets.ThirdPersonController player)
    {
        player.currentHealth += restoreValue;
        if (player.currentHealth > player.maxHealth) player.currentHealth = player.maxHealth;
        
        // UI'ı güncelle
        if (player.playerHealthSlider != null)
            player.playerHealthSlider.value = player.currentHealth;
            
        Debug.Log(itemName + " ile can basıldı: " + restoreValue);
    }
}