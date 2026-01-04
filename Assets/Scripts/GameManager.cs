using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject gameOverPanel;
    public GameObject inGameHUD; // Joystick ve diğer butonların olduğu grup

    public void Awake()
    {
        Instance = this;
    }
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        if (inGameHUD != null) inGameHUD.SetActive(false); // HUD'ı gizle

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // "Revive" veya "Yeniden Başla" butonuna basınca çalışacak
    public void RestartGame()
    {
        // Sahneyi yeniden yüklediğimiz için HUD otomatik olarak varsayılan (açık) haline döner.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Eğer sahneyi yeniden yüklemeden sadece canlandırmak istersen (Revive):
    public void RevivePlayer()
    {
        gameOverPanel.SetActive(false);
        if (inGameHUD != null) inGameHUD.SetActive(true); // HUD'ı geri getir
        
        // Burada oyuncunun canını full'leme ve animasyonunu düzeltme kodları eklenmeli
    }
}