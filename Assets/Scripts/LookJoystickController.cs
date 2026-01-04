using UnityEngine;
using StarterAssets;
using UnityEngine.EventSystems; 

public class LookJoystickController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Gerekli Referanslar")]
    public FixedJoystick lookJoystick; 
    public StarterAssetsInputs starterInputs;

    [Header("Bakış Ayarları")]
    public float lookSensitivity = 1f;
    public float sensitivityMultiplier = 100f; 

    private bool _isPressed = false;

    void Start()
    {
        // (Start metodu içeriği aynı kalır)
        if (lookJoystick == null) 
            Debug.LogError("LookJoystickController: Look Joystick bileşeni atanmamış!");
        
        if (starterInputs == null)
        {
             starterInputs = FindObjectOfType<StarterAssetsInputs>();
             if (starterInputs == null)
                 Debug.LogError("LookJoystickController: StarterAssetsInputs bulunamadı!");
        }
    }

    void Update()
    {
        if (lookJoystick == null || starterInputs == null || !_isPressed) return;
       
        Vector2 lookDirection = lookJoystick.Direction; 
        Vector2 adjustedInput = lookDirection * lookSensitivity * Time.deltaTime * sensitivityMultiplier;
        
        starterInputs.LookInput(adjustedInput);
    }

    // ⭐ GÜNCELLEME: isLooking bayrağı ekleniyor
    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        // ThirdPersonController'a bakışın başladığını bildir
        if (starterInputs != null)
        {
             starterInputs.SetIsLooking(true); 
        }
    }

    // ⭐ GÜNCELLEME: isLooking bayrağı ekleniyor
    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        
        // Bakışı durdur
        if (starterInputs != null)
        {
            starterInputs.LookInput(Vector2.zero); 
            // ThirdPersonController'a bakışın bittiğini bildir
            starterInputs.SetIsLooking(false); 
        }
    }
}