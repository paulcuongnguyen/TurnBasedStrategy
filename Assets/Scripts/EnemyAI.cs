using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private State state;
    private float timer;

    private enum State
    {
        WaitingForEnemyTurn,
        TakingTurn,
        Busy,
    }

    private void Awake() 
    {
        state = State.WaitingForEnemyTurn;
    }

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void Update()
    {
        if(TurnSystem.Instance.IsPlayerTurn()) return;

        switch (state)
        {
            case State.WaitingForEnemyTurn:
                break;
            case State.TakingTurn:
                timer -= Time.deltaTime;
                if(timer < 0)
                {                    
                    if (TryTakeEnemyAIAction(SetStageTakingTurn))
                    {
                        state = State.Busy;
                    }      
                    else
                    {
                        //no more enemies have actions to take, end enemy turn
                        TurnSystem.Instance.NextTurn();
                    }           
                }
                break;
            case State.Busy:
                break;            
        }        
    }

    private void SetStageTakingTurn()
    {
        timer = 0.5f;
        state = State.TakingTurn;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)   
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            state = State.TakingTurn;
            timer = 2f;
        }        
    }

    private bool TryTakeEnemyAIAction(Action onEnemyAIActionComplete)
    {
        Debug.Log("Take enemy AI action");
        foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
        {
            if (TryTakeEnemyAIAction(enemyUnit, onEnemyAIActionComplete))
            {
            return true;
            }
        }
        return false;
    }

    private bool TryTakeEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {
        SpinAction spinAction = enemyUnit.GetSpinAction();

        GridPosition actionGridPosition = enemyUnit.GetGridPosition();

            if (!spinAction.IsValidActionGridPosition(actionGridPosition))
            {
                return false;            
            }          
           
            if (!enemyUnit.TrySpendActionPointsToTakeAction(spinAction))
            {
                return false;
            }    
            
            Debug.Log("Spin Action");
            spinAction.TakeAction(actionGridPosition, onEnemyAIActionComplete);
            return true;
    }
}
