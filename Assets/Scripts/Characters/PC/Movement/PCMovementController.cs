using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

[CreateAssetMenu(menuName = "PCComponents/Movement Controller")]
public class PCMovementController : PCComponent
{
    public float walkingSpeed;
    public float runningSpeed;

    bool running = false;

    public float gravity = -12f;

    public float turnSmoothTime = 0.1f;
    public float turnSmoothTime2 = 0.5f;
    float turnSmoothVelocity;

    public float targetRadius;

    const float closeEnoughValue = 0.0001f;

    Coroutine moveRotateAndExecuteCoroutine;

    #region Components

    private UnityEngine.AI.NavMeshAgent m_Agent;
    public UnityEngine.AI.NavMeshAgent Agent
    {
        get
        {
            if (m_Agent == null)
                m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

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

    private PCAnimationController m_AnimationController;
    public PCAnimationController AnimationController
    {
        get
        {
            if (m_AnimationController == null)
                m_AnimationController = m_PCController.AnimationController;

            return m_AnimationController;
        }
    }

    #endregion

    public void MovementUpdate(float horizontal, float vertical, bool running)
    {
        if (!Agent.enabled && !Controller.isGrounded)
        {
            Controller.Move(Vector3.up * gravity * Time.deltaTime);
        }

        this.running = running;
        Agent.speed = this.running ? runningSpeed : walkingSpeed;

        if (Agent.enabled)
        {
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            AnimationController.SetWalking(Move(direction) || Agent.velocity.magnitude > Mathf.Epsilon);
            AnimationController.SetRunning(running);
        }
    }

    public bool Move(Vector3 direction)
    {
        if (direction.magnitude > closeEnoughValue * 10 && Agent.enabled)
        {
            Agent.isStopped = true;

            RotateToDirection(direction);

            Agent.Move(direction * (running ? runningSpeed : walkingSpeed) * Time.deltaTime);

            return true;
        }

        return false;
    }

    public void RotateToDirection(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    public IEnumerator MoveAndRotateToPoint(Vector3 targetPoint, Vector3 lookAtPoint, bool dontRotate = false)
    {
        AgentMoveTo(targetPoint);

        while(!IsOnPoint(targetPoint))
        {
            yield return null;
        }

        if(!dontRotate)
        {
            Vector3 direction = lookAtPoint - transform.position;
            direction.y = 0f;
            Quaternion initialRotation = transform.rotation;
            Quaternion finalRotation = Quaternion.LookRotation(direction);

            float elapsedTime = 0.0f;

            while (elapsedTime < turnSmoothTime2)
            {
                elapsedTime += Time.deltaTime;

                transform.rotation = Quaternion.Lerp(initialRotation, finalRotation, elapsedTime / turnSmoothTime2);

                yield return null;
            }
            transform.rotation = finalRotation;
        }
    }

    bool IsOnPoint(Vector3 point, bool ignoreY = true)
    {
        if (ignoreY) point.y = transform.position.y;
        bool result = (transform.position - point).magnitude <= closeEnoughValue;

        return result;
    }

    #region Controller methods

    public void ControllerMoveTo(Transform target, bool xMovement = true, bool yMovement = true, bool zMovement = true, Action movementDoneCallback = null)
    {
        StartCoroutine(ControllerMoveToCoroutine(target, xMovement, yMovement, zMovement, movementDoneCallback));
    }

    IEnumerator ControllerMoveToCoroutine(Transform target, bool xMovement = true, bool yMovement = true, bool zMovement = true, Action movementDoneCallback = null)
    {
        AnimationController.SetWalking(true);

        Vector3 direction = target.position - transform.position;
        if (!xMovement) direction.x = 0f;
        if (!yMovement) direction.y = 0f;
        if (!zMovement) direction.z = 0f;

        while (direction.magnitude > targetRadius)
        {
            AnimationController.SetRunning(running);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Controller.Move(direction.normalized * (running ? runningSpeed : walkingSpeed) * Time.deltaTime);

            direction = target.position - transform.position;
            if (!xMovement) direction.x = 0f;
            if (!yMovement) direction.y = 0f;
            if (!zMovement) direction.z = 0f;

            yield return null;
        }

        if (movementDoneCallback != null) movementDoneCallback();
        AnimationController.StopMovement();
    }

    #endregion

    #region Agent methods

    public void ActivateAgent(bool value)
    {
        Agent.enabled = value;
    }

    public void AgentMoveTo(Vector3 point)
    {
        ActivateAgent(true);

        Agent.isStopped = false;
        Agent.SetDestination(point);
    }

    #endregion

    public void SetParentWhenAbove(Transform parent)
    {
        transform.parent = null;

        StartCoroutine(SetParentWhenAboveCoroutine(parent));
    }

    IEnumerator SetParentWhenAboveCoroutine(Transform parent)
    {
        while (transform.parent == null)
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (hitInfo.collider.transform.root == parent)
                {
                    transform.parent = parent;
                }
            }

            yield return null;
        }
    }
}
