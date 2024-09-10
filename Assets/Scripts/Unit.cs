using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private Animator unitAnimator;
    [SerializeField] private Vector3 targetPosition;
    
    private void Awake()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {       
        //unit will stop at this distance from destination to avoid jittering 
        float stoppingDistance = .1f;

        if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            Vector3 moveDirection = (targetPosition - transform.position).normalized;

            float moveSpeed = 4f;
            float rotateSpeed = 15f;
            transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime*rotateSpeed);

            
            transform.position += moveDirection * moveSpeed *Time.deltaTime;

            
            unitAnimator.SetBool("isWalking", true);
        }
        else
        {
            unitAnimator.SetBool("isWalking", false);
        }
        
    }

    public void Move(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
    
}
