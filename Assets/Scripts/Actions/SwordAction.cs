using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAction : BaseAction
{
    public event EventHandler OnSwordActionStarted;
    public event EventHandler OnSwordActionCompleted;
    private int maxSwordDistance = 1;
    private State state;
    private float stateTimer;
    private Unit targetUnit;
    private enum State
    {
        SwingingBeforeHit,
        SwingingAfterHit,
    }

    private void Update()
    {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;
        switch(state)
        {
            case State.SwingingBeforeHit:  
                UnityEngine.Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                float rotateSpeed = 10f;                
                UnityEngine.Quaternion targetRotation = UnityEngine.Quaternion.LookRotation(aimDirection);
                transform.rotation = UnityEngine.Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);              
                break;
            case State.SwingingAfterHit:
                break;
        }       

        if (stateTimer <= 0f)
        {
            NextStage();
        } 
    }

    private void NextStage()
    {
        switch (state)
        {
            case State.SwingingBeforeHit:
                state = State.SwingingAfterHit;
                float afterHitStateTime = 0.5f;
                stateTimer = afterHitStateTime;
                targetUnit.Damage(100);
                break;
            case State.SwingingAfterHit:   
                OnSwordActionCompleted?.Invoke(this, EventArgs.Empty);         
                ActionComplete();
                break;  
        }
    }

    public override string GetActionName()
    {
        return "Sword";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction{
                    actionValue = 200,
                    gridPosition = gridPosition,
                    };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionsList = new List<GridPosition>();  
        GridPosition unitGridPosition = unit.GetGridPosition();      

        for (int x = -maxSwordDistance; x <= maxSwordDistance; x++)
        {
            for (int z = -maxSwordDistance; z <= maxSwordDistance; z++)
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
                
                // this below is excluded to allow unit to shoot anywhere, not just the grid occupied by enemy
                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    //gridposition is empty
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                if (targetUnit == null) continue;                               
                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    //both units on the same team
                    continue;
                }        
                                           
                validGridPositionsList.Add(testGridPosition);
            }
        }

        return validGridPositionsList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        state = State.SwingingBeforeHit;
                float beforeHitStateTime = 0.7f;
                stateTimer = beforeHitStateTime;

        OnSwordActionStarted?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }

    public int GetMaxSwordDistance()    
    {
        return maxSwordDistance;
    }
}
