using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class MikeBehavior : NPCBehavior
{
    [Header("Conversations")]
    public VIDE_Assign firstTimeConv;
    public VIDE_Assign secondTimeConv;

    [HideInInspector]
    public bool toldOliverIsRaul = false;

    [Header("Node triggers")]
    public string secondConvBeginning = "secondConvBeginning";
    public string toldOliverIsRaulTrigger = "toldOliverIsRaul";

    [Header("Node IDs to jump")]
    public int hiOliverNodeID = 7;
    public int hiRaulNodeID = 8;

    [Space(15)]
    public AudioClip mikeTheme;

    public override IEnumerator TalkMethod()
    {
        if (firstTimeTalk)
        {
            AudioSource mikeThemeSource = AudioManager.PlaySound(mikeTheme, SoundType.ForegroundMusic);
            yield return StartCoroutine(_StartConversation(firstTimeConv));
            AudioManager.FadeOutSound(mikeThemeSource, 3f);

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

    public override IEnumerator _NextDialogue(VIDE_Assign dialogue)
    {
        VD.NodeData data = VD.nodeData;

        if(data.extraVars.ContainsKey(toldOliverIsRaulTrigger))
        {
            toldOliverIsRaul = true;
        }
        else if(data.extraVars.ContainsKey(secondConvBeginning))
        {
            if(toldOliverIsRaul)
            {
                VD.SetNode(hiRaulNodeID);
            }
            else
            {
                VD.SetNode(hiOliverNodeID);
            }

            yield break;
        }

        yield return base._NextDialogue(dialogue);
    }

    public override void OnNodeChange(VD.NodeData data)
    {
        SetTalking(data.tag == obj.name);

        base.OnNodeChange(data);
    }

    #region Animations

    #endregion

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if (data is MikeData mikeData)
        {
            toldOliverIsRaul = mikeData.toldOliverIsRaul;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new MikeData(inScene, firstTimeTalk, toldOliverIsRaul);
    }
}
