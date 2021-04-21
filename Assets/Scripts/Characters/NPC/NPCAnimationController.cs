using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPCComponents/Animation Controller")]
public class NPCAnimationController : NPCComponent
{
    private Animator m_Animator;
    public Animator Animator
    {
        get
        {
            if (m_Animator == null) m_Animator = GetComponent<Animator>();
            return m_Animator;
        }
    }
}
