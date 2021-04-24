using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "NPCComponents/Movement Controller")]
public class NPCMovementController : NPCComponent
{
    public float walkingSpeed;
    public float runningSpeed;

    bool running = false;

    public float gravity = -12f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float targetRadius;

    const float closeEnoughValue = 0.00001f;

    #region Components

    private NavMeshObstacle m_Obstacle;
    public NavMeshObstacle Obstacle
    {
        get
        {
            if (m_Obstacle == null)
                m_Obstacle = GetComponent<NavMeshObstacle>();

            return m_Obstacle;
        }
    }

    private NavMeshAgent m_Agent;
    public NavMeshAgent Agent
    {
        get
        {
            if (m_Agent == null)
                m_Agent = GetComponent<NavMeshAgent>();

            return m_Agent;
        }
    }

    private CharacterController m_Controller;
    public CharacterController Controller
    {
        get
        {
            if (m_Controller == null)
                m_Controller = GetComponent<CharacterController>();

            return m_Controller;
        }
    }

    private NPCAnimationController m_AnimationController;
    public NPCAnimationController AnimationController
    {
        get
        {
            if (m_AnimationController == null)
                m_AnimationController = m_NPCController.AnimationController;

            return m_AnimationController;
        }
    }

    #endregion

    public bool IsOnPoint(Vector3 point, bool ignoreY = true)
    {
        if (ignoreY) point.y = transform.position.y;
        bool result = (transform.position - point).magnitude <= closeEnoughValue;

        return result;
    }
    public void MovementUpdate()
    {
        if (!Agent.enabled && !Controller.isGrounded)
        {
            Controller.Move(Vector3.up * gravity * Time.deltaTime);
        }
    }

    #region Agent methods

    public void ActivateAgent(bool value)
    {
        Agent.enabled = value;
        Obstacle.enabled = !value;
    }

    public void AgentMoveTo(Vector3 point)
    {
        ActivateAgent(true);

        Agent.isStopped = false;
        Agent.SetDestination(point);
    }

    #endregion

    public void MoveTo(Vector3 point)
    {
        AgentMoveTo(point);
    }

    public bool Move(Vector3 direction)
    {
        if (direction.magnitude > closeEnoughValue && Agent.enabled)
        {
            Agent.isStopped = true;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Agent.Move(direction * (running ? runningSpeed : walkingSpeed) * Time.deltaTime);

            return true;
        }

        return false;
    }

}
