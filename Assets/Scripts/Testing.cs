using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Testing : MonoBehaviour
{
    [SerializeField] private Unit unit;
    public static event EventHandler TestingActionOn;


    void Start()
    {
        
    }  

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestingActionOn?.Invoke(this, EventArgs.Empty);
        }
        
    }

    
}
