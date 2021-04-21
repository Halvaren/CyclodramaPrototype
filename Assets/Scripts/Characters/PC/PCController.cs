using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PCController : MonoBehaviour
{
    [HideInInspector]
    public bool processInteractionInput = true;
    [HideInInspector]
    public bool processMovementInput = true;
    [HideInInspector]
    public bool processPauseInput = true;
    [HideInInspector]
    public bool processInventoryInput = true;

    [HideInInspector]
    public Action getBackCallback = null;

    public GameObject inventoryGO;

    #region Components

    public PCMovementController MovementController;

    public PCAnimationController AnimationController;

    public PCActionController ActionController;

    public PCInputController InputController;

    public PCInventoryController InventoryController;

    #endregion

    #region Properties

    public ActionVerbsUIController actionVerbsUIController
    {
        get
        {
            return GeneralUIController.Instance.actionVerbsUIController;
        }
    }

    public DialogueUIController dialogueUIController
    {
        get
        {
            return GeneralUIController.Instance.dialogueUIController;
        }
    }

    public InventoryUIController inventoryUIController
    {
        get
        {
            return GeneralUIController.Instance.inventoryUIController;
        }
    }

    #endregion

    public static PCController Instance;

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (MovementController) MovementController.m_PCController = this;
        if (AnimationController) AnimationController.m_PCController = this;
        if (ActionController) 
        { 
            ActionController.m_PCController = this;
            ActionController.CancelCurrentVerb();
        }
        if (InputController) 
        { 
            InputController.m_PCController = this;
            InputController.InitializeInput();
        }
        if (InventoryController)
        {
            InventoryController.m_PCController = this;
            InventoryController.InitializeInventory();
        }
    }

    #region Movement Intermediary Methods

    bool onWaypoint = false;

    public void PrepareForMovementBetweenSets(bool stopMovementAnim = false)
    {
        EnableGameplayInput(false);
        if (stopMovementAnim) AnimationController.StopMovement();
    }

    public void LinearMovementBetweenSets(Transform nextSet, Transform target, bool xMovement = true, bool yMovement = true, bool zMovement = true)
    {
        MovementController.ControllerMoveTo(target, xMovement, yMovement, zMovement, SetTransitionSystem.instance.SetCharacterMovementDone);
        MovementController.SetParentWhenAbove(nextSet);
    }

    public void MoveToPoint(Transform target)
    {
        MovementController.ControllerMoveTo(target, true, false, true, SetTransitionSystem.instance.SetCharacterMovementDone);
    }

    public void FollowWaypoints(List<Transform> waypoints, Transform nextSet)
    {
        StartCoroutine(FollowWaypointsCoroutine(waypoints));
        MovementController.SetParentWhenAbove(nextSet);
    }

    IEnumerator FollowWaypointsCoroutine(List<Transform> waypoints)
    {
        int currentWaypoint = 0;

        while (currentWaypoint < waypoints.Count)
        {
            onWaypoint = false;

            Debug.Log("Going to " + waypoints[currentWaypoint].name);

            MovementController.ControllerMoveTo(waypoints[currentWaypoint], true, false, true, SetOnWaypoint);

            while (!onWaypoint) yield return null;

            currentWaypoint++;
        }

        SetTransitionSystem.instance.SetCharacterMovementDone();
    }

    void SetOnWaypoint()
    {
        onWaypoint = true;
    }

    public void SetTransitionDone()
    {
        transform.parent = null;
        EnableGameplayInput(true);
    }

    #endregion

    public void EnableGameplayInput(bool value)
    {
        processInteractionInput = value;

        EnableMovementInput(value);
    }

    public void EnableMovementInput(bool value)
    {
        processMovementInput = value;
        MovementController.ActivateAgent(value);

        if (!value)
        {
            MovementController.CancelMoveRotateAndExecute();
            ActionController.CancelCurrentVerb();
        }
    }

    public void EnableInventoryInput(bool value)
    {
        processInventoryInput = value;
    }

    private void Update()
    {
        bool clicked = InputController.InputUpdate();

        InventoryInput();
        GameplayInput(clicked);        
    }

    void InventoryInput()
    {
        if (processInventoryInput)
        {
            if (InputController.openCloseInventory)
            {
                bool open = inventoryUIController.OpenCloseInventory();
                if (!open) return;
            }

            PointingResult pointingResult = InputController.pointingResult;
            GameObject pointedGO = InputController.pointedGO;
            InteractableObjBehavior objBehavior = null;

            bool somethingPointed = true;
            if (pointingResult == PointingResult.Door || pointingResult == PointingResult.Object || pointingResult == PointingResult.Subject)
            {
                objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                if (objBehavior != null && objBehavior._CheckUseOfVerb(ActionController.GetSelectedVerb(), false))
                {
                    CursorManager.instance.ChangeCursorState(CursorState.Highlighted);
                    if (objBehavior is DoorBehavior door)
                    {
                        actionVerbsUIController.SetFocusedObjSubj(door.nextSetName);
                    }
                    else
                    {
                        actionVerbsUIController.SetFocusedObjSubj(objBehavior.obj.name);
                    }
                }
                else
                {
                    CursorManager.instance.ChangeCursorState(CursorState.Disable);
                    somethingPointed = false;
                }
            }
            else
            {
                CursorManager.instance.ChangeCursorState(CursorState.Normal);
                somethingPointed = false;
            }

            if (!somethingPointed)
            {
                actionVerbsUIController.SetFocusedObjSubj("");
            }
        }
    }

    void GameplayInput(bool clicked)
    {
        PointingResult pointingResult = InputController.pointingResult;

        Vector3 clickedPoint = InputController.clickedPoint;
        GameObject pointedGO = InputController.pointedGO;

        InteractableObjBehavior objBehavior = null;       

        if (processInteractionInput)
        {
            bool somethingPointed = true;
            if (pointingResult == PointingResult.Door || pointingResult == PointingResult.Object || pointingResult == PointingResult.Subject)
            {
                objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                if (objBehavior != null && objBehavior._CheckUseOfVerb(ActionController.GetSelectedVerb()))
                {
                    CursorManager.instance.ChangeCursorState(CursorState.Highlighted);
                    if (objBehavior is DoorBehavior door)
                    {
                        actionVerbsUIController.SetFocusedObjSubj(door.nextSetName);
                    }
                    else
                    {
                        actionVerbsUIController.SetFocusedObjSubj(objBehavior.obj.name);
                    }
                }
                else
                {
                    CursorManager.instance.ChangeCursorState(CursorState.Disable);
                    somethingPointed = false;
                }
            }
            else 
            {
                CursorManager.instance.ChangeCursorState(CursorState.Normal);
                somethingPointed = false; 
            }

            if(!somethingPointed)
            {
                actionVerbsUIController.SetFocusedObjSubj("");
            }

            if (clicked && pointingResult != PointingResult.Nothing)
            {
                if (pointingResult == PointingResult.Door || pointingResult == PointingResult.Object || pointingResult == PointingResult.Subject)
                {
                    if (objBehavior == null) objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                    if (objBehavior.obj != null)
                    {
                        UseOfVerb useOfVerb = objBehavior._GetUseOfVerb(ActionController.GetSelectedVerb());

                        if (useOfVerb != null)
                        {
                            ManageUseOfVerb(useOfVerb, objBehavior);
                            return;
                        }
                    }
                }
            }

            if (InputController.EscapeKey)
            {
                if (getBackCallback != null)
                {
                    getBackCallback();
                    getBackCallback = null;
                }
            }
        }

        if (processMovementInput)
        {
            if (clicked && pointingResult != PointingResult.Nothing)
            {
                MovementController.CancelMoveRotateAndExecute();
                ActionController.CancelCurrentVerb();
                MovementController.AgentMoveTo(clickedPoint);
            }

            MovementController.MovementUpdate(InputController.horizontal, InputController.vertical, InputController.running);

            if (InputController.horizontal != 0f && InputController.vertical != 0f)
            {
                MovementController.CancelMoveRotateAndExecute();
                ActionController.CancelCurrentVerb();
            }
        }
        else
        {
            MovementController.MovementUpdate(0f, 0f, false);
        }
    }

    public void ManageUseOfVerb(UseOfVerb useOfVerb, InteractableObjBehavior objBehavior, bool dontMove = false)
    {
        Vector3 pointToMove = transform.position;
        Vector3 direction = objBehavior.transform.position - transform.position;

        if(!dontMove)
        {
            switch (useOfVerb.verbMovement)
            {
                case VerbMovement.MoveAround:
                    pointToMove = objBehavior._GetPointAroundObject(transform.position, useOfVerb.distanceFromObject);
                    break;
                case VerbMovement.MoveToExactPoint:
                    pointToMove = useOfVerb.pointToMove.position;
                    break;
            }
        }
        
        ActionController.SetCurrentVerb(useOfVerb);
        MovementController.MoveRotateAndExecute(pointToMove, direction, ActionController.ExecuteCurrentVerb, dontMove);
    }

    public void MakeInvisible(bool invisible)
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();

        foreach(Renderer render in renders)
        {
            render.enabled = !invisible;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(InputController.clickedPoint, 0.5f);
    }
}
