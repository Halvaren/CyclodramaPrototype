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

    protected override void InitializeObjBehavior()
    {
        base.InitializeObjBehavior();
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

        CameraManager.instance.ChangeToProjectorCamera(detailCameraBehavior);

        PCController.instance.getBackCallback = GetBack;
        PCController.instance.EnableMovementInput(false);

        yield return null;
    }

    public virtual void GetBack()
    {
        TriggerCollider.enabled = true;
        ActivateObjBehaviorColliders(false);
        ActivateLighting(false);

        CameraManager.instance.ChangeToMainCamera();
        PCController.instance.EnableMovementInput(true);
    }

    #region Data methods

    public void _LoadData(ContainerObjData data)
    {
        _ApplyData(data.inScene, data.accessible);
    }

    public void _ApplyData(bool inScene, bool accesible)
    {
        _ApplyData(inScene);
    }

    public override InteractableObjData _GetObjData()
    {
        return new ContainerObjData(inScene, accessible);
    }

    #endregion
}
