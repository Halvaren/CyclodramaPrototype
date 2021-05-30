using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NPCLocation
{
    Stage, StageLeftSide, StageRightSide, BehindStage, 
    AtrezzoWarehouse, AtrezzoWorkshop, 
    Corridor1, DressingRoom1, Bathroom1, 
    Corridor2, EmployeeZone, CostumeWorkshop
}

public class NPCBehavior : InteractableObjBehavior
{
    #region Components

    [HideInInspector]
    public NPCMovementController MovementController;

    protected Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    #endregion

    [HideInInspector]
    public VIDE_Assign defaultGiveAnswer;
    [HideInInspector]
    public VIDE_Assign defaultConvinceAnswer;

    [HideInInspector]
    public NPCLocation location;

    [HideInInspector]
    public bool firstTimeTalk;

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
        currentSet.GetComponent<NavMeshSurface>().BuildNavMesh();
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
