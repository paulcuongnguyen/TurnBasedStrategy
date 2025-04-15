using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }
    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;
    
    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private GameObject gridPositionMarkerPrefab;

    private bool isMouseOverValidGridPosition = false; // Tracks if the mouse is over a valid grid position
    private GameObject gridPositionMarkerInstance;
    private GridPosition selectedGridPosition;
    private BaseAction selectedAction;
    private bool isBusy;
    private int selectedGridPositionIndex = 0; // Track the current grid position index

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

        // Instantiate the marker at the start
        if (gridPositionMarkerPrefab != null)
        {
            gridPositionMarkerInstance = Instantiate(gridPositionMarkerPrefab);
            gridPositionMarkerInstance.SetActive(false);
            
            // // Initial position setup if we have a valid selected position
            // if (selectedAction != null && selectedAction.GetValidActionGridPositionList().Count > 0)
            // {
            //     selectedGridPosition = selectedAction.GetValidActionGridPositionList()[0];
            //     UpdateGridPositionMarker();
            // }
        }
    }

    private void Update()
    {          
        if (isBusy) return;

        if (!TurnSystem.Instance.IsPlayerTurn()) return;

        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // check if that mouseclick is to select a unit, if it is true, then do not move unit, only move after unit already be selected
        if (TryHandleUnitSelection()) return;    

        UpdateSelectedGridPosition();

        AutoSelectUnit();      
                               
        HandleSelectedAction(); 
        
        SwitchSelectedGridPosition();
        
        HandleButtonSelectedAction();              

        SwitchUnit();

        SwitchAction();        

        UpdateGridPositionMarker();
    }

    //this will be used for choosing and taking action by pressing mouse button
    private void HandleSelectedAction()
    {
        if (selectedAction == null) return;

        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

        // Check if the mouse is over a valid grid position
        bool isValidPosition = selectedAction.IsValidActionGridPosition(mouseGridPosition);
        isMouseOverValidGridPosition = isValidPosition;

        if (isValidPosition)
        {
            selectedGridPosition = mouseGridPosition;
            UpdateGridPositionMarker();
        }
        else if (gridPositionMarkerInstance != null && isMouseOverValidGridPosition)
        {
            // Only hide the marker if we're transitioning from valid to invalid position
            gridPositionMarkerInstance.SetActive(false);
        }

        // Handle mouse click for taking action
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            if (!selectedAction.IsValidActionGridPosition(mouseGridPosition)) // Check if the grid position is valid for the selected action
            {
                return;
            }

            if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction)) // Use action points to take action
            {
                return;
            }

            SetBusy(); // Set busy on the action bar at the bottom of the screen
            selectedAction.TakeAction(mouseGridPosition, ClearBusy); // Take action on the selected grid position, then clear busy for the action bar

            OnActionStarted?.Invoke(this, EventArgs.Empty); // Set action started event to update UI, action points, etc.
        }
    }

    //this will be used for taking action by pressing button on controller or keyboard
    private void HandleButtonSelectedAction()
    {
        if (InputManager.Instance.TakeAction())
        {
            if (selectedAction.GetValidActionGridPositionList().Count == 0)
            {
                return;
            }
            if (selectedGridPosition == null)
            {
                return;
            }
            if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction)) // Use action points to take action
            {
                return;
            }

            SetBusy(); //set busy on the action bar at the bottom of the screen
            selectedAction.TakeAction(selectedGridPosition, ClearBusy); //take action on the selected grid position, then clear busy for the action bar 
            OnActionStarted?.Invoke(this, EventArgs.Empty); //set action started event to update UI, action points, etc.
        }
    }

    private void UpdateSelectedGridPosition()
    {
        List<GridPosition> validGridPositionList = selectedAction.GetValidActionGridPositionList();
        // Check if current selectedGridPosition is still valid
        if (!validGridPositionList.Contains(selectedGridPosition))
        {
            // Update to first valid position
            if (validGridPositionList.Count == 0) return;
            selectedGridPosition = validGridPositionList[0];
            selectedGridPositionIndex = 0;                       
        }        
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
            selectedGridPositionIndex = 0; // Reset the grid position index when switching actions
            selectedGridPosition = friendlyUnitList[nextIndex].GetAction<MoveAction>().GetValidActionGridPositionList()[0]; // Reset the grid position to the first valid position of the new action
            UpdateGridPositionMarker();
        }
        if (InputManager.Instance.SelectPreviousUnit())
        {
            List<Unit> friendlyUnitList = UnitManager.Instance.GetFriendlyUnitList();
            int currentIndex = friendlyUnitList.IndexOf(selectedUnit);
            int previousIndex = (currentIndex - 1 + friendlyUnitList.Count) % friendlyUnitList.Count;
            SetSelectedUnit(friendlyUnitList[previousIndex]);
            selectedGridPositionIndex = 0; // Reset the grid position index when switching actions
            selectedGridPosition = friendlyUnitList[previousIndex].GetAction<MoveAction>().GetValidActionGridPositionList()[0]; // Reset the grid position to the first valid position of the new action
            UpdateGridPositionMarker();
        }
    }

    private void SwitchAction()
    {      
        if (InputManager.Instance.SelectNextAction())
        {
            List<BaseAction> baseActionList = new List<BaseAction>(selectedUnit.GetBaseActionArray());
            int currentIndex = baseActionList.IndexOf(selectedAction);
            int nextIndex = (currentIndex + 1) % baseActionList.Count;
            BaseAction nextAction = baseActionList[nextIndex];        
            
            SetSelectedAction(nextAction);
            
            // Try to set valid grid position if available
            List<GridPosition> validPositions = nextAction.GetValidActionGridPositionList();
            if (validPositions.Count > 0)
            {
                selectedGridPositionIndex = 0;
                selectedGridPosition = validPositions[0];
                UpdateGridPositionMarker();
            }
        }        
        if (InputManager.Instance.SelectPreviousAction())
        {
            List<BaseAction> baseActionList = new List<BaseAction>(selectedUnit.GetBaseActionArray());
            int currentIndex = baseActionList.IndexOf(selectedAction);
            int previousIndex = (currentIndex - 1 + baseActionList.Count) % baseActionList.Count;
            BaseAction previousAction = baseActionList[previousIndex];

            SetSelectedAction(previousAction);
            
            // Try to set valid grid position if available
            List<GridPosition> validPositions = previousAction.GetValidActionGridPositionList();
            if (validPositions.Count > 0)
            {
                selectedGridPositionIndex = 0;
                selectedGridPosition = validPositions[0];
                UpdateGridPositionMarker();
            }
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

    private void SwitchSelectedGridPosition()
    {
        // Skip updating the marker if the mouse is over a valid grid position
        if (isMouseOverValidGridPosition) return;

        List<GridPosition> validGridPositions = selectedAction.GetValidActionGridPositionList();

        if (validGridPositions == null || validGridPositions.Count == 0)
        {
            if (gridPositionMarkerInstance != null)
            {
                gridPositionMarkerInstance.SetActive(false);
            }
            return;
        }

        // Ensure index is within bounds
        selectedGridPositionIndex = Mathf.Clamp(selectedGridPositionIndex, 0, validGridPositions.Count - 1);
        GridPosition currentGridPosition = validGridPositions[selectedGridPositionIndex];
        int previousIndex = selectedGridPositionIndex; // Store previous index

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            var nextPositions = validGridPositions
                .Where(pos => pos.z > currentGridPosition.z && pos.x == currentGridPosition.x)
                .OrderBy(pos => pos.z);

            if (nextPositions.Any())
            {
                selectedGridPositionIndex = validGridPositions.IndexOf(nextPositions.First());
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            var nextPositions = validGridPositions
                .Where(pos => pos.z < currentGridPosition.z && pos.x == currentGridPosition.x)
                .OrderByDescending(pos => pos.z);

            if (nextPositions.Any())
            {
                selectedGridPositionIndex = validGridPositions.IndexOf(nextPositions.First());
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            var nextPositions = validGridPositions
                .Where(pos => pos.x > currentGridPosition.x && pos.z == currentGridPosition.z)
                .OrderBy(pos => pos.x);

            if (nextPositions.Any())
            {
                selectedGridPositionIndex = validGridPositions.IndexOf(nextPositions.First());
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            var nextPositions = validGridPositions
                .Where(pos => pos.x < currentGridPosition.x && pos.z == currentGridPosition.z)
                .OrderByDescending(pos => pos.x);

            if (nextPositions.Any())
            {
                selectedGridPositionIndex = validGridPositions.IndexOf(nextPositions.First());
            }
        }

        // Only update if the index actually changed
        if (selectedGridPositionIndex != previousIndex)
        {
            selectedGridPosition = validGridPositions[selectedGridPositionIndex];
            UpdateGridPositionMarker();
        }
    }

    private void UpdateGridPositionMarker()
    {
        if (gridPositionMarkerInstance == null) return;

        if (selectedAction == null) return;

        if (selectedAction.GetValidActionGridPositionList().Count <= 0)
        {
            gridPositionMarkerInstance.SetActive(false);
            return;
        }

        // Convert the selected grid position to world position
        Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(selectedGridPosition);

        // Move the marker to the selected grid position
        gridPositionMarkerInstance.transform.position = worldPosition;

        // Ensure the marker is active
        gridPositionMarkerInstance.SetActive(true);
    }

}
