using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// PCComponent that manages PC movement
/// PC can move with a NavMeshAgent component or with a CharacterController component. When he's moving within a set (therefore, within a NavMeshSurface), he will
/// do it using the NavMeshAgent. In some cases, like set transition, when he's not moving on a NavMeshSurface, he will use CharacterController to move
/// Also, it has a NavMeshObstacle component to generate a hole in NavMeshSurface when it's activated. This is primarily used during cutscenes, when PC is stopped,
/// NPC or NPCs move and they have to avoid him
/// </summary>
[CreateAssetMenu(menuName = "PCComponents/Movement Controller")]
public class PCMovementController : PCComponent
{
    #region Variables

    public float walkingSpeed;
    public float runningSpeed;

    bool running = false;

    public float gravity = -12f;

    public float turnSmoothTime = 0.1f;
    public float turnSmoothTime2 = 0.5f;
    float turnSmoothVelocity;

    public float targetRadius;

    const float closeEnoughValue = 0.0001f;

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

    private UnityEngine.AI.NavMeshObstacle m_Obstacle;
    public UnityEngine.AI.NavMeshObstacle Obstacle
    {
        get
        {
            if (m_Obstacle == null)
                m_Obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();

            return m_Obstacle;
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

    #endregion

    #region Methods

    /// <summary>
    /// It is executed each frame
    /// </summary>
    /// <param name="horizontal"></param>
    /// <param name="vertical"></param>
    /// <param name="running"></param>
    public void MovementUpdate(float horizontal, float vertical, bool running)
    {
        //If PC is moving with CharacterController and it is not grounded
        if (!Agent.enabled && !Controller.isGrounded)
        {
            //Moves PC to the ground
            Controller.Move(Vector3.up * gravity * Time.deltaTime);
        }

        this.running = running;
        Agent.speed = this.running ? runningSpeed : walkingSpeed;

        //If PC is moving with NavMeshAgent
        if (Agent.enabled)
        {
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            //Moves and animates accordingly to that movement
            AnimationController.SetWalking(Move(direction) || Agent.velocity.magnitude > Mathf.Epsilon);
            AnimationController.SetRunning(running);
        }
    }

    /// <summary>
    /// Moves the character using the NavMeshAgent in the direction passed as parameter
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool Move(Vector3 direction)
    {
        if (direction.magnitude > closeEnoughValue * 10 && Agent.enabled)
        {
            Agent.isStopped = true;

            //Rotates the PC to face the direction
            RotateToDirection(direction);

            Agent.Move(direction * (running ? runningSpeed : walkingSpeed) * Time.deltaTime);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Rotates smoothly the character to the direction passed as parameter
    /// </summary>
    /// <param name="direction"></param>
    public void RotateToDirection(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    /// <summary>
    /// Returns if character is on the point passed as parameter
    /// </summary>
    /// <param name="point"></param>
    /// <param name="ignoreY">If true, it will not check it the Y component of character and the point are equal</param>
    /// <returns></returns>
    bool IsOnPoint(Vector3 point, bool ignoreY = true)
    {
        if (ignoreY) point.y = transform.position.y;
        bool result = (transform.position - point).magnitude <= closeEnoughValue;

        return result;
    }

    #region Controller methods

    /// <summary>
    /// Moves the character using the CharacterController in the directions indicated in the xMovement, yMovement and zMovement parameters
    /// </summary>
    /// <param name="target"></param>
    /// <param name="xMovement"></param>
    /// <param name="yMovement"></param>
    /// <param name="zMovement"></param>
    /// <param name="movementDoneCallback"></param>
    public void ControllerMoveTo(Transform target, bool xMovement = true, bool yMovement = true, bool zMovement = true, Action movementDoneCallback = null)
    {
        StartCoroutine(ControllerMoveToCoroutine(target, xMovement, yMovement, zMovement, movementDoneCallback));
    }

    /// <summary>
    /// Coroutine that moves the character using the CharacterController in the directions indicated in the xMovement, yMovement and zMovement parameters
    /// </summary>
    /// <param name="target"></param>
    /// <param name="xMovement"></param>
    /// <param name="yMovement"></param>
    /// <param name="zMovement"></param>
    /// <param name="movementDoneCallback"></param>
    /// <returns></returns>
    IEnumerator ControllerMoveToCoroutine(Transform target, bool xMovement = true, bool yMovement = true, bool zMovement = true, Action movementDoneCallback = null)
    {
        AnimationController.SetWalking(true);

        //Calculates the initial direction
        Vector3 direction = target.position - transform.position;
        if (!xMovement) direction.x = 0f;
        if (!yMovement) direction.y = 0f;
        if (!zMovement) direction.z = 0f;

        //And the initial angle
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        while (direction.magnitude > targetRadius)
        {
            float newTargetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            if (Mathf.Abs(newTargetAngle - targetAngle) > 90f)
            {
                break;
            }

            AnimationController.SetRunning(running);
            //Rotates smoothly the PC to face the movement direction
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //Moves the PC using CharacterController
            Controller.Move(direction.normalized * (running ? runningSpeed : walkingSpeed) * Time.deltaTime);

            //Recalculates the direction using the new position
            direction = target.position - transform.position;
            if (!xMovement) direction.x = 0f;
            if (!yMovement) direction.y = 0f;
            if (!zMovement) direction.z = 0f;

            yield return null;
        }

        //When movement is finished and the callback action is different from null, executes it
        if (movementDoneCallback != null) movementDoneCallback();
        AnimationController.StopMovement();
    }

    /// <summary>
    /// Makes the PC exit the current scene in the exitDirection using CharacterController
    /// </summary>
    /// <param name="time"></param>
    /// <param name="exitDirection"></param>
    /// <param name="running"></param>
    public void ExitScene(float time, Vector3 exitDirection, bool running)
    {
        StartCoroutine(ExitSceneCoroutine(time, exitDirection, running));
    }

    /// <summary>
    /// Coroutine that makes the PC exit the current scene in the exitDirection using CharacterController
    /// </summary>
    /// <param name="time"></param>
    /// <param name="exitDirection"></param>
    /// <param name="running"></param>
    /// <returns></returns>
    IEnumerator ExitSceneCoroutine(float time, Vector3 exitDirection, bool running)
    {
        float elapsedTime = 0.0f;

        AnimationController.SetWalking(true);

        while (elapsedTime < time)
        {
            //Smoothly rotates the PC to face the exitDirection
            float targetAngle = Mathf.Atan2(exitDirection.x, exitDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //Moves the PC using CharacterController
            Controller.Move(exitDirection * (running ? runningSpeed : walkingSpeed) * Time.deltaTime);

            yield return null;
        }
    }

    #endregion

    #region Agent methods

    /// <summary>
    /// Activates NavMeshAgent
    /// </summary>
    /// <param name="value"></param>
    public void ActivateAgent(bool value)
    {
        Agent.enabled = value;
    }

    /// <summary>
    /// Activates NavMeshObstacle
    /// </summary>
    /// <param name="value"></param>
    public void ActivateObstacle(bool value)
    {
        Obstacle.enabled = value;
    }

    /// <summary>
    /// Sets NavMeshAgent destination
    /// </summary>
    /// <param name="point"></param>
    public void AgentMoveTo(Vector3 point)
    {
        ActivateAgent(true);

        Agent.isStopped = false;
        Agent.SetDestination(point);
    }

    /// <summary>
    /// Moves PC to targetPoint using NavMeshAgent and, then, rotates it to face the direction
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <param name="rotateDirection"></param>
    /// <param name="dontRotate"></param>
    /// <returns></returns>
    public IEnumerator MoveAndRotateToDirection(Vector3 targetPoint, Vector3 rotateDirection, bool dontRotate = false)
    {
        AgentMoveTo(targetPoint);

        while (!IsOnPoint(targetPoint))
        {
            yield return null;
        }

        if (!dontRotate)
        {
            yield return StartCoroutine(RotateToDirectionCoroutine(rotateDirection));
        }
    }

    /// <summary>
    /// Moves PC to targetPoint using NavMeshAgent and, then, rotates it to look at lookAtPoint
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <param name="lookAtPoint"></param>
    /// <param name="dontRotate"></param>
    /// <returns></returns>
    public IEnumerator MoveAndRotateToPoint(Vector3 targetPoint, Vector3 lookAtPoint, bool dontRotate = false)
    {
        AgentMoveTo(targetPoint);

        while (!IsOnPoint(targetPoint))
        {
            yield return null;
        }

        if (!dontRotate)
        {
            yield return StartCoroutine(RotateToDirectionCoroutine(lookAtPoint - transform.position));
        }
    }

    /// <summary>
    /// Smoothly rotates PC to face direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
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

    #endregion

    #region Other methods

    /// <summary>
    /// Starts to search a set below PC to make it his parent in hierarchy. It is used when PC is moving with CharacterController due to a set transition and his
    /// movement has to be affected because he starts walking on a set that it's still moving.
    /// </summary>
    /// <param name="parent"></param>
    public void SetParentWhenAbove(Transform parent)
    {
        transform.parent = null;

        StartCoroutine(SetParentWhenAboveCoroutine(parent));
    }

    /// <summary>
    /// Coroutine that searches a set below PC to make it his parent in hierarchy
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    IEnumerator SetParentWhenAboveCoroutine(Transform parent)
    {
        while (transform.parent == null)
        {
            //Throws a ray to the floor
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hitInfo;

            //If ray hits
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                SetBehavior setBehavior;
                //If it hits with a set and it's the same than the parent (that must be the just spawned set)
                if ((setBehavior = hitInfo.collider.GetComponentInParent<SetBehavior>()) != null && setBehavior.transform == parent)
                {
                    //It becomes PC new parent
                    transform.parent = parent;
                }
            }

            yield return null;
        }
    }

    #endregion

    #endregion
}
