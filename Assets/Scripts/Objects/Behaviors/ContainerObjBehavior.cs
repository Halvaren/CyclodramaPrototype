using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerObjBehavior : InteractableObjBehavior
{
    public DetailCameraBehavior detailCameraBehavior;
    public List<InteractableObjBehavior> objBehaviors;
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

    public void LookInto()
    {
        TriggerCollider.enabled = false;
        ActivateObjBehaviorColliders(true);
        ActivateLighting(true);

        CameraManager.instance.ChangeToProjectorCamera(detailCameraBehavior);

        PCController.Instance.getBackCallback = GetBack;
        PCController.Instance.EnableMovementInput(false);
    }

    public void GetBack()
    {
        TriggerCollider.enabled = true;
        ActivateObjBehaviorColliders(false);
        ActivateLighting(false);

        CameraManager.instance.ChangeToMainCamera();
        PCController.Instance.EnableMovementInput(true);
    }
}
