using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class RaulBehavior : NPCBehavior
{
    public VIDE_Assign firstTimeConv;

    public AudioClip hammerHitSound;
    public ParticleSystem[] hammerHitPSs;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        if (location == SetLocation.EmployeeZone)
        {
            MakeObjectInvisible(!PCController.pcData.employeeZoneInitialCutsceneActive);
        }
        else if(location == SetLocation.AtrezzoWorkshop)
        {
            MakeObjectInvisible(false);
            SetWorking(true);
        }
    }
    public override IEnumerator TalkMethod()
    {
        yield return StartCoroutine(_StartConversation(firstTimeConv));
    }

    public override IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        SetWorking(false);

        yield return base._BeginDialogue(dialogue);

        SetTalking(false);
        SetWorking(true);
    }

    public override void OnNodeChange(VD.NodeData data)
    {
        SetTalking(data.tag == obj.name);

        base.OnNodeChange(data);
    }

    #region Animations

    public void SetWorking(bool value)
    {
        Animator.SetBool("working", value);
    }

    #endregion

    public void PlayHammerHitSound()
    {
        AudioManager.PlaySound(hammerHitSound, SoundType.Set);
    }

    public void PlayHammerHitParticles()
    {
        foreach(ParticleSystem ps in hammerHitPSs)
        {
            ps.Play();
        }
    }
}
