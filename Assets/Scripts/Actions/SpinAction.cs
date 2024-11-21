using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpinAction : BaseAction
{
    // public delegate void SpinCompleteDelegate();
    private float totalSpinAmount;

    private void Update()
    {
        if (!isActive) return;
        
        float spinAddAmount = 360 * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, spinAddAmount,0);

        totalSpinAmount += spinAddAmount;
        if (totalSpinAmount >= 360f)
        {
            ActionComplete();
        }
        
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {        
        totalSpinAmount = 0f;
        ActionStart(onActionComplete);
    }

    public override string GetActionName()
    {
        return "Spin";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();

        return new List<GridPosition>{unitGridPosition} ;
    }

    public override int GetActionPointsCost()
    {
        return 1;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0,
        };
    }
}
