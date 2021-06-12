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
    public SetLocation location;

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

    public virtual IEnumerator GoToPoint(Vector3 point, bool running, bool makePlayerObstacle = false, bool makeNPCObstacleAfter = false, bool stopAnimationAtEnd = true)
    {
        if (makePlayerObstacle)
        {
            PCController.MovementController.ActivateAgent(false);
            PCController.MovementController.ActivateObstacle(true);
        }
        MovementController.ActivateObstacle(false);
        RecalculateMesh();
        MovementController.ActivateAgent(true);

        SetWalking(true);
        SetRunning(running);
        MovementController.MoveTo(point);

        while (!MovementController.IsOnPoint(point))
        {
            yield return null;
        }

        if(stopAnimationAtEnd)
        {
            SetWalking(false);
            SetRunning(false);
        }

        if (makeNPCObstacleAfter || makePlayerObstacle)
        {
            if (makeNPCObstacleAfter)
            {
                MovementController.ActivateAgent(false);
                MovementController.ActivateObstacle(true);
            }
            if (makePlayerObstacle) PCController.MovementController.ActivateObstacle(false);
            RecalculateMesh();
            if (makePlayerObstacle) PCController.MovementController.ActivateAgent(true);
        }
    }

    public virtual IEnumerator SpawnFromDoor(bool running, SetDoorBehavior door, Vector3 enterDirection, float spawnDistance, bool stopAnimationAtEnd = true, bool closeDoor = false)
    {
        transform.position = door.interactionPoint.position + spawnDistance * -enterDirection;
        MakeObjectInvisible(false);

        bool doorClosed = !door.opened;

        if(doorClosed)
        {
            SetWalking(false);
            if (doorClosed) yield return StartCoroutine(door.OpenDoor());
        }

        MovementController.ActivateAgent(false);
        SetWalking(true);
        SetRunning(running);
        yield return StartCoroutine(MovementController.MoveInDirectionToPoint(door.interactionPoint.position, enterDirection, running));
        if(stopAnimationAtEnd)
        {
            SetWalking(false);
            SetRunning(false);
        }

        yield return StartCoroutine(door.CloseDoor());

        MovementController.ActivateObstacle(true);
        RecalculateMesh();
    }

    public virtual IEnumerator GoToDoorAndExit(bool running, SetDoorBehavior door, Vector3 exitDirection, bool closeDoor = false, float timeToDisappear = -1f)
    {
        yield return StartCoroutine(GoToPoint(door.interactionPoint.position, running, true, false, false));

        bool doorClosed = !door.opened;
        if(doorClosed)
        {
            SetWalking(false);
            SetRunning(false);
            if (doorClosed) yield return StartCoroutine(door.OpenDoor());
        }

        MovementController.ActivateAgent(false);
        SetWalking(true);
        SetRunning(running);
        StartCoroutine(MovementController.MoveInDirectionDuringTime(float.MaxValue, exitDirection, running));

        yield return new WaitForSeconds(1f);

        if(doorClosed || closeDoor)
        {
            yield return StartCoroutine(door.CloseDoor());
        }

        if(timeToDisappear >= 0) StartCoroutine(DisappearAfterTime(timeToDisappear));

        PCController.MovementController.ActivateObstacle(false);
        RecalculateMesh();
        PCController.MovementController.ActivateAgent(true);
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

    #region Animation methods

    public virtual void SetWalking(bool value)
    {
        Animator.SetBool("walking", value);
    }

    public virtual void SetRunning(bool value)
    {
        Animator.SetBool("running", value);
    }

    public virtual void SetTalking(bool value)
    {
        Animator.SetBool("talking", value);
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
