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

    [HideInInspector]
    public float lightReductionMultiplier = 3f / 5f;

    DetailedUIBase detailedUI;

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
        }
    }

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        detailedObjGO.SetActive(false);
        detailedLight.SetActive(false);
    }

    public virtual IEnumerator LookInto()
    {
        TriggerCollider.enabled = false;

        detailedObjGO.SetActive(true);
        detailedLight.SetActive(true);

        currentSet.GetComponent<SetBehavior>().TurnOnOffLights(lightReductionMultiplier);

        CameraManager.instance.FromMainToProjectCamera(detailCameraBehavior, false);

        detailedUI = GeneralUIController.ShowDetailedUI(this);

        if (GeneralUIController.displayingGameplayUI)
        {
            GeneralUIController.UnshowGameplayUI();
        }

        PCController.instance.getBackActionStack.Push(GetBack);
        PCController.instance.EnableMovementInput(false);

        yield return null;
    }
    public void GetBack()
    {
        TriggerCollider.enabled = true;

        detailedObjGO.SetActive(false);
        detailedLight.SetActive(false);

        currentSet.GetComponent<SetBehavior>().TurnOnOffLights(1/lightReductionMultiplier);

        GeneralUIController.UnshowDetailedUI();
        CameraManager.instance.FromProjectionToMainCamera();
        PCController.instance.EnableMovementInput(true);
    }

    protected void BlockInput(bool value)
    {
        detailedUI.BlockInput(value);
    }

    public override InteractableObjData GetObjData()
    {
        return new DetailedObjData(inScene);
    }
}
