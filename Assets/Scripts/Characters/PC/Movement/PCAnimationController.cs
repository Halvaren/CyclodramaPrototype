using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "PCComponents/Animation Controller")]
public class PCAnimationController : PCComponent
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

    public void StopMovement()
    {
        Animator.SetBool("Walk", false);
        Animator.SetBool("Run", false);
    }

    public void SetWalking(bool value)
    {
        Animator.SetBool("Walk", value);
    }

    public void SetRunning(bool value)
    {
        Animator.SetBool("Run", value);
    }
}
