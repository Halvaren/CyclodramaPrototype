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

        CameraManager.instance.FromMainToProjectCamera(detailCameraBehavior);

        PCController.instance.getBackActionStack.Push(GetBack);
        PCController.instance.EnableMovementInput(false);

        yield return null;
    }

    public virtual void GetBack()
    {
        TriggerCollider.enabled = true;
        ActivateObjBehaviorColliders(false);
        ActivateLighting(false);

        CameraManager.instance.FromProjectionToMainCamera();
        PCController.instance.EnableMovementInput(true);
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
