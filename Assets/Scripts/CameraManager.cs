using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject actionCameraGameObject;
    [SerializeField] private GameObject canvas;
    [SerializeField] private List<UnitWorldUI> unitWorldUIsList;  
    [SerializeField] private List<UnitSelectedVisual> unitSelectedVisualList;

    private void Start()
    {
        HideActionCamera();
        BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;       

        unitWorldUIsList = new List<UnitWorldUI>(); 
        UnitWorldUI[] foundUI = FindObjectsOfType<UnitWorldUI>();
        unitWorldUIsList.AddRange(foundUI);

        unitSelectedVisualList = new List<UnitSelectedVisual>();
        UnitSelectedVisual[] foundVisual = FindObjectsOfType<UnitSelectedVisual>();
        unitSelectedVisualList.AddRange(foundVisual);
    }
    
    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        switch(sender)
        {
            // case ShootAction shootAction:
            //     Unit shooterUnit = shootAction.GetUnit();
            //     Unit targetUnit = shootAction.GetTargetUnit();

            //     Vector3 cameraCharacterHeight = Vector3.up * 1.6f;

            //     Vector3 shootDirection = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

            //     float shoulderOffsetAmount = 0.5f;
            //     Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDirection * shoulderOffsetAmount;

            //     Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() 
            //                                 + cameraCharacterHeight 
            //                                 + shoulderOffset 
            //                                 + (shootDirection * -1);                
            //     actionCameraGameObject.transform.position = actionCameraPosition;
            //     actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);

            //     ShowActionCamera();
            //     break;
        }
    }
    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        switch(sender)
        {
            // case ShootAction shootAction:
            //     HideActionCamera();
            //     break;
        }
    }
         
    private void ShowActionCamera()
    {
        actionCameraGameObject.SetActive(true);
        canvas.SetActive(false);
        foreach (UnitWorldUI unitWorldUI in unitWorldUIsList)
        {
            if (unitWorldUI == null) continue;
            unitWorldUI.gameObject.SetActive(false);
        }
        foreach (UnitSelectedVisual unitSelectedVisual in unitSelectedVisualList)
        {
            if (unitSelectedVisual == null) continue;
            unitSelectedVisual.gameObject.SetActive(false);
        }      
    }   

    private void HideActionCamera()
    {        
        actionCameraGameObject.SetActive(false);
        canvas.SetActive(true);
        foreach (UnitWorldUI unitWorldUI in unitWorldUIsList)
        {
            if (unitWorldUI == null) continue;
            unitWorldUI.gameObject.SetActive(true);
        }
        foreach (UnitSelectedVisual unitSelectedVisual in unitSelectedVisualList)
        {
            if (unitSelectedVisual == null) continue;
            unitSelectedVisual.gameObject.SetActive(true);
        }              
    } 

    public void KillCamSequence(Transform unitTranform)
    {
        Vector3 killCamShoulderOffset =  Quaternion.Euler(0, 120, 0) * unitTranform.forward * 4f;
        Vector3 killCamHeight = Vector3.up * 1.6f;
        
        actionCameraGameObject.transform.position = unitTranform.position + killCamShoulderOffset + killCamHeight;
        actionCameraGameObject.transform.LookAt(unitTranform.position + killCamHeight);

        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            ShowActionCamera();   
            yield return new WaitForSeconds(2); 
            HideActionCamera();
        }
    }   
}
