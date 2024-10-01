using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedGameObject;
    private BaseAction baseAction;

    public void SetBaseAction(BaseAction baseAction)
    {
        this.baseAction = baseAction;
        textMeshPro.text =  baseAction.GetActionName().ToUpper();

        //this below is using an anonymous function
        button.onClick.AddListener(() => {
            UnitActionSystem.Instance.SetSelectedAction(baseAction);
        });
    }

    public void UpdateSelectedVisual()
    {
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        selectedGameObject.SetActive(selectedAction == baseAction);
    }
}
