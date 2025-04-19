#define USE_NEW_INPUT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerInputActions playerInputActions;

    private void Awake()
    {        
        if (Instance != null)
        {
            Debug.LogError("There are more than one InputManager " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }        
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    public Vector2 GetMouseScreenPosition()
    {
        #if USE_NEW_INPUT_SYSTEM
        return Mouse.current.position.ReadValue();
        #else
        return Input.mousePosition;
        #endif
    }

    public Vector2 GetCameraMoveVector()
    {      

        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.CameraMovement.ReadValue<Vector2>();
        #else
        Vector2 inputMoveDirection = new Vector2(0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            inputMoveDirection.y = +1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputMoveDirection.y = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputMoveDirection.x = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputMoveDirection.x = +1f;
        }

        return inputMoveDirection;
        #endif
    }

    public float GetCameraRotateAmount()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.CameraRotation.ReadValue<float>();
        #else
        float rotateAmount = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotateAmount = +1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotateAmount = -1f;
        }

        return rotateAmount;
        #endif
    }

    public float GetCameraZoomAmount()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.CameraHeight.ReadValue<float>();
        #else
        float zoomAmount = 0f;

        if (Input.mouseScrollDelta.y > 0)
        {
            zoomAmount = +1f;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            zoomAmount = -1f;
        }

        return zoomAmount;
        #endif
    }

    public bool IsMouseButtonDownThisFrame()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.MouseClick.WasPressedThisFrame();
        #else
        return Input.GetMouseButtonDown(0);
        #endif
    }

    public bool IsMiddleMouseButtonDown()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.CameraReset.triggered;
        #else
        return Input.GetMouseButtonDown(2);
        #endif
    }    

    public bool SelectNextUnit()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.SelectNextUnit.triggered;
        #else
        return Input.GetKeyDown(KeyCode.Tab);
        #endif
    }

    public bool SelectPreviousUnit()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.SelectPreviousUnit.triggered;
        #else
        return Input.GetKeyDown(KeyCode.LeftShift);
        #endif
    }

    public bool SelectNextAction()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.SelectNextAction.triggered;
        #else
        return Input.GetKeyDown(KeyCode.ArrowRight);
        #endif
    }

    public bool SelectPreviousAction()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.SelectPreviousAction.triggered;
        #else
        return Input.GetKeyDown(KeyCode.ArrowLeft);
        #endif
    }

    public bool EndTurn()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.EndTurn.triggered;
        #else
        return Input.GetKeyDown(KeyCode.Enter);
        #endif
    }

    public bool TakeAction()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.TakeAction.triggered;
        #else
        return Input.GetKeyDown(KeyCode.Space);
        #endif
    }

    public bool GridSelectionUp()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.GridSelectionUp.triggered;
        #else
        return Input.GetKeyDown(KeyCode.UpArrow);
        #endif
    }

    public bool GridSelectionDown()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.GridSelectionDown.triggered;
        #else
        return Input.GetKeyDown(KeyCode.UpArrow);
        #endif
    }

    public bool GridSelectionLeft()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.GridSelectionLeft.triggered;
        #else
        return Input.GetKeyDown(KeyCode.UpArrow);
        #endif
    }

    public bool GridSelectionRight()
    {
        #if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.GridSelectionRight.triggered;
        #else
        return Input.GetKeyDown(KeyCode.UpArrow);
        #endif
    }
}
