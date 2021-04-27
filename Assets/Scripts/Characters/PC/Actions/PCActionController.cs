using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "PCComponents/Action Controller")]
public class PCActionController : PCComponent
{
    public ActionVerb close;
    public ActionVerb convince;
    public ActionVerb draw;
    public ActionVerb forceLock;
    public ActionVerb give;
    public ActionVerb hit;
    public ActionVerb inspect;
    public ActionVerb look;
    public ActionVerb open;
    public ActionVerb pick;
    public ActionVerb pull;
    public ActionVerb push;
    public ActionVerb repair;
    public ActionVerb sing;
    public ActionVerb steal;
    public ActionVerb talkTo;
    public ActionVerb think;
    public ActionVerb @throw;
    public ActionVerb use;
    public ActionVerb walkTo;

    [SerializeField, HideInInspector]
    private ActionVerb selectedVerb;

    [SerializeField, HideInInspector]
    private UseOfVerb currentVerb;
    [SerializeField, HideInInspector]
    private UseOfVerb verbInExecution;
    [SerializeField, HideInInspector]
    private InteractableObjBehavior behaviorInExecution;

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
        SetCurrentVerb(null);
    }

    public UseOfVerb GetVerbInExecution()
    {
        return verbInExecution;
    }

    public void SetVerbAndBehaviorInExecution(UseOfVerb verb, InteractableObjBehavior behavior)
    {
        verbInExecution = verb;
        behaviorInExecution = behavior;
    }

    public void CancelVerbInExecution()
    {
        SetVerbAndBehaviorInExecution(null, null);
    }

    public void ExecuteCurrentVerb()
    {
        switch (verbInExecution.useType)
        {
            case VerbResult.StartConversation:
                m_PCController.DialogueUIController.Interact(behaviorInExecution, verbInExecution.conversation);
                break;
            case VerbResult.PickObject:
                verbInExecution.actuatorObj._GetPicked();
                break;
            case VerbResult.ExecuteMethod:
                if(verbInExecution.multiObj)
                    verbInExecution.methodToExecute.methodInfo.Invoke(verbInExecution.actuatorObj, new object[] { verbInExecution.targetObj });
                else
                    verbInExecution.methodToExecute.methodInfo.Invoke(verbInExecution.actuatorObj, null);
                break;
        }
        CancelVerbInExecution();
        CancelCurrentVerb();
    }
}
