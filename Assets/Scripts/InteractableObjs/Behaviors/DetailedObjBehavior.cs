using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailedObjBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public DetailCameraBehavior detailCameraBehavior;

    [HideInInspector]
    public GameObject detailedObjGO;

    [HideInInspector]
    public GameObject detailedLight;

    DetailedUIBase detailedUI;

    protected override void InitializeObjBehavior()
    {
        base.InitializeObjBehavior();

        detailedObjGO.SetActive(false);
        detailedLight.SetActive(false);
    }

    public IEnumerator LookInto()
    {
        TriggerCollider.enabled = false;

        detailedObjGO.SetActive(true);
        detailedLight.SetActive(true);

        currentSet.GetComponent<SetBehavior>().TurnOnOffLights(3f/5f);

        CameraManager.instance.ChangeToProjectorCamera(detailCameraBehavior, false);

        detailedUI = GeneralUIController.Instance.DisplayDetailedUI(this);
        PCController.instance.getBackCallback = GetBack;
        PCController.instance.EnableMovementInput(false);

        yield return null;
    }
    public void GetBack()
    {
        TriggerCollider.enabled = true;

        detailedObjGO.SetActive(false);
        detailedLight.SetActive(false);

        currentSet.GetComponent<SetBehavior>().TurnOnOffLights(5f/3f);

        GeneralUIController.Instance.DisplayGameplayUI();
        CameraManager.instance.ChangeToMainCamera();
        PCController.instance.EnableMovementInput(true);
    }

    protected void BlockInput(bool value)
    {
        detailedUI.BlockInput(value);
    }
}
