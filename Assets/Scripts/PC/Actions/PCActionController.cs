using System.Collections;
using UnityEngine;

/// <summary>
/// PCComponent that manages interaction with objects and subjects
/// </summary>
[CreateAssetMenu(menuName = "PCComponents/Action Controller")]
public class PCActionController : PCComponent
{
    //Selected verb in ActionVerb
    [SerializeField, HideInInspector]
    private ActionVerb selectedVerb;

    //Selected verb in ActionVerb that is currently in use (or waiting for clicking target object in case of a multi-object verb)
    [SerializeField, HideInInspector]
    private UseOfVerb currentVerb;

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
        }
    }

    private ActionVerbsUIController actionVerbsUIController;
    public ActionVerbsUIController ActionVerbsUIController
    {
        get
        {
            if (actionVerbsUIController == null) actionVerbsUIController = GeneralUIController.actionVerbsUIController;
            return actionVerbsUIController;
        }
    }     

    /// <summary>
    /// Returns selected verb
    /// </summary>
    /// <returns></returns>
    public ActionVerb GetSelectedVerb()
    {
        return selectedVerb;
    }

    /// <summary>
    /// Sets selected verb
    /// </summary>
    /// <param name="verb"></param>
    public void SetSelectedVerb(ActionVerb verb)
    {
        selectedVerb = verb;
        CancelCurrentVerb();
    }

    /// <summary>
    /// Returns current verb
    /// </summary>
    /// <returns></returns>
    public UseOfVerb GetCurrentVerb()
    {
        return currentVerb;
    }

    /// <summary>
    /// Sets current verb
    /// </summary>
    /// <param name="verb"></param>
    public void SetCurrentVerb(UseOfVerb verb)
    {
        currentVerb = verb;
    }

    /// <summary>
    /// Resets action verb bar verb info and current verb
    /// </summary>
    public void CancelCurrentVerb()
    {
        ActionVerbsUIController.SetActionTextHighlighted(false);
        ActionVerbsUIController.SetSelectedVerbInfo(selectedVerb.singleObjActionInfo);
        SetCurrentVerb(null);
    }

    /// <summary>
    /// Executes the mainUseOfVerb received as a parameter
    /// </summary>
    /// <param name="mainUseOfVerb"></param>
    /// <param name="targetUseOfVerb"></param>
    /// <returns></returns>
    public IEnumerator ExecuteVerb(UseOfVerb mainUseOfVerb, UseOfVerb targetUseOfVerb)
    {
        ActionVerbsUIController.SetActionTextHighlighted(true);

        IEnumerator movementCoroutine;

        //If it's a multi-object verb, the actuatorObj is pickableObj (it has to be) and it is not an inventory item
        if (mainUseOfVerb.multiObj && mainUseOfVerb.actuatorObj is PickableObjBehavior pickableObj && !pickableObj.inventoryObj)
        {
            //First, Óliver has to pick that object
            UseOfVerb pickUseOfVerb = pickableObj.GetUseOfVerb(DataManager.Instance.verbsDictionary["pick"]);

            //Executes movement
            movementCoroutine = ExecuteMovement(pickUseOfVerb);

            AddVerbExecutionCoroutine(movementCoroutine);

            yield return StartCoroutine(movementCoroutine);

            RemoveVerbExecutionCoroutine();

            //Executes action
            yield return StartCoroutine(ExecuteAction(pickUseOfVerb));

            mainUseOfVerb = mainUseOfVerb.CopyUseOfVerb();

            //Sets the new actuatorObj to mainUseOfVerb as the object in inventory
            PickableObjBehavior newActuatorObj = m_PCController.InventoryController.GetInventoryObj(mainUseOfVerb.actuatorObj.obj);
            if (newActuatorObj != null) mainUseOfVerb.actuatorObj = newActuatorObj;
        }

        //Executes movement
        movementCoroutine = ExecuteMovement(mainUseOfVerb, targetUseOfVerb);

        AddVerbExecutionCoroutine(movementCoroutine);

        yield return StartCoroutine(movementCoroutine);

        RemoveVerbExecutionCoroutine();

        //Executes action
        yield return StartCoroutine(ExecuteAction(mainUseOfVerb));

        ActionVerbsUIController.SetActionTextHighlighted(false);
    }

    /// <summary>
    /// Moves Óliver to the interaction point and rotates to the look at point
    /// </summary>
    /// <param name="mainUseOfVerb"></param>
    /// <param name="targetUseOfVerb"></param>
    /// <returns></returns>
    IEnumerator ExecuteMovement(UseOfVerb mainUseOfVerb, UseOfVerb targetUseOfVerb = null)
    {
        //If the mainUseOfVerb is multiObj, that means there has to exist a targetUseOfVerb
        Vector3 pointToMove = transform.position;
        Vector3 pointToLook = (mainUseOfVerb.multiObj) ? mainUseOfVerb.targetObj.GetLookAtPoint().position : mainUseOfVerb.actuatorObj.GetLookAtPoint().position;
        bool dontRotate = false;

        UseOfVerb auxiliarVerb = (mainUseOfVerb.multiObj && targetUseOfVerb != null) ? targetUseOfVerb : mainUseOfVerb;
        switch (auxiliarVerb.verbMovement)
        {
            case VerbMovement.DontMove:
                //If Óliver doesn't have to move, it doesn't rotate either
                dontRotate = true;
                break;
            case VerbMovement.MoveAround:
                //Moves to any point around the actuatorObj at a given distance
                pointToMove = auxiliarVerb.actuatorObj.GetPointAroundObject(transform.position, auxiliarVerb.distanceFromObject);
                break;
            case VerbMovement.MoveToExactPoint:
                //Moves to a exact point determined by the actuatorObj or the UseOfVerb
                if (auxiliarVerb.overrideInteractionPoint != null)
                    pointToMove = auxiliarVerb.overrideInteractionPoint.position;
                else
                    pointToMove = auxiliarVerb.actuatorObj.GetInteractionPoint().position;
                break;
        }

        //Moves to the interactionPoint and rotates to the lookAtPoint
        IEnumerator movementCoroutine = m_PCController.MovementController.MoveAndRotateToPoint(pointToMove, pointToLook, dontRotate);

        AddVerbExecutionCoroutine(movementCoroutine);

        yield return StartCoroutine(movementCoroutine);

        RemoveVerbExecutionCoroutine();
    }

    /// <summary>
    /// When Óliver's in the interactionPoint, action is executed
    /// </summary>
    /// <param name="verb"></param>
    /// <returns></returns>
    IEnumerator ExecuteAction(UseOfVerb verb)
    {
        //Disables PC input
        m_PCController.EnableGameplayInput(false, false);
        m_PCController.EnableInventoryInput(false);

        //Action is a conversation
        if (verb.useType == VerbResult.StartConversation)
        {
            yield return StartCoroutine(verb.actuatorObj._StartConversation(verb.conversation));
        }
        //Action is picking an object
        else if(verb.useType == VerbResult.PickObject)
        {
            yield return StartCoroutine(verb.actuatorObj._GetPicked());
        }
        //Action is stealing an object
        else if(verb.useType == VerbResult.StealObject)
        {
            yield return StartCoroutine(verb.actuatorObj._GetStolen());
        }
        //Action is think
        else if(verb.useType == VerbResult.Think)
        {
            yield return StartCoroutine(verb.actuatorObj._Think(verb.conversation));
        }
        //Action is executing a specific method from the interacted object behavior
        else if(verb.useType == VerbResult.ExecuteMethod)
        {
            IEnumerator methodCoroutine;
            if (verb.multiObj)
            {
                methodCoroutine = (IEnumerator)verb.methodToExecute.methodInfo.Invoke(verb.actuatorObj, new object[] { verb.targetObj });
            }
            else
            {
                if(verb.methodToExecute.methodInfo.GetParameters().Length == 1)
                {
                    methodCoroutine = (IEnumerator)verb.methodToExecute.methodInfo.Invoke(verb.actuatorObj, new object[] { null });
                }
                else
                {
                    methodCoroutine = (IEnumerator)verb.methodToExecute.methodInfo.Invoke(verb.actuatorObj, null);
                }
            }

            yield return StartCoroutine(methodCoroutine);
        }

        //When action is finished, enables PC input
        m_PCController.EnableGameplayInput(true);
        m_PCController.EnableInventoryInput(!GeneralUIController.displayingDetailedUI);

        CancelCurrentVerb();
    }
}
