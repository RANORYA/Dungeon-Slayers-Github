using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBodyManager : MonoBehaviour
{
    public static CharacterBodyManager Instance;

    [System.Serializable]
    public struct BodyPartEntry
    {
        public string partName;       // Editörde karışmasın diye isim (Örn: "Saç")
        public BodyPartType type;     // Türü (Enum'dan seç)
        public GameObject meshObject; // Karakter üzerindeki gerçek obje (SkinnedMeshRenderer)
    }

    [Header("Vücut Parçaları Tanımları")]
    public List<BodyPartEntry> bodyParts = new List<BodyPartEntry>();

    private void Awake()
    {
        Instance = this;
    }

    // EquipmentManager burayı çağıracak
    public void UpdateBodyVisibilities(EquippableItem[] currentEquipment)
    {
        // 1. Önce her şeyi GÖRÜNÜR yap (Sıfırla)
        foreach (var part in bodyParts)
        {
            if (part.meshObject != null)
                part.meshObject.SetActive(true);
        }

        // 2. Takılı olan tüm ekipmanları gez
        if (currentEquipment != null)
        {
            foreach (var item in currentEquipment)
            {
                if (item != null)
                {
                    // Bu eşya bir şeyleri gizlemek istiyor mu?
                    foreach (var hiddenPartType in item.hiddenBodyParts)
                    {
                        HidePart(hiddenPartType);
                    }
                }
            }
        }
    }

    private void HidePart(BodyPartType type)
    {
        // Listeden bu tipe uyan parçayı bul ve kapat
        foreach (var part in bodyParts)
        {
            if (part.type == type && part.meshObject != null)
            {
                part.meshObject.SetActive(false);
            }
        }
    }
}