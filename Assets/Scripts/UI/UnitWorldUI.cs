using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class UnitWorldUI : MonoBehaviour
{
    private Transform cameraTransform;
    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private Unit unit;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private HealthSystem healthSystem;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged;
        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;

        UpdateActionPointsText();
        UpdateHealthBarImage();
    }
   
    private void LateUpdate()
    {
        transform.rotation = cameraTransform.rotation;
    }

    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        UpdateActionPointsText();
    }

     private void HealthSystem_OnHealthChanged(object sender, EventArgs e)
    {
        UpdateHealthBarImage();;
    }


    private void UpdateActionPointsText()
    {
        actionPointsText.text = unit.GetActionPoints().ToString();
    }

    private void UpdateHealthBarImage()
    {
        healthBarImage.fillAmount = healthSystem.GetHealthNormalized();
    }

}
