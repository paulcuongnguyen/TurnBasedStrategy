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
            gridPositionMarkerInstance.SetActive(false); // Hide it initially
        }
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

        SwitchSelectedGridPosition();

        HandleButtonSelectedAction();              

        SwitchUnit();

        SwitchAction();
    }

    //this will be used for choosing and taking action by pressing mouse button
    private void HandleSelectedAction()
    {
        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition()); // Get the grid position of the mouse

        // Check if the mouse is over a valid grid position
        if (selectedAction.IsValidActionGridPosition(mouseGridPosition))
        {
            isMouseOverValidGridPosition = true; // Mouse is over a valid grid position
            selectedGridPosition = mouseGridPosition;
            UpdateGridPositionMarker(); // Update the marker to the hovered grid position
        }
        else
        {
            isMouseOverValidGridPosition = false; // Mouse is not over a valid grid position
            if (gridPositionMarkerInstance != null)
            {
                gridPositionMarkerInstance.SetActive(false); // Hide the marker if the position is invalid
            }
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

            // selectedGridPosition = selectedAction.GetValidActionGridPositionList()[0]; 
            
            SetBusy(); //set busy on the action bar at the bottom of the screen
            selectedAction.TakeAction(selectedGridPosition, ClearBusy); //take action on the selected grid position, then clear busy for the action bar 

            OnActionStarted?.Invoke(this, EventArgs.Empty); //set action started event to update UI, action points, etc.
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

        // Return early if the list is empty
        if (validGridPositions == null || validGridPositions.Count == 0)
        {
            Debug.LogWarning("No valid grid positions available.");
            if (gridPositionMarkerInstance != null)
            {
                gridPositionMarkerInstance.SetActive(false); // Hide the marker if no valid positions
            }
            return;
        }

        // Clamp the index to ensure it's within bounds
        selectedGridPositionIndex = Mathf.Clamp(selectedGridPositionIndex, 0, validGridPositions.Count - 1);

        // Get the current grid position
        GridPosition currentGridPosition = validGridPositions[selectedGridPositionIndex];

        // Handle arrow key input
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Find the next grid position with a higher z value
            GridPosition? nextPosition = validGridPositions
                .Where(pos => pos.z > currentGridPosition.z && pos.x == currentGridPosition.x)
                .OrderBy(pos => pos.z)
                .FirstOrDefault();

            if (nextPosition != null)
            {
                selectedGridPositionIndex = validGridPositions.IndexOf(nextPosition.Value);
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Find the next grid position with a lower z value
            GridPosition? nextPosition = validGridPositions
                .Where(pos => pos.z < currentGridPosition.z && pos.x == currentGridPosition.x)
                .OrderByDescending(pos => pos.z)
                .FirstOrDefault();

            if (nextPosition != null)
            {
                selectedGridPositionIndex = validGridPositions.IndexOf(nextPosition.Value);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Find the next grid position with a higher x value
            GridPosition? nextPosition = validGridPositions
                .Where(pos => pos.x > currentGridPosition.x && pos.z == currentGridPosition.z)
                .OrderBy(pos => pos.x)
                .FirstOrDefault();

            if (nextPosition != null)
            {
                selectedGridPositionIndex = validGridPositions.IndexOf(nextPosition.Value);
            }
            else
            {
                Debug.Log("No valid grid position to the right.");
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Find the next grid position with a lower x value
            GridPosition? nextPosition = validGridPositions
                .Where(pos => pos.x < currentGridPosition.x && pos.z == currentGridPosition.z)
                .OrderByDescending(pos => pos.x)
                .FirstOrDefault();

            if (nextPosition != null)
            {
                selectedGridPositionIndex = validGridPositions.IndexOf(nextPosition.Value);
            }
            else
            {
                Debug.Log("No valid grid position to the left.");
            }
        }

        // Update the selected grid position
        selectedGridPosition = validGridPositions[selectedGridPositionIndex];
        // Debug.Log($"Selected Grid Position: {selectedGridPosition}");

        // Update the marker position
        UpdateGridPositionMarker();
    }

    private void UpdateGridPositionMarker()
    {
        if (gridPositionMarkerInstance == null) return;

        // Convert the selected grid position to world position
        Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(selectedGridPosition);

        // Move the marker to the selected grid position
        gridPositionMarkerInstance.transform.position = worldPosition;

        // Ensure the marker is active
        gridPositionMarkerInstance.SetActive(true);
    }

}
