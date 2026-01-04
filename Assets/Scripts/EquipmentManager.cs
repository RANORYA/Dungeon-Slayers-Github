using System.Collections;
using System.Collections.Generic; // Dictionary ve List için
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Karakter Eklem Noktaları")]
    public Transform weaponHoldPoint; 
    public Transform headPoint;
    public Transform chestPoint; 
    public Transform legsPoint;
    
    [Header("Sahnedeki UI Slotları")]
    public List<EquipmentSlotUI> uiSlots = new List<EquipmentSlotUI>();

    private Dictionary<EquipSlot, GameObject> spawnedObjects = new Dictionary<EquipSlot, GameObject>();
    private EquippableItem[] currentEquipment;

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        
        int slotCount = System.Enum.GetNames(typeof(EquipSlot)).Length;
        currentEquipment = new EquippableItem[slotCount];
    }

    public void Equip(EquippableItem newItem)
    {
        if (newItem == null) return;
        int slotIndex = (int)newItem.equipSlot;
        
        Unequip(slotIndex); // Önce eski eşyayı çıkar
        currentEquipment[slotIndex] = newItem;

        if (newItem.itemPrefab != null)
        {
            SpawnEquipModel(newItem);
        }

        // UI Güncelleme
        if (uiSlots != null)
        {
            foreach (EquipmentSlotUI uiSlot in uiSlots)
            {
                if (uiSlot != null && uiSlot.slotType == newItem.equipSlot)
                {
                    uiSlot.SetEquipment(newItem);
                    break;
                }
            }
        }

        // --- EKLENEN KISIM: VÜCUT PARÇALARINI GÜNCELLE ---
        // Bir eşya giyildiğinde (örneğin kask), vücut yöneticisine haber ver ki saçı gizlesin.
        if (CharacterBodyManager.Instance != null)
        {
            CharacterBodyManager.Instance.UpdateBodyVisibilities(currentEquipment);
        }
    }

    public void Unequip(int slotIndex)
    {
        if (currentEquipment[slotIndex] != null)
        {
            EquipSlot slotEnum = (EquipSlot)slotIndex;
            
            if (spawnedObjects.ContainsKey(slotEnum))
            {
                if (spawnedObjects[slotEnum] != null)
                {
                    Destroy(spawnedObjects[slotEnum]);
                }
                spawnedObjects.Remove(slotEnum);
            }
            currentEquipment[slotIndex] = null;

            if (uiSlots != null)
            {
                foreach (EquipmentSlotUI uiSlot in uiSlots)
                {
                    if (uiSlot != null && (int)uiSlot.slotType == slotIndex)
                    {
                        uiSlot.SetEquipment(null); 
                        break;
                    }
                }
            }

            // 4. VÜCUT PARÇALARINI GÜNCELLE (Eşya çıktı, saç geri gelsin)
            if (CharacterBodyManager.Instance != null)
            {
                CharacterBodyManager.Instance.UpdateBodyVisibilities(currentEquipment);
            }
        }
    }

    void SpawnEquipModel(EquippableItem item)
    {
        if (item.itemPrefab == null)
        {
            Debug.LogError(item.itemName + " için Prefab atanmamış!");
            return;
        }

        // 1. Obje Oluşturma
        GameObject model = Instantiate(item.itemPrefab);
        Debug.Log(item.itemName + " başarıyla oluşturuldu."); // Bunu konsolda görmelisin

        SkinnedMeshRenderer armorRenderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer characterRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        // 2. Yerleştirme Mantığı
        if (armorRenderer != null && characterRenderer != null && (item.equipSlot == EquipSlot.Chest || item.equipSlot == EquipSlot.Legs))
        {
            armorRenderer.bones = characterRenderer.bones;
            armorRenderer.rootBone = characterRenderer.rootBone;

            // Karakterin ana objesine (parent) bağla
            model.transform.SetParent(characterRenderer.transform.parent);
            
            model.transform.localPosition = item.positionOffset;
            model.transform.localRotation = Quaternion.Euler(item.rotationOffset);
            
            Debug.Log("Rigli zırh (Legs/Chest) olarak yerleştirildi.");
        }
        else
        {
            Transform target = GetTargetPoint(item.equipSlot);
            if (target != null)
            {
                model.transform.SetParent(target);
                model.transform.localPosition = item.positionOffset;
                model.transform.localRotation = Quaternion.Euler(item.rotationOffset);
                Debug.Log("Statik eşya olarak " + target.name + " altına yerleştirildi.");
            }
            else
            {
                Debug.LogError(item.equipSlot + " için Target Point bulunamadı! Hierarchy'de oluşmamasının sebebi bu olabilir.");
            }
        }

        // Ölçek ayarı
        Vector3 originalScale = item.itemPrefab.transform.localScale;
        model.transform.localScale = new Vector3(
            originalScale.x * item.scaleOffset.x,
            originalScale.y * item.scaleOffset.y,
            originalScale.z * item.scaleOffset.z
        );

        UpdateSpawnedDictionary(item.equipSlot, model);
    }

    public EquippableItem GetCurrentItem(EquipSlot slot)
    {
        if (currentEquipment == null) return null;
        int index = (int)slot;
        if (index >= 0 && index < currentEquipment.Length)
        {
            return currentEquipment[index];
        }
        return null;
    }
    // GetTargetPoint Hatasını Çözen Fonksiyon
    Transform GetTargetPoint(EquipSlot slot)
    {
        switch (slot)
        {
            case EquipSlot.Weapon: return weaponHoldPoint;
            case EquipSlot.Head:   return headPoint;
            case EquipSlot.Chest:  return chestPoint;
            case EquipSlot.Legs:   return legsPoint;
            default: return null;
        }
    }

    // UpdateSpawnedDictionary Hatasını Çözen Fonksiyon
    void UpdateSpawnedDictionary(EquipSlot slot, GameObject newModel)
    {
        if (spawnedObjects.ContainsKey(slot))
        {
            if (spawnedObjects[slot] != null) Destroy(spawnedObjects[slot]);
            spawnedObjects[slot] = newModel;
        }
        else
        {
            spawnedObjects.Add(slot, newModel);
        }
    }
}