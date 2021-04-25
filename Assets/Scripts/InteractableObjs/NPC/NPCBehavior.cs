using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehavior : InteractableObjBehavior
{
    #region Components

    [HideInInspector]
    public NPCMovementController MovementController;

    [HideInInspector]
    public NPCAnimationController AnimationController;

    #endregion

    public static NPCBehavior Instance;

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (MovementController) MovementController.m_NPCController = this;
        if (AnimationController) AnimationController.m_NPCController = this;
    }

    private void Update()
    {
        MovementController.MovementUpdate();
    }

    #region

    public void _LoadData(NPCData data)
    {
        _ApplyData(data.inScene);
    }

    /*
    public void _ApplyData(bool inScene, ...)
    {
        _ApplyData(inScene);
    }
    */

    public override InteractableObjData _GetObjData()
    {
        return new NPCData(inScene);
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
