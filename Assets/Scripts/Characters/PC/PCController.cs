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

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.Instance;
            return generalUIController;
        }
    }

    private ActionVerbsUIController actionVerbsUIController;
    public ActionVerbsUIController ActionVerbsUIController
    {
        get
        {
            if (actionVerbsUIController == null) actionVerbsUIController = GeneralUIController.actionVerbsUIController;
            return actionVerbsUIController;
        }
    }

    private DialogueUIController dialogueUIController;
    public DialogueUIController DialogueUIController
    {
        get
        {
            if (dialogueUIController == null) dialogueUIController = GeneralUIController.dialogueUIController;
            return dialogueUIController;
        }
    }

    private InventoryUIController inventoryUIController;
    public InventoryUIController InventoryUIController
    {
        get
        {
            if (inventoryUIController == null) inventoryUIController = GeneralUIController.inventoryUIController;
            return inventoryUIController;
        }
    }

    private CameraManager cameraManager;
    public CameraManager CameraManager
    {
        get
        {
            if (cameraManager == null) cameraManager = CameraManager.instance;
            return cameraManager;
        }
    }

    #endregion

    public static PCController Instance;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (MovementController) MovementController.m_PCController = this;
        if (AnimationController) AnimationController.m_PCController = this;
        if (ActionController) 
        { 
            ActionController.m_PCController = this;
            ActionController.CancelCurrentVerb();
            ActionController.CancelVerbInExecution();
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
        EnableInventoryInput(false);
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
        EnableInventoryInput(true);
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
            ActionController.CancelVerbInExecution();
        }
    }

    public void EnableInventoryInput(bool value)
    {
        processInventoryInput = value;
    }

    private void Update()
    {
        bool clicked = InputController.InputUpdate();

        if (InventoryInput(clicked)) return;
        if (GameplayInput(clicked)) return;
    }

    bool InventoryInput(bool clicked)
    {
        if (processInventoryInput)
        {
            if (InputController.openCloseInventory)
            {
                if (InventoryUIController.showingInventory)
                {
                    InventoryController.CloseInventory();
                    return false;
                }
                else
                {
                    InventoryController.OpenInventory();
                }
            }
            
            if(InventoryUIController.showingInventory)
            {
                PointingResult pointingResult = InputController.pointingResult;
                GameObject pointedGO = InputController.pointedGO;
                PickableObjBehavior objBehavior = null;

                bool somethingPointed = false;
                if (pointingResult == PointingResult.Object)
                {
                    objBehavior = pointedGO.GetComponent<PickableObjBehavior>();

                    if (objBehavior != null && objBehavior._CheckUseOfVerb(ActionController.GetSelectedVerb(), false))
                    {
                        CursorManager.instance.ChangeCursorState(CursorState.Highlighted);

                        UseOfVerb currentVerb;
                        if((currentVerb = ActionController.GetCurrentVerb()) != null && currentVerb.multiObj && currentVerb.actuatorObj != objBehavior)
                        {
                            ActionVerbsUIController.SetSecondFocusedObj(objBehavior.obj.name);
                            somethingPointed = true;
                        }
                        else if(currentVerb == null)
                        {
                            ActionVerbsUIController.SetFirstFocusedObj(objBehavior.obj.name);
                            ActionVerbsUIController.SetSecondFocusedObj("");
                            somethingPointed = true;
                        }
                        
                    }
                    else
                    {
                        CursorManager.instance.ChangeCursorState(CursorState.Disable);
                    }
                }
                else
                {
                    CursorManager.instance.ChangeCursorState(CursorState.Normal);
                }

                if (!somethingPointed)
                {
                    if (ActionController.GetCurrentVerb() != null)
                        ActionVerbsUIController.SetSecondFocusedObj("");
                    else
                    {
                        ActionVerbsUIController.SetFirstFocusedObj("");
                        ActionVerbsUIController.SetSecondFocusedObj("");
                    }
                }

                if (clicked && pointingResult != PointingResult.Nothing)
                {
                    if (objBehavior == null) objBehavior = pointedGO.GetComponent<PickableObjBehavior>();

                    if(objBehavior.obj != null)
                    {
                        UseOfVerb useOfVerb = null;
                        if (ActionController.GetCurrentVerb() != null)
                        {
                            UseOfVerb targetUseOfVerb = objBehavior._GetUseOfVerb(ActionController.GetSelectedVerb());

                            useOfVerb = ActionController.GetCurrentVerb();
                            useOfVerb.targetObj = objBehavior;
                        }
                        else
                        {
                            useOfVerb = objBehavior._GetUseOfVerb(ActionController.GetSelectedVerb());
                        }

                        if (useOfVerb != null)
                        {
                            ActionController.SetCurrentVerb(useOfVerb);

                            if (!useOfVerb.multiObj || (useOfVerb.multiObj && useOfVerb.targetObj != null))
                                ManageUseOfVerb(useOfVerb, objBehavior, true);

                            InventoryController.CloseInventory();
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    bool GameplayInput(bool clicked)
    {
        PointingResult pointingResult = InputController.pointingResult;

        Vector3 clickedPoint = InputController.clickedPoint;
        GameObject pointedGO = InputController.pointedGO;

        InteractableObjBehavior objBehavior = null;       

        if (processInteractionInput)
        {
            bool somethingPointed = false;
            if (pointingResult == PointingResult.Door || pointingResult == PointingResult.Object || pointingResult == PointingResult.Subject)
            {
                objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                if (objBehavior != null && objBehavior._CheckUseOfVerb(ActionController.GetSelectedVerb()))
                {
                    CursorManager.instance.ChangeCursorState(CursorState.Highlighted);
                    if (objBehavior is DoorBehavior door)
                    {
                        UseOfVerb currentVerb;
                        if((currentVerb = ActionController.GetCurrentVerb()) != null && currentVerb.multiObj && currentVerb.actuatorObj != objBehavior)
                        {
                            ActionVerbsUIController.SetSecondFocusedObj(door.nextSetName);
                            somethingPointed = true;
                        }
                        else if(currentVerb == null)
                        {
                            ActionVerbsUIController.SetFirstFocusedObj(door.nextSetName);
                            ActionVerbsUIController.SetSecondFocusedObj("");
                            somethingPointed = true;
                        }
                    }
                    else
                    {
                        UseOfVerb currentVerb;
                        if ((currentVerb = ActionController.GetCurrentVerb()) != null && currentVerb.multiObj && currentVerb.actuatorObj != objBehavior)
                        {
                            ActionVerbsUIController.SetSecondFocusedObj(objBehavior.obj.name);
                            somethingPointed = true;
                        }
                        else if (currentVerb == null)
                        {
                            ActionVerbsUIController.SetFirstFocusedObj(objBehavior.obj.name);
                            ActionVerbsUIController.SetSecondFocusedObj("");
                            somethingPointed = true;
                        }
                    }
                }
                else
                {
                    CursorManager.instance.ChangeCursorState(CursorState.Disable);
                }
            }
            else 
            {
                CursorManager.instance.ChangeCursorState(CursorState.Normal);
            }

            if(!somethingPointed)
            {
                if (ActionController.GetCurrentVerb() != null)
                    ActionVerbsUIController.SetSecondFocusedObj("");
                else
                {
                    ActionVerbsUIController.SetFirstFocusedObj("");
                    ActionVerbsUIController.SetSecondFocusedObj("");
                }
            }
            

            if (clicked && pointingResult != PointingResult.Nothing)
            {
                if (pointingResult == PointingResult.Door || pointingResult == PointingResult.Object || pointingResult == PointingResult.Subject)
                {
                    if (objBehavior == null) objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                    if (objBehavior.obj != null)
                    {
                        UseOfVerb useOfVerb;
                        if(ActionController.GetCurrentVerb() != null)
                        {
                            UseOfVerb targetUseOfVerb = objBehavior._GetUseOfVerb(ActionController.GetSelectedVerb());

                            useOfVerb = ActionController.GetCurrentVerb();
                            useOfVerb.targetObj = objBehavior;

                            useOfVerb.verbMovement = targetUseOfVerb.verbMovement;
                            switch(useOfVerb.verbMovement)
                            {
                                case VerbMovement.MoveAround:
                                    useOfVerb.distanceFromObject = targetUseOfVerb.distanceFromObject;
                                    break;
                                case VerbMovement.MoveToExactPoint:
                                    useOfVerb.pointToMove = targetUseOfVerb.pointToMove;
                                    break;
                            }
                        }
                        else
                        {
                            useOfVerb = objBehavior._GetUseOfVerb(ActionController.GetSelectedVerb());
                        }

                        if (useOfVerb != null)
                        {
                            ActionController.SetCurrentVerb(useOfVerb);

                            if(!useOfVerb.multiObj || (useOfVerb.multiObj && useOfVerb.targetObj != null))
                                ManageUseOfVerb(useOfVerb, objBehavior);

                            return true;
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
                ActionController.CancelVerbInExecution();
                MovementController.AgentMoveTo(clickedPoint);
            }

            MovementController.MovementUpdate(InputController.horizontal, InputController.vertical, InputController.running);

            if (InputController.horizontal != 0f && InputController.vertical != 0f)
            {
                MovementController.CancelMoveRotateAndExecute();
                ActionController.CancelVerbInExecution();
            }
        }
        else
        {
            MovementController.MovementUpdate(0f, 0f, false);
        }

        return false;
    }

    public void ManageUseOfVerb(UseOfVerb useOfVerb, InteractableObjBehavior objBehavior, bool dontMove = false)
    {
        Vector3 pointToMove = transform.position;
        Vector3 pointToLook = objBehavior.transform.position;

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

        ActionController.SetVerbInExecution(useOfVerb);
        MovementController.MoveRotateAndExecute(pointToMove, pointToLook, ActionController.ExecuteCurrentVerb, dontMove);
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
