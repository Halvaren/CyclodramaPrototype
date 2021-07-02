using UnityEngine;

/// <summary>
/// There are three variations in pick and steal animation according on the height where the object to pick or to steal is located
/// </summary>
public enum PickAnimationHeight
{
    Small, Medium, Large
}

/// <summary>
/// There are three variations in pick animation according on the weight of the object to pick
/// </summary>
public enum PickAnimationWeight
{
    Small, Medium, Large
}

/// <summary>
/// PCComponent that manages PC animations
/// </summary>
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

    /// <summary>
    /// Stops movement animations
    /// </summary>
    public void StopMovement()
    {
        Animator.SetBool("Walk", false);
        Animator.SetBool("Run", false);
    }

    /// <summary>
    /// Sets walking animation on or off
    /// </summary>
    /// <param name="value"></param>
    public void SetWalking(bool value)
    {
        Animator.SetBool("Walk", value);
    }

    /// <summary>
    /// Sets running animation on or off
    /// </summary>
    /// <param name="value"></param>
    public void SetRunning(bool value)
    {
        Animator.SetBool("Run", value);
    }

    /// <summary>
    /// Triggers knife slash animation
    /// </summary>
    public void UseKnife()
    {
        Animator.SetTrigger("UseKnife");
    }

    /// <summary>
    /// Triggers seat animation
    /// </summary>
    /// <param name="seatType"></param>
    public void Seat(SeatType seatType)
    {
        m_PCController.SetSittingSound(seatType);

        Animator.SetTrigger("Seat");
    }

    /// <summary>
    /// Triggers stand up animation
    /// </summary>
    /// <param name="seatType"></param>
    public void StandUp(SeatType seatType)
    {
        m_PCController.SetStandUpSound(seatType);

        Animator.SetTrigger("StandUp");
    }

    /// <summary>
    /// Triggers golf club animation
    /// </summary>
    public void ReachWithGolfClub()
    {
        Animator.SetTrigger("ReachGolfClub");
    }

    /// <summary>
    /// Triggers pick animation with its corresponding height and weight parameters
    /// </summary>
    /// <param name="height"></param>
    /// <param name="weight"></param>
    public void PickObject(PickAnimationHeight height, PickAnimationWeight weight)
    {
        Animator.SetTrigger("PickObj" + height.ToString()[0] + weight.ToString()[0]);
    }

    /// <summary>
    /// Triggers steal animation with its corresponding height parameter
    /// </summary>
    /// <param name="height"></param>
    public void StealObject(PickAnimationHeight height)
    {
        Animator.SetTrigger("StealObj" + height.ToString()[0]);
    }

    /// <summary>
    /// Triggers first part of give animation
    /// </summary>
    public void GiveObj()
    {
        Animator.SetTrigger("GiveObj");
    }

    /// <summary>
    /// Triggers second part of give animation
    /// </summary>
    public void GivenObj()
    {
        Animator.SetTrigger("GivenObj");
    }

    /// <summary>
    /// Triggers throw animation with a cup
    /// </summary>
    public void ThrowCup()
    {
        Animator.SetTrigger("ThrowCup");
    }
}
