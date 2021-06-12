using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class YaizaBehavior : NPCBehavior
{
    public VIDE_Assign firstTimeConv;

    public string stopTextingTrigger = "stopTexting";

    bool wasTexting = false;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        if(location == SetLocation.Corridor2)
        {
            MakeObjectInvisible(true);
        }
        else if(location == SetLocation.EmployeeZone)
        {
            MakeObjectInvisible(!PCController.pcData.employeeZoneInitialCutsceneActive);
        }
        else if(location == SetLocation.StageLeftSide)
        {
            MakeObjectInvisible(false);
            SetTexting(true);
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
        if(wasTexting) SetTexting(true);
        wasTexting = false;
    }

    public override void OnNodeChange(VD.NodeData data)
    {
        SetTalking(data.tag == obj.name);

        if(data.extraVars.ContainsKey(stopTextingTrigger))
        {
            SetTexting(false);
            wasTexting = true;
        }

        base.OnNodeChange(data);
    }

    #region Animations

    public void SetTexting(bool value)
    {
        Animator.SetBool("texting", value);
    }

    #endregion
}
