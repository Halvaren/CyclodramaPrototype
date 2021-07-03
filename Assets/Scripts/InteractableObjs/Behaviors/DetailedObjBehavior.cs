using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the behavior of an object that can be looked in detail
/// </summary>
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

    /// <summary>
    /// Initializes the behavior
    /// </summary>
    /// <param name="currentSet"></param>
    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        detailedObjGO.SetActive(false);
        detailedLight.SetActive(false);
    }

    /// <summary>
    /// Coroutine that changes the point of view to look into the container object
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator LookInto()
    {
        TriggerCollider.enabled = false;

        detailedObjGO.SetActive(true);
        detailedLight.SetActive(true);

        currentSet.GetComponent<SetBehavior>().TurnOnOffLights(lightReductionMultiplier);

        CameraManager.FromMainToProjectCamera(detailCameraBehavior, false);

        detailedUI = GeneralUIController.ShowDetailedUI(this);

        if (GeneralUIController.displayingGameplayUI)
        {
            yield return GeneralUIController.UnshowGameplayUICoroutine();
        }

        PCController.AddGetBackAction(GetBack);
        PCController.EnableMovementInput(false);

        yield return null;
    }

    /// <summary>
    /// Gets back to the regular point of view
    /// </summary>
    public void GetBack()
    {
        PCController.RemoveGetBackAction();
        TriggerCollider.enabled = true;

        detailedObjGO.SetActive(false);
        detailedLight.SetActive(false);

        currentSet.GetComponent<SetBehavior>().TurnOnOffLights(1/lightReductionMultiplier);

        GeneralUIController.UnshowDetailedUI();
        CameraManager.FromProjectionToMainCamera();
        PCController.EnableMovementInput(true);
    }

    /// <summary>
    /// Blocks the interaction with detailedUI
    /// </summary>
    /// <param name="value"></param>
    protected void BlockInput(bool value)
    {
        detailedUI.BlockInput(value);
    }

    /// <summary>
    /// Returns a data object with the info of the behavior
    /// </summary>
    /// <returns></returns>
    public override InteractableObjData GetObjData()
    {
        return new DetailedObjData(inScene);
    }
}
