using UnityEngine;
using System.Collections.Generic;
using System.Text;

public enum BodyPartType 
{ 
    Hair,   // Saç
    Head,   // Kafa (Tamamen)
    Torso,  // Gövde
    Arms,   // Kollar
    Legs,   // Bacaklar
    Hands,  // Eller
    Feet    // Ayaklar
}
public enum EquipSlot { Head, Chest, Legs, Weapon, Shield, Pet }

[CreateAssetMenu(fileName = "NewEquippable", menuName = "Inventory/Equippable")]
public class EquippableItem : ItemData
{
    [Header("Ekipman Türü")]
    public EquipSlot equipSlot;
    
    [Header("Gizlenecek Vücut Parçaları")]
    [Tooltip("Bu eşya kuşanıldığında vücudun hangi bölümleri gizlensin?")]
    public List<BodyPartType> hiddenBodyParts = new List<BodyPartType>();
    
    [Header("İstatistikler (Statlar)")]
    [Min(0)] public int damageBonus;
    [Min(0)] public int armorBonus; 

    [Header("Görsel Ayarlar")]
    public GameObject itemPrefab; 

    [Header("Pozisyon ve Rotasyon (Elde Duruş)")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.one;

    // UI'daki sağ taraftaki metin alanını dolduran fonksiyon
    public string GetStatsText()
    {
        StringBuilder sb = new StringBuilder();
        if (damageBonus > 0) sb.AppendLine($"<color=#FF4444>Hasar: +{damageBonus}</color>");
        if (armorBonus > 0) sb.AppendLine($"<color=#44FF44>Zırh: +{armorBonus}</color>");
        if (sb.Length == 0) return "Sıradan Eşya";
        return sb.ToString();
    }

    public override void Use()
    {
        // Envanterden tıklandığında EquipmentManager üzerinden kuşan
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.Equip(this);
            Debug.Log($"{itemName} kuşanıldı.");
        }
        else
        {
            Debug.LogError("Sahnede EquipmentManager bulunamadı!");
        }
    }
}