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

    public void ExecuteCurrentVerb()
    {
        switch (currentVerb.useType)
        {
            case VerbResult.StartConversation:
                m_PCController.dialogueUIController.Interact(currentVerb.conversation);
                break;
            case VerbResult.PickObject:
                currentVerb.target._GetPicked();
                break;
            case VerbResult.ExecuteMethod:
                currentVerb.methodToExecute.methodInfo.Invoke(currentVerb.target, null);
                break;
        }
        CancelCurrentVerb();
    }
}
