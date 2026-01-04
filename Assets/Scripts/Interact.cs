using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
public class Door : Interactable
{
    private bool _isOpen = false;

    // Interactable sınıfındaki metodu eziyoruz (override).
    public override void Interact(ThirdPersonController characterController)
    {
        // Kapı durumunu değiştir
        _isOpen = !_isOpen;

        if (_isOpen)
        {
            Debug.Log(gameObject.name + " açıldı.");
            // Görsel/animasyonel kapı açma kodları buraya gelir.
            // Örneğin: transform.Rotate(Vector3.up, 90f);
        }
        else
        {
            Debug.Log(gameObject.name + " kapatıldı.");
            // Kapı kapatma kodları buraya gelir.
        }
        
        // Base sınıfın mesajını da isterseniz çağırabilirsiniz:
        // base.Interact(characterController);
    }
}
