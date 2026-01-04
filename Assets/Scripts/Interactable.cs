using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
public class Interactable : MonoBehaviour
{
    // Etkileşim gerçekleştiğinde (buton basıldığında) çağrılacak metot.
    // Bu metodun içeriği her obje için farklı olabilir (Kapı aç, Sandık yağmala vb.).
    public virtual void Interact(ThirdPersonController characterController)
    {
        // Temel etkileşim mesajı. Bu metot, alt sınıflarda (Kapı, Sandık) ezilecektir.
        Debug.Log("Objeyle etkileşime girildi: " + gameObject.name);
    }
}