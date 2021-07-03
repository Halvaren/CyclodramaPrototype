using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the behavior of an object that contains other objects and it can be looked in detail
/// </summary>
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

    /// <summary>
    /// Initializes the behavior
    /// </summary>
    /// <param name="currentSet"></param>
    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);
        ActivateObjBehaviorColliders(false);
        ActivateLighting(false);
    }

    /// <summary>
    /// Activates (or deactivates) the triggers colliders of the contained objects
    /// </summary>
    /// <param name="value"></param>
    void ActivateObjBehaviorColliders(bool value)
    {
        foreach(InteractableObjBehavior behavior in objBehaviors)
        {
            behavior.TriggerCollider.enabled = value;
        }
    }

    /// <summary>
    /// Turns on or off the detail lighting
    /// </summary>
    /// <param name="value"></param>
    void ActivateLighting(bool value)
    {
        foreach(Light light in detailLighting)
        {
            light.enabled = value;
        }
    }

    /// <summary>
    /// Coroutine that changes the point of view to look into the container object
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Gets back to the regular point of view
    /// </summary>
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

    /// <summary>
    /// Loads the data received as a parameter in the variables
    /// </summary>
    /// <param name="data"></param>
    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is ContainerObjData containerObjData)
        {
            accessible = containerObjData.accessible;
        }
    }

    /// <summary>
    /// Returns a data object with the info of the behavior
    /// </summary>
    /// <returns></returns>
    public override InteractableObjData GetObjData()
    {
        return new ContainerObjData(inScene, accessible);
    }

    #endregion
}
