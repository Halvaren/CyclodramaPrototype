using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class VeronicaBehavior : NPCBehavior
{
    public VIDE_Assign firstTimeConv;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        if (location == SetLocation.EmployeeZone)
        {
            MakeObjectInvisible(!PCController.pcData.employeeZoneInitialCutsceneActive);
        }
        else
        {
            MakeObjectInvisible(false);
        }
    }

    public override IEnumerator TalkMethod()
    {
        yield return StartCoroutine(_StartConversation(firstTimeConv));
    }

    public override IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        yield return base._BeginDialogue(dialogue);

        SetTalking(false);
    }

    public override void OnNodeChange(VD.NodeData data)
    {
        SetTalking(data.tag == obj.name);

        base.OnNodeChange(data);
    }

    #region Animations

    public void SetTalking(bool value)
    {
        Animator.SetBool("talking", value);
    }

    #endregion
}
