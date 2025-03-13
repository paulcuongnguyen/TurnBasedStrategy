using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }
    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;
    
    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;
    
    private BaseAction selectedAction;
    private bool isBusy;

    private void Awake()
    {        
        if (Instance != null)
        {
            Debug.LogError("There are more than one UnitActionSystem " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }        
        Instance = this;
    }

    private void Start()
    {
        SetSelectedUnit(selectedUnit);
    }

    private void Update()
    {          
        if (isBusy) return;

        if (!TurnSystem.Instance.IsPlayerTurn()) return;

        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // check if that mouseclick is to select a unit, if it is true, then do not move unit, only move after unit already be selected
        if (TryHandleUnitSelection()) return;    

        AutoSelectUnit();      
                               
        HandleSelectedAction();        

        SwitchUnit();

        SwitchAction();
    }

    private void HandleSelectedAction()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

            if (!selectedAction.IsValidActionGridPosition(mouseGridPosition))
            {
                return;            
            }          
           
            if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
            {
                return;
            }    
            
            SetBusy();
            selectedAction.TakeAction(mouseGridPosition, ClearBusy);  

            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetBusy()
    {
        isBusy = true;
        OnBusyChanged?.Invoke(this, isBusy);
    }

    private void ClearBusy()
    {
        isBusy = false;
        OnBusyChanged?.Invoke(this, isBusy);
    }

    private bool TryHandleUnitSelection()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
            if(Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                if(raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    if (unit == selectedUnit)
                    {
                        //Unit already selected
                        return false;
                    }
                    if (unit.IsEnemy())
                    {
                        //Unit is enemy
                        return false;
                    }
                    SetSelectedUnit(unit);
                    return true;
                }            
            }        
        }
        return false;
    }
  
    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        SetSelectedAction(unit.GetAction<MoveAction>());

        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);       
    }

    // if no unit selected or when selected unit died, then select the first unit in the list
    private void AutoSelectUnit()
    {
        if (selectedUnit == null)
        {
            SetSelectedUnit(UnitManager.Instance.GetFriendlyUnitList()[0]);
        }
    }

    private void SwitchUnit()
    {
        if (InputManager.Instance.SelectNextUnit())
        {
            List<Unit> friendlyUnitList = UnitManager.Instance.GetFriendlyUnitList();
            int currentIndex = friendlyUnitList.IndexOf(selectedUnit);
            int nextIndex = (currentIndex + 1) % friendlyUnitList.Count;
            SetSelectedUnit(friendlyUnitList[nextIndex]);
        }
        if (InputManager.Instance.SelectPreviousUnit())
        {
            List<Unit> friendlyUnitList = UnitManager.Instance.GetFriendlyUnitList();
            int currentIndex = friendlyUnitList.IndexOf(selectedUnit);
            int previousIndex = (currentIndex - 1 + friendlyUnitList.Count) % friendlyUnitList.Count;
            SetSelectedUnit(friendlyUnitList[previousIndex]);
        }
    }

    private void SwitchAction()
    {
        if (InputManager.Instance.SelectNextAction())
        {
            List<BaseAction> baseActionList = new List<BaseAction>(selectedUnit.GetBaseActionArray());
            int currentIndex = baseActionList.IndexOf(selectedAction);
            int nextIndex = (currentIndex + 1) % baseActionList.Count;
            SetSelectedAction(baseActionList[nextIndex]);
        }        
        if (InputManager.Instance.SelectPreviousAction())
        {
            List<BaseAction> baseActionList = new List<BaseAction>(selectedUnit.GetBaseActionArray());
            int currentIndex = baseActionList.IndexOf(selectedAction);
            int previousIndex = (currentIndex - 1 + baseActionList.Count) % baseActionList.Count;
            SetSelectedAction(baseActionList[previousIndex]);
        }
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }
    
    public void SetSelectedAction(BaseAction baseAction)
    {
        selectedAction = baseAction;

        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
    }

    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }    
}
