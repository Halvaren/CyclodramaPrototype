using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerObjBehavior : InteractableObjBehavior
{
    public DetailCameraBehavior detailCameraBehavior;
    public List<InteractableObjBehavior> objBehaviors;

    protected override void InitializeObjBehavior()
    {
        base.InitializeObjBehavior();
        ActivateObjBehaviorColliders(false); 
    }

    void ActivateObjBehaviorColliders(bool value)
    {
        foreach(InteractableObjBehavior behavior in objBehaviors)
        {
            behavior.TriggerCollider.enabled = value;
        }
    }

    public void LookInto()
    {
        TriggerCollider.enabled = false;
        ActivateObjBehaviorColliders(true);

        detailCameraBehavior.ActivateCamera();

        PCController.Instance.getBackCallback = GetBack;
        PCController.Instance.EnableMovementInput(false);
    }

    public void GetBack()
    {
        TriggerCollider.enabled = true;
        ActivateObjBehaviorColliders(false);

        detailCameraBehavior.DeactivateCamera();
        PCController.Instance.EnableMovementInput(true);
    }
}
