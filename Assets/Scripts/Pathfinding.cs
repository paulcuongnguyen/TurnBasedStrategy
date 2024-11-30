using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private int width;
    private int height;
    private float cellSize;
    private GridSystem<PathNode> gridSystem;
    [SerializeField] private Transform gridDebugObjectPrefab;

    private void Awake()
    {
        gridSystem = new GridSystem<PathNode>(10, 10, 2f, 
            (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));
        gridSystem.CreateDebugObject(gridDebugObjectPrefab);
    }
}
