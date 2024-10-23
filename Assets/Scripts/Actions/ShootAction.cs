using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class ShootAction : BaseAction
{
    public event EventHandler<OnShootEventArgs> OnShoot;

    public class OnShootEventArgs: EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }

    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }

    private State state;
    private float stateTimer;
    private Unit targetUnit;
    private bool canShootBullet;
    [SerializeField] float aimingStateTime = 1f;
    [SerializeField] float shootingStateTime = 0.1f;
    [SerializeField] float coolOffStateTime = 0.5f;
    [SerializeField] private int maxShootDistance = 4;

    private void Update()
    {
        if (!isActive) return;
        
        stateTimer -= Time.deltaTime;
        switch(state)
        {
            case State.Aiming:
                UnityEngine.Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                float rotateSpeed = 10f;
                
                UnityEngine.Quaternion targetRotation = UnityEngine.Quaternion.LookRotation(aimDirection);
                transform.rotation = UnityEngine.Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
                // code by CodeMonkey, rotate did not happen fast enough before shooting
                // transform.forward = UnityEngine.Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime*rotateSpeed);
                break;
            case State.Shooting:
                if (canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }       

        if (stateTimer <= 0f)
        {
            NextStage();
        } 
    }

    private void Shoot()
    {
        OnShoot?.Invoke(this, new OnShootEventArgs {
                                targetUnit = targetUnit,
                                shootingUnit = unit
                                });
        targetUnit.Damage();
    }

    private void NextStage()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Cooloff;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;  
        }
    }

    public override string GetActionName()
    {
        return "Shoot";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionsList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                GridPosition offSetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offSetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                if (testDistance > maxShootDistance)
                {
                    continue;
                }
                
                if (unitGridPosition == testGridPosition)
                {
                    //same grid position where unit already at
                    continue;
                }
                // this below is excluded to allow unit to shoot anywhere, not just the grid occupied by enemy
                // if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                // {
                //     //gridposition is empty
                //     continue;
                // }

                Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(testGridPosition);
                if (targetUnit == null) 
                {
                    continue;
                }
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
        ActionStart(onActionComplete);


        targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(gridPosition);

        state = State.Aiming;
        stateTimer = aimingStateTime;

        canShootBullet = true;
    }
}
