﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PickAnimationHeight
{
    Small, Medium, Large
}

public enum PickAnimationWeight
{
    Small, Medium, Large
}

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

    public void UseKnife()
    {
        Animator.SetTrigger("UseKnife");
    }

    public void Seat(SeatType seatType)
    {
        m_PCController.SetSittingSound(seatType);

        Animator.SetTrigger("Seat");
    }

    public void StandUp(SeatType seatType)
    {
        m_PCController.SetStandUpSound(seatType);

        Animator.SetTrigger("StandUp");
    }

    public void ReachWithGolfClub()
    {
        Animator.SetTrigger("ReachGolfClub");
    }

    public void PickObject(PickAnimationHeight height, PickAnimationWeight weight)
    {
        Animator.SetTrigger("PickObj" + height.ToString()[0] + weight.ToString()[0]);
    }

    public void StealObject(PickAnimationHeight height)
    {
        Animator.SetTrigger("StealObj" + height.ToString()[0]);
    }

    public void GiveObj()
    {
        Animator.SetTrigger("GiveObj");
    }

    public void GivenObj()
    {
        Animator.SetTrigger("GivenObj");
    }

    public void ThrowCup()
    {
        Animator.SetTrigger("ThrowCup");
    }
}
