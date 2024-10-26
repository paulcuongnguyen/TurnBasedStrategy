using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform cameraTransform;
    [SerializeField] private bool invert;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.rotation = cameraTransform.rotation;
        
        // weird code by CodeMonkey, make healthbar and actionpoints wonky and uneven
        // if (invert)
        // {
        //     // Vector3 dirCamera = (cameraTransform.position - transform.position).normalized;
        //     // transform.LookAt(transform.position + dirCamera * -1);            
        // }
        // else
        // {
        //     transform.LookAt(cameraTransform);
        // }
    }
}
