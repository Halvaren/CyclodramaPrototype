using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "PCComponents/Action Controller")]
public class PCActionController : PCComponent
{
    public List<ActionVerb> actionVerbs;

    [SerializeField]
    private ActionVerb selectedVerb;

    [SerializeField]
    private UseOfVerb currentVerb;
    [SerializeField]
    private UseOfVerb verbInExecution;

    public void AddVerb(ActionVerb verb)
    {
        actionVerbs.Add(verb);
    }

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

    public void SetVerbInExecution(UseOfVerb verb)
    {
        verbInExecution = verb;
    }

    public void CancelVerbInExecution()
    {
        SetVerbInExecution(null);
    }

    public void ExecuteCurrentVerb()
    {
        switch (verbInExecution.useType)
        {
            case VerbResult.StartConversation:
                m_PCController.DialogueUIController.Interact(verbInExecution.conversation);
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
