using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isOpen;
    private Animator animator;
    private GridPosition gridPosition;
    private Action onInteractionComplete;
    private float timer;
    private bool isActive;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetInteractableAtGridPosition(gridPosition, this);

        if (isOpen)
        {
            Pathfinding.Instance.SetIsWalkableGridPosition(gridPosition, true);
        }
        else
        {
            Pathfinding.Instance.SetIsWalkableGridPosition(gridPosition, false);
        }
    }

    private void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isActive = false;
            onInteractionComplete();
        }
    }

    public void Interact(Action onInteractionComplete)
    {
        this.onInteractionComplete = onInteractionComplete;
        isActive = true;
        timer = 1f;

        if (isOpen)
        {
            animator.SetBool("IsOpen", false);
            CloseDoor();
        }
        else
        {
            animator.SetBool("IsOpen", true);
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        // animator.SetBool("IsOpen", isOpen);
        Pathfinding.Instance.SetIsWalkableGridPosition(gridPosition, true);
    }

    private void CloseDoor()
    {
        isOpen = false;
        // animator.SetBool("IsOpen", isOpen);
        Pathfinding.Instance.SetIsWalkableGridPosition(gridPosition, false);
    }   
}
