using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    private Vector3 targetPosition;   
    [SerializeField] private int maxMoveDistance = 4;


    protected override void Awake()
    {
        base.Awake();
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (!isActive) return;

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
            OnStopMoving?.Invoke(this, EventArgs.Empty);
            ActionComplete();
        }       

    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        this.targetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
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
                validGridPositionsList.Add(testGridPosition);
            }
        }

        return validGridPositionsList;
    }

    public override string GetActionName()
    {
        return "Move";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPostion = unit.GetShootAction().GetTargetCountAtPosition(gridPosition);
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = targetCountAtGridPostion * 10,
        };
    }
}
