using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingUpdater : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        DestructibleCrate.OnAnyDestroyed += DestructibleCrate_OnAnyDestroyed;
    }

    private void DestructibleCrate_OnAnyDestroyed(object sender, EventArgs e)
    {
        DestructibleCrate destructibleCrate = sender as DestructibleCrate;
        Pathfinding.Instance.SetIsWalkableGridPosition(destructibleCrate.GetGridPosition(), true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
