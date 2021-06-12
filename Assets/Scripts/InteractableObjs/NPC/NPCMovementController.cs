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
    public float turnSmoothTime2 = 0.5f;
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

    #endregion

    public bool IsOnPoint(Vector3 point, bool ignoreY = true)
    {
        if (ignoreY) point.y = transform.position.y;
        bool result = (transform.position - point).magnitude <= closeEnoughValue;

        return result;
    }

    public void MovementUpdate()
    {
        if (m_NPCController.gameObject.activeSelf && !Agent.enabled && !Controller.isGrounded)
        {
            Controller.Move(Vector3.up * gravity * Time.deltaTime);
        }
    }

    #region Agent methods

    public void ActivateObstacle(bool value)
    {
        Obstacle.enabled = value;
    }

    public void ActivateAgent(bool value)
    {
        Agent.enabled = value;
    }

    public void AgentMoveTo(Vector3 point)
    {
        if(!Agent.enabled) ActivateAgent(true);

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

    public IEnumerator MoveToPointCoroutine(Vector3 point, float time)
    {
        Vector3 initialPosition = transform.position;
        Vector3 finalPosition = point;
        finalPosition.y = transform.position.y;

        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            transform.position = Vector3.Lerp(initialPosition, finalPosition, elapsedTime / time);

            yield return null;
        }
        transform.position = finalPosition;
    }

    public IEnumerator RotateToDirectionCoroutine(Vector3 direction)
    {
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

    public IEnumerator MoveInDirectionToPoint(Vector3 point, Vector3 enterDirection, bool running)
    {
        Vector3 distance = point - transform.position;
        float distanceProjectionOnDirection = Vector3.Project(distance, enterDirection).magnitude;

        while(distanceProjectionOnDirection > targetRadius)
        {
            float targetAngle = Mathf.Atan2(enterDirection.x, enterDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Controller.Move(enterDirection.normalized * (running ? runningSpeed : walkingSpeed) * Time.deltaTime);

            distance = point - transform.position;
            distanceProjectionOnDirection = Vector3.Project(distance, enterDirection).magnitude;

            yield return null;
        }
    }

    public IEnumerator MoveInDirectionDuringTime(float time, Vector3 exitDirection, bool running)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            float targetAngle = Mathf.Atan2(exitDirection.x, exitDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Controller.Move(exitDirection * (running ? runningSpeed : walkingSpeed) * Time.deltaTime);

            yield return null;
        }
    }
}
