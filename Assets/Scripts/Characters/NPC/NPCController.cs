using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum SubjState
{
    Normal, Seated, Working, SpecialAction1, Absent
}

public class NPCController : InteractableObjBehavior
{
    public SubjState subjState;

    #region Components

    public NPCMovementController MovementController;

    public NPCAnimationController AnimationController;

    #endregion

    public static NPCController Instance;

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

    private void OnDrawGizmos()
    {
        if (obj != null)
        {
            UseOfVerb openVerb = null;
            for (int i = 0; i < useOfVerbs.Count; i++)
            {
                if (useOfVerbs[i].verb.name == "Talk to")
                {
                    openVerb = useOfVerbs[i];
                }
            }

            if (openVerb != null && openVerb.verbMovement == VerbMovement.MoveToExactPoint)
            {
                float radius = (transform.position - transform.TransformPoint(openVerb.pointToMove.position)).magnitude;
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.TransformPoint(openVerb.pointToMove.position), 0.5f);
                Gizmos.DrawWireSphere(transform.position, radius);
            }
        }
    }
}

public class NPCComponent : ScriptableObject
{
    [NonSerialized]
    public NPCController m_NPCController;

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
