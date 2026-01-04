using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public InventoryUI inventoryUI_Script;
    public GameObject inGameHUD; // Inspector'dan InGameHUD objesini buraya sürükle

    private StarterAssetsInputs _input;

    private void Start()
    {
        _input = FindObjectOfType<StarterAssetsInputs>();
    }

    private void Update()
    {
        if (_input == null) return;

        if (_input.inventory)
        {
            ToggleInventory();
            _input.InventoryInput(false);
        }
    }

    public void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            bool isOpening = !inventoryUI.activeSelf;
            
            // Paneli aç/kapat
            inventoryUI.SetActive(isOpening);
            
            // HUD'ı aç/kapat (Envanter açılınca HUD kapanır)
            if (inGameHUD != null)
            {
                inGameHUD.SetActive(!isOpening);
            }

            // Oyun kontrol ayarları
            Time.timeScale = isOpening ? 0f : 1f;
            Cursor.visible = isOpening;
            Cursor.lockState = isOpening ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}