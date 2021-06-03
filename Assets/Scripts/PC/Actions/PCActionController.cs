﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "PCComponents/Action Controller")]
public class PCActionController : PCComponent
{
    [SerializeField, HideInInspector]
    private ActionVerb selectedVerb;

    [SerializeField, HideInInspector]
    private UseOfVerb currentVerb;

    public ActionVerb GetSelectedVerb()
    {
        return selectedVerb;
    }

    public void SetSelectedVerb(ActionVerb verb)
    {
        selectedVerb = verb;
        CancelCurrentVerb();
    }

    public UseOfVerb GetCurrentVerb()
    {
        return currentVerb;
    }

    public void SetCurrentVerb(UseOfVerb verb)
    {
        currentVerb = verb;
    }

    public void CancelCurrentVerb()
    {
        GeneralUIController.instance.actionVerbsUIController.SetSelectedVerbInfo(selectedVerb.singleObjActionInfo);
        SetCurrentVerb(null);
    }

    public IEnumerator ExecuteVerb(UseOfVerb mainUseOfVerb, UseOfVerb targetUseOfVerb)
    {
        IEnumerator movementCoroutine;

        if (mainUseOfVerb.multiObj && mainUseOfVerb.actuatorObj is PickableObjBehavior pickableObj && !pickableObj.inventoryObj)
        {
            UseOfVerb pickUseOfVerb = pickableObj.GetUseOfVerb(DataManager.Instance.verbsDictionary["pick"]);

            movementCoroutine = ExecuteMovement(pickUseOfVerb);

            AddVerbExecutionCoroutine(movementCoroutine);

            yield return StartCoroutine(movementCoroutine);

            RemoveVerbExecutionCoroutine();

            yield return StartCoroutine(ExecuteAction(pickUseOfVerb));

            mainUseOfVerb = mainUseOfVerb.CopyUseOfVerb();

            PickableObjBehavior newActuatorObj = m_PCController.InventoryController.GetInventoryObj(mainUseOfVerb.actuatorObj.obj);
            if (newActuatorObj != null) mainUseOfVerb.actuatorObj = newActuatorObj;
        }

        movementCoroutine = ExecuteMovement(mainUseOfVerb, targetUseOfVerb);

        AddVerbExecutionCoroutine(movementCoroutine);

        yield return StartCoroutine(movementCoroutine);

        RemoveVerbExecutionCoroutine();

        yield return StartCoroutine(ExecuteAction(mainUseOfVerb));
    }

    IEnumerator ExecuteMovement(UseOfVerb mainUseOfVerb, UseOfVerb targetUseOfVerb = null)
    {
        Vector3 pointToMove = transform.position;
        Vector3 pointToLook = (mainUseOfVerb.multiObj) ? mainUseOfVerb.targetObj.GetLookAtPoint().position : mainUseOfVerb.actuatorObj.GetLookAtPoint().position;
        bool dontRotate = false;

        UseOfVerb auxiliarVerb = (mainUseOfVerb.multiObj && targetUseOfVerb != null) ? targetUseOfVerb : mainUseOfVerb;
        switch (auxiliarVerb.verbMovement)
        {
            case VerbMovement.DontMove:
                dontRotate = true;
                break;
            case VerbMovement.MoveAround:
                pointToMove = auxiliarVerb.actuatorObj.GetPointAroundObject(transform.position, auxiliarVerb.distanceFromObject);
                break;
            case VerbMovement.MoveToExactPoint:
                if (auxiliarVerb.overrideInteractionPoint != null)
                    pointToMove = auxiliarVerb.overrideInteractionPoint.position;
                else
                    pointToMove = auxiliarVerb.actuatorObj.GetInteractionPoint().position;
                break;
        }

        IEnumerator movementCoroutine = m_PCController.MovementController.MoveAndRotateToPoint(pointToMove, pointToLook, dontRotate);

        AddVerbExecutionCoroutine(movementCoroutine);

        yield return StartCoroutine(movementCoroutine);

        RemoveVerbExecutionCoroutine();
    }

    IEnumerator ExecuteAction(UseOfVerb verb)
    {
        m_PCController.EnableGameplayInput(false);
        m_PCController.EnableInventoryInput(false);

        if (verb.useType == VerbResult.StartConversation)
        {
            yield return StartCoroutine(verb.actuatorObj._StartConversation(verb.conversation));
        }
        else if(verb.useType == VerbResult.PickObject)
        {
            yield return StartCoroutine(verb.actuatorObj._GetPicked());
        }
        else if(verb.useType == VerbResult.StealObject)
        {
            yield return StartCoroutine(verb.actuatorObj._GetStolen());
        }
        else if(verb.useType == VerbResult.Think)
        {
            yield return StartCoroutine(verb.actuatorObj._Think(verb.conversation));
        }
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

        m_PCController.EnableGameplayInput(true);
        m_PCController.EnableInventoryInput(true);

        CancelCurrentVerb();
    }
}
