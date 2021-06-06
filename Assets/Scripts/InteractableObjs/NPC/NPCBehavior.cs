using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class NPCBehavior : InteractableObjBehavior
{
    #region Components

    [HideInInspector]
    public NPCMovementController MovementController;

    #endregion

    [HideInInspector]
    public VIDE_Assign defaultGiveAnswer;
    [HideInInspector]
    public VIDE_Assign defaultConvinceAnswer;

    [HideInInspector]
    public CharacterLocation location;

    [HideInInspector]
    public bool firstTimeTalk;

    [HideInInspector]
    public AudioClip[] footstepClips;
    int footstepClipPointer = 0;

    [HideInInspector]
    public AudioClip chairSittingClip;
    [HideInInspector]
    public AudioClip chairStandUpClip;
    [HideInInspector]
    public AudioClip couchSittingClip;
    [HideInInspector]
    public AudioClip couchStandUpClip;

    AudioClip sittingClip;
    AudioClip standUpClip;

    protected bool movementUpdate = false;

    public static NPCBehavior Instance;

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (MovementController) MovementController.m_NPCController = this;
    }

    private void Update()
    {
        if(movementUpdate) MovementController.MovementUpdate();
    }

    public override IEnumerator _PlayInitialBehavior()
    { 
        yield return base._PlayInitialBehavior();
        movementUpdate = true;
        yield return null;
    }

    public virtual IEnumerator TalkMethod()
    {
        yield return null;
    }

    protected virtual IEnumerator DisappearAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        gameObject.SetActive(false);
    }

    public void RecalculateMesh()
    {
        currentSet.GetComponent<SetBehavior>().RecalculateMesh();
    }

    #region Data methods

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is NPCData npcData)
        {
            firstTimeTalk = npcData.firstTimeTalk;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new NPCData(inScene, firstTimeTalk);
    }

    #endregion

    public void PlayFootstepSound()
    {
        AudioManager.PlaySound(footstepClips[footstepClipPointer++], SoundType.Footstep);

        if (footstepClipPointer >= footstepClips.Length) footstepClipPointer = 0;
    }

    public void SetSittingSound(SeatType type)
    {
        switch (type)
        {
            case SeatType.Chair:
                sittingClip = chairSittingClip;
                break;
            case SeatType.Couch:
                sittingClip = couchSittingClip;
                break;
        }
    }

    public void SetStandUpSound(SeatType type)
    {
        switch (type)
        {
            case SeatType.Chair:
                standUpClip = chairStandUpClip;
                break;
            case SeatType.Couch:
                standUpClip = couchStandUpClip;
                break;
        }
    }

    public void PlaySittingSound()
    {
        if (sittingClip != null) AudioManager.PlaySound(sittingClip, SoundType.Character);
    }

    public void PlayStandUpSound()
    {
        if (standUpClip != null) AudioManager.PlaySound(standUpClip, SoundType.Character);
    }
}

public class NPCComponent : ScriptableObject
{
    [NonSerialized]
    public NPCBehavior m_NPCController;

    public Transform transform { get { return m_NPCController.transform; } }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return m_NPCController.StartCoroutine(routine);
    }

    public T GetComponent<T>() where T : Component
    {
        return m_NPCController.GetComponent<T>();
    }
}
