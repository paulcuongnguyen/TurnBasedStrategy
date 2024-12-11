using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private GridPosition gridPosition;
    private BaseAction[] baseActionArray;
    private HealthSystem healthSystem;

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;

    
    [SerializeField] private int actionPoints = ACTION_POINTS_MAX;
    [SerializeField] private const int ACTION_POINTS_MAX = 4;
    [SerializeField] private bool isEnemy;
    [SerializeField] private Transform primaryWeapon;
    [SerializeField] private Transform secondaryWeapon;
    [SerializeField] private CameraManager cameraManager;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        baseActionArray = GetComponents<BaseAction>();
        cameraManager = FindObjectOfType<CameraManager>();
    }


    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);
        
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        healthSystem.OnDead += HealthSystem_OnDead;

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    

    private void Update()
    {     
              
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != gridPosition)
        {
            //Unit changed Grid position
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;
            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
        }
    }

    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction baseAction in baseActionArray)
        {
            if (baseAction is T)
            {
                return (T)baseAction;
            }

        }
        return null;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }

    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (actionPoints >= baseAction.GetActionPointsCost())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if ((IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) ||
            (!IsEnemy() && TurnSystem.Instance.IsPlayerTurn()))
        {
            actionPoints = ACTION_POINTS_MAX;   
            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        cameraManager.KillCamSequence(this.transform);
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);

        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);

        Destroy(gameObject);
    }   

    public bool IsEnemy()
    {
        return isEnemy;
    }

    internal void Damage(int damageAmount)
    {
        healthSystem.Damage(damageAmount);
    }

    public HealthSystem GetHealthSystem()
    {
        return healthSystem;
    }

    public float GetHealthNormalized()
    {
        return healthSystem.GetHealthNormalized();
    }
}
