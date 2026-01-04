using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootNotificationManager : MonoBehaviour
{
    public static LootNotificationManager Instance;
    public GameObject notificationPrefab; // Hazırladığın prefab
    public Transform notificationParent;  // Bildirimlerin alt alta dizileceği dikey alan (Vertical Layout Group önerilir)

    void Awake() => Instance = this;

    public void ShowLoot(ItemData item, int amount)
    {
        GameObject go = Instantiate(notificationPrefab, notificationParent);
        // Prefab üzerindeki basit bir script ile ikon ve yazıyı set edeceğiz
        go.GetComponent<LootNotificationUI>().Setup(item, amount);
        
        // 3 saniye sonra bildirimi yok et
        Destroy(go, 3f);
    }
}