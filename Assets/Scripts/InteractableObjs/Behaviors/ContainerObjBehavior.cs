using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerObjBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public bool accessible;

    [HideInInspector]
    public DetailCameraBehavior detailCameraBehavior;
    [HideInInspector]
    public List<InteractableObjBehavior> objBehaviors;
    [HideInInspector]
    public List<Light> detailLighting;

    private ActionVerbsUIController actionVerbsUIController;
    public ActionVerbsUIController ActionVerbsUIController
    {
        get
        {
            if (actionVerbsUIController == null) actionVerbsUIController = GeneralUIController.instance.actionVerbsUIController;
            return actionVerbsUIController;
        }
    }

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);
        ActivateObjBehaviorColliders(false);
        ActivateLighting(false);
    }

    void ActivateObjBehaviorColliders(bool value)
    {
        foreach(InteractableObjBehavior behavior in objBehaviors)
        {
            behavior.TriggerCollider.enabled = value;
        }
    }

    void ActivateLighting(bool value)
    {
        foreach(Light light in detailLighting)
        {
            light.enabled = value;
        }
    }

    public virtual IEnumerator LookInto()
    {
        TriggerCollider.enabled = false;
        ActivateObjBehaviorColliders(true);
        ActivateLighting(true);

        CameraManager.FromMainToProjectCamera(detailCameraBehavior);

        ActionVerbsUIController.ShowUnshowEscapeIcon(true);

        PCController.AddGetBackAction(GetBack);
        PCController.EnableMovementInput(false);

        yield return null;
    }

    public virtual void GetBack()
    {
        PCController.RemoveGetBackAction();
        TriggerCollider.enabled = true;
        ActivateObjBehaviorColliders(false);
        ActivateLighting(false);

        ActionVerbsUIController.ShowUnshowEscapeIcon(false);

        CameraManager.FromProjectionToMainCamera();
    }

    #region Data methods

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is ContainerObjData containerObjData)
        {
            accessible = containerObjData.accessible;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new ContainerObjData(inScene, accessible);
    }

    #endregion
}
