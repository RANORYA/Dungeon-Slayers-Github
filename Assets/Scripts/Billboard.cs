using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _camTransform;

    void Start()
    {
        if (Camera.main != null)
            _camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (_camTransform != null)
        {
            transform.LookAt(transform.position + _camTransform.forward);
        }
    }
}
