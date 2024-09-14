using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridDebugObject : MonoBehaviour
{
    private GridObject gridObject;
    
    [SerializeField] TextMeshPro textMeshPro;



    public void SetGridObject(GridObject gridObject)
    {
        this.gridObject = gridObject;
    }

    public void Update()
    {
        if (gridObject == null) return;
        textMeshPro.text = gridObject.ToString();
    }
}
