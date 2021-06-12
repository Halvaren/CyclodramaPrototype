using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class BobBehavior : NPCBehavior
{
    public VIDE_Assign firstTimeConv;
    public VIDE_Assign secondTimeConv;

    public AudioClip bobTheme;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        if(location == SetLocation.EmployeeZone)
        {
            MakeObjectInvisible(PCController.pcData.employeeZoneInitialCutsceneActive);
        }
    }

    public override IEnumerator TalkMethod()
    {
        if (firstTimeTalk)
        {
            AudioSource bobThemeSource = AudioManager.PlaySound(bobTheme, SoundType.ForegroundMusic);
            yield return StartCoroutine(_StartConversation(firstTimeConv));
            AudioManager.FadeOutSound(bobThemeSource, 3f);

            firstTimeTalk = false;
        }
        else
        {
            yield return StartCoroutine(_StartConversation(secondTimeConv));
        }
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

    #endregion
}
