using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using StarterAssets; // StarterAssetsInputs'a erişmek için ekleyin

public class FixedJoystick : Joystick
{
    // StarterAssetsInputs'a bir referans tutmak için değişken
    [Header("Giriş Ayarları")]
    [Tooltip("StarterAssetsInputs scriptinin bulunduğu nesne")]
    public StarterAssetsInputs starterInputs;

    [Tooltip("Koşmanın tetikleneceği minimum joystick magnitüdü (örn: 0.8)")]
    public float runThreshold = 0.8f; 

    // Başlangıçta StarterAssetsInputs referansını bul
    protected override void Start()
    {
        base.Start();
        // StarterAssetsInputs'ı otomatik olarak bulmaya çalış
        if (starterInputs == null)
        {
            starterInputs = FindObjectOfType<StarterAssetsInputs>();
            if (starterInputs == null)
            {
                Debug.LogError("FixedJoystick: Sahneden StarterAssetsInputs bulunamadı!");
            }
        }
    }

    // Joystick hareket ettiğinde çağrılır
    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        base.HandleInput(magnitude, normalised, radius, cam);
        
        // Joystick'in güncel girişini StarterAssetsInputs'a gönder
        if (starterInputs != null)
        {
            // Yürüme/Koşma (Move) girdisini doğrudan gönderiyoruz
            starterInputs.MoveInput(input); 

            // Koşma (Sprint) mantığı
            if (magnitude > deadZone) // Ölü bölgeyi aşmışsa
            {
                // Joystick sapması belirlenen eşiği aşarsa koşmayı etkinleştir
                bool shouldSprint = magnitude >= runThreshold;
                starterInputs.SprintInput(shouldSprint);
            }
            else
            {
                // Joystick bırakılmışsa (deadZone içinde) koşmayı kapat
                starterInputs.SprintInput(false);
            }
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        // Joystick bırakıldığında hareket ve koşmayı durdur
        if (starterInputs != null)
        {
            starterInputs.MoveInput(Vector2.zero);
            starterInputs.SprintInput(false);
        }
    }
}