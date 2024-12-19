using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    private int currentPositionIndex;
    private List<Vector3> positionList;   
    [SerializeField] private int maxMoveDistance = 4;

    private void Update()
    {
        if (!isActive) return;

        Vector3 targetPosition = positionList[currentPositionIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

         //unit will stop at this distance from destination to avoid jittering 
        float stoppingDistance = .1f;

        if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {          
            float moveSpeed = 4f;   
          
            // Moonwalk FIXED, unit NO LONGER move backward for some seconds before turning around 
            float rotateSpeed = 25f;
                        
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            transform.position += moveDirection * moveSpeed *Time.deltaTime;
            
            // code of CodeMonkey, created moonwalk issue
            // transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime*rotateSpeed);                  
            
        }
        else
        {
            currentPositionIndex++; 
            if (currentPositionIndex >= positionList.Count)
            {
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                ActionComplete();
            }            
        }       

    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out int pathLength);
        currentPositionIndex = 0;
        positionList = new List<Vector3>();

        foreach (GridPosition pathGridPosition in pathGridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        OnStartMoving?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }
    
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionsList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                GridPosition offSetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offSetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                if (unitGridPosition == testGridPosition)
                {
                    //same grid position where unit already at
                    continue;
                }
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    //grid position already occupied
                    continue;
                }
                if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                {
                    //grid position is not walkable
                    continue;
                }
                if (!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition))
                {
                    //grid position is not reachable
                    continue;
                }

                int pathfindingDistanceMultiplier = 10;
                if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathfindingDistanceMultiplier)
                {
                    //path length is too far
                    continue;
                }

                validGridPositionsList.Add(testGridPosition);
            }
        }

        return validGridPositionsList;
    }

    public override string GetActionName()
    {
        return "Move";
    }

    // // Original EnemyAIAction by CodeMonkey with null modified to use with below override GetBestEnemyAIAction
    // public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    // {
    //     int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);
    //     if (targetCountAtGridPosition == 0)
    //     {
    //         return null;
    //     }
    //     else 
    //     {
    //         return new EnemyAIAction
    //     {
    //         gridPosition = gridPosition,
    //         actionValue = targetCountAtGridPosition * 10,
    //     };
    //     }   
                
    // }    

    // // better EnemyAI implement, if no targetable playr in range, enemy will seek nearest player to target 
    
    // public override EnemyAIAction GetBestEnemyAIAction()
    // {
    //     List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();
    //     List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();

    //     foreach (GridPosition gridPosition in validActionGridPositionList)
    //     {
    //         EnemyAIAction action = GetEnemyAIAction(gridPosition);
    //         if (action != null)
    //             enemyAIActionList.Add(action);
    //     }

    //     // if no targetable enemies within path than lets pathfind to closest enemy
    //     if (enemyAIActionList.Count == 0)
    //     {
    //         // find shortest path
    //         var enemies = UnitManager.Instance.GetFriendlyUnitList();
    //         var unitPosition = unit.GetGridPosition();
    //         List<GridPosition> shortestPath = new List<GridPosition>();
    //         foreach (var enemy in enemies)
    //         {
    //             var path = Pathfinding.Instance.FindPath(unitPosition, enemy.GetGridPosition(), out int length);
    //             if (shortestPath.Count == 0 || length <= shortestPath.Count)
    //             {
    //                 shortestPath = path;
    //             }
    //         }
    //         //reverse list from furthest to closes so we can find furthest item first and return it
    //         shortestPath.Reverse();
    //         foreach(var pathItem in shortestPath)
    //         {
    //             if (validActionGridPositionList.Contains(pathItem))
    //             {
    //                 var gridPosition = new GridPosition(pathItem.x, pathItem.z);
    //                 return new EnemyAIAction
    //                 {
    //                     gridPosition = gridPosition,
    //                     actionValue = 50
    //                 };
    //             }
    //         }
    //     }

    //     if (enemyAIActionList.Count > 0)
    //     {
    //         enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
    //         return enemyAIActionList[0];
    //     }
    //     else
    //     {
    //         return null;
    //     }
    // }

    // new EnemyAIAction will patrol randomly if no target sighted
    // will chase target once sighted, back to patrol if loosing sight
    // TODO: Enemy to run to last known player location before back to patrol
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);

        if (targetCountAtGridPosition == 0)
        {
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = UnityEngine.Random.Range(1, 51)
            };
        }
        else
        {
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = 50 + targetCountAtGridPosition * 10
            };
        }    
    }
}
