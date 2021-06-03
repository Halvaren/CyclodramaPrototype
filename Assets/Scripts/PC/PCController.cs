using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CharacterLocation
{
    Stage, StageLeftSide, StageRightSide, BehindStage,
    AtrezzoWarehouse, AtrezzoWorkshop,
    Corridor1, DressingRoom1, Bathroom1,
    Corridor2, EmployeeZone, CostumeWorkshop
}

public class PCController : MonoBehaviour
{
    #region Input switches

    [HideInInspector]
    public bool processGameplayInput = true;
    [HideInInspector]
    public bool processMovementInput = true;
    [HideInInspector]
    public bool processPauseInput = true;
    [HideInInspector]
    public bool processInventoryInput = true;
    [HideInInspector]
    public bool processRunningInput = true;

    #endregion

    [HideInInspector]
    public Stack<Action> getBackActionStack;

    public Stack<IEnumerator> verbExecutionCoroutines;

    public GameObject inventoryGO;

    protected SetDoorBehavior lastPointedDoor;

    public bool newScene;
    public CharacterLocation location;

    public PCData pcData;

    public delegate void AnimationCallback();
    public event AnimationCallback mainAnimationCallback;
    public event AnimationCallback secondAnimationCallback;

    #region Components

    public PCMovementController MovementController;

    public PCAnimationController AnimationController;

    public PCActionController ActionController;

    public PCInventoryController InventoryController;
    public List<PickableObjBehavior> objBehaviorsInInventory;

    #endregion

    #region Properties

    private InputManager inputManager;
    public InputManager InputManager
    {
        get
        {
            if (inputManager == null) inputManager = InputManager.instance;
            return inputManager;
        }
    }

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
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

    private Transform gameContainerTransform;
    public Transform GameContainerTransform
    {
        get
        {
            if (gameContainerTransform == null) gameContainerTransform = GameManager.instance.gameContainer.transform;
            return gameContainerTransform;
        }
    }

    #endregion

    public static PCController instance;

    void Awake()
    {
        instance = this;
    }

    public void InitializePC()
    {
        pcData = new PCData(DataManager.Instance.pcData);
        location = pcData.location;
        DataManager.OnSaveData += SavePCData;

        getBackActionStack = new Stack<Action>();
        verbExecutionCoroutines = new Stack<IEnumerator>();

        if (MovementController) MovementController.m_PCController = this;
        if (AnimationController) AnimationController.m_PCController = this;
        if (ActionController) 
        { 
            ActionController.m_PCController = this;
            ActionController.CancelCurrentVerb();
        }
        if (InventoryController)
        {
            InventoryController.m_PCController = this;
            InventoryController.InitializeInventory();
        }

        InputManager.InitializeInput();
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

    public void SetTransitionDone(int setID)
    {
        location = (CharacterLocation)setID;
        transform.parent = GameContainerTransform;
    }

    #endregion

    public void EnableGameplayInput(bool value)
    {
        processGameplayInput = value;

        EnableMovementInput(value);
    }

    public bool IsEnableGameplayInput
    {
        get { return processGameplayInput; }
    }

    public void EnableMovementInput(bool value)
    {
        processMovementInput = value;
        MovementController.ActivateAgent(value);

        if (!value)
        {
            CancelVerbExecution();
        }
    }

    public void EnableInventoryInput(bool value)
    {
        processInventoryInput = value;
    }

    public bool IsEnableInventoryInput
    {
        get { return processInventoryInput; }
    }

    public void EnablePauseInput(bool value)
    {
        processPauseInput = value;
    }

    private void Update()
    {
        if (InventoryInput(InputManager.clicked)) return;
        if (GameplayInput(InputManager.clicked)) return;
    }

    void CancelVerbExecution()
    {
        while(verbExecutionCoroutines.Count > 0)
        {
            StopCoroutine(verbExecutionCoroutines.Peek());
            verbExecutionCoroutines.Pop();
        }
        verbExecutionCoroutines.Clear();
    }

    bool InventoryInput(bool clicked)
    {
        if (processInventoryInput)
        {
            if (InputManager.pressedInventoryKey)
            {
                if (GeneralUIController.displayingInventoryUI)
                {
                    InventoryController.CloseInventory();
                    return false;
                }
                else
                {
                    InventoryController.OpenInventory();
                }
            }
            
            //POINTING OBJECTS IN INVENTORY

            if(GeneralUIController.displayingInventoryUI)
            {
                UseOfVerb currentVerb = ActionController.GetCurrentVerb();

                PointingResult pointingResult = InputManager.pointingResult;
                GameObject pointedGO = InputManager.pointedGO;
                PickableObjBehavior objBehavior = null;
                UseOfVerb pointedGOUseOfVerb = null;

                bool somethingPointed = false;
                if (pointingResult == PointingResult.Object)
                {
                    objBehavior = pointedGO.GetComponent<PickableObjBehavior>();

                    if (objBehavior != null && objBehavior.CheckUseOfVerb(ActionController.GetSelectedVerb(), false))
                    {
                        pointedGOUseOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());
                        CursorManager.instance.ChangeCursorState(CursorState.Highlighted);

                        if(currentVerb != null && currentVerb.multiObj && currentVerb.actuatorObj != objBehavior)
                        {
                            ActionVerbsUIController.SetSecondFocusedObj(objBehavior.GetObjName());
                            somethingPointed = true;
                        }
                        else if(currentVerb == null)
                        {
                            ActionVerbsUIController.SetFirstFocusedObj(objBehavior.GetObjName());
                            ActionVerbsUIController.ResetSecondFocusedObj();
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
                        ActionVerbsUIController.ResetSecondFocusedObj();
                    else
                    {
                        ActionVerbsUIController.ResetFirstFocusedObj();
                        ActionVerbsUIController.ResetSecondFocusedObj();
                    }
                }

                if(currentVerb != null && currentVerb.multiObj)
                {
                    ActionVerbsUIController.SetSelectedVerbInfo(currentVerb.GetVerbInfo(true));
                }
                else if(pointedGOUseOfVerb != null)
                {
                    ActionVerbsUIController.SetSelectedVerbInfo(pointedGOUseOfVerb.GetVerbInfo(false));
                }
                else
                {
                    ActionVerbsUIController.SetSelectedVerbInfo(null);
                }

                //CLICKING OBJECTS IN INVENTORY

                if (clicked && pointingResult != PointingResult.Nothing)
                {
                    if (objBehavior == null) objBehavior = pointedGO.GetComponent<PickableObjBehavior>();

                    if(objBehavior.obj != null)
                    {
                        UseOfVerb useOfVerb;
                        UseOfVerb targetUseOfVerb;
                        if (ActionController.GetCurrentVerb() != null)
                        {
                            targetUseOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());

                            useOfVerb = ActionController.GetCurrentVerb();
                            useOfVerb.targetObj = objBehavior;
                        }
                        else
                        {
                            useOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());
                            targetUseOfVerb = null;
                        }

                        if (useOfVerb != null)
                        {
                            ActionController.SetCurrentVerb(useOfVerb);

                            if (!useOfVerb.multiObj || (useOfVerb.multiObj && useOfVerb.targetObj != null))
                            {
                                IEnumerator executeVerbCoroutine = ActionController.ExecuteVerb(useOfVerb, targetUseOfVerb);

                                StartCoroutine(executeVerbCoroutine);
                                verbExecutionCoroutines.Push(executeVerbCoroutine);
                            }

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
        bool executedBackAction = false;

        UseOfVerb currentVerb = ActionController.GetCurrentVerb();

        PointingResult pointingResult = InputManager.pointingResult;
        Vector3 clickedPoint = InputManager.clickedPoint;
        GameObject pointedGO = InputManager.pointedGO;
        UseOfVerb pointedGOUseOfVerb = null;

        InteractableObjBehavior objBehavior = null;

        if (lastPointedDoor != null && lastPointedDoor.gameObject != pointedGO)
        {
            lastPointedDoor.SetSignBlink(false);
            lastPointedDoor = null;
        }

        if (processGameplayInput)
        {
            bool somethingPointed = false;
            if (pointingResult == PointingResult.Object)
            {
                objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                if (objBehavior != null && objBehavior.CheckUseOfVerb(ActionController.GetSelectedVerb()))
                {
                    pointedGOUseOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());
                    CursorManager.instance.ChangeCursorState(CursorState.Highlighted);

                    if (objBehavior is SetDoorBehavior door)
                    {
                        door.SetSignBlink(true);
                        lastPointedDoor = door;
                    }

                    if (currentVerb != null && currentVerb.multiObj && currentVerb.actuatorObj != objBehavior)
                    {
                        ActionVerbsUIController.SetSecondFocusedObj(objBehavior.GetObjName());
                        somethingPointed = true;
                    }
                    else if (currentVerb == null)
                    {
                        ActionVerbsUIController.SetFirstFocusedObj(objBehavior.GetObjName());
                        ActionVerbsUIController.ResetSecondFocusedObj();
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
                if (currentVerb != null)
                    ActionVerbsUIController.ResetSecondFocusedObj();
                else
                {
                    ActionVerbsUIController.ResetFirstFocusedObj();
                    ActionVerbsUIController.ResetSecondFocusedObj();
                }
            }

            if (currentVerb != null && currentVerb.multiObj)
            {
                ActionVerbsUIController.SetSelectedVerbInfo(currentVerb.GetVerbInfo(true));
            }
            else if (pointedGOUseOfVerb != null)
            {
                ActionVerbsUIController.SetSelectedVerbInfo(pointedGOUseOfVerb.GetVerbInfo(false));
            }
            else
            {
                ActionVerbsUIController.SetSelectedVerbInfo(null);
            }

            if (clicked && pointingResult != PointingResult.Nothing)
            {
                if (pointingResult == PointingResult.Object)
                {
                    if (objBehavior == null) objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                    if (objBehavior.obj != null)
                    {
                        UseOfVerb useOfVerb;
                        UseOfVerb targetUseOfVerb;
                        if(ActionController.GetCurrentVerb() != null)
                        {
                            targetUseOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());

                            useOfVerb = ActionController.GetCurrentVerb();
                            useOfVerb.targetObj = objBehavior;
                        }
                        else
                        {
                            useOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());
                            targetUseOfVerb = null;
                        }

                        if (useOfVerb != null)
                        {
                            ActionController.SetCurrentVerb(useOfVerb);

                            if(!useOfVerb.multiObj || (useOfVerb.multiObj && targetUseOfVerb != null))
                            {
                                IEnumerator executeVerbCoroutine = ActionController.ExecuteVerb(useOfVerb, targetUseOfVerb);

                                StartCoroutine(executeVerbCoroutine);
                                verbExecutionCoroutines.Push(executeVerbCoroutine);
                            }                            

                            return true;
                        }
                    }
                }
            }

            if (InputManager.pressedEscape && getBackActionStack.Count > 0)
            {
                Action getBackAction = getBackActionStack.Peek();

                getBackActionStack.Pop();

                getBackAction();
                executedBackAction = true;
            }
        }

        if (processPauseInput)
        {
            if (InputManager.pressedEscape && !executedBackAction)
            {
                GeneralUIController.ShowPauseUI();
            }
        }

        if (processMovementInput)
        {
            if (clicked && pointingResult != PointingResult.Nothing)
            {
                CancelVerbExecution();
                MovementController.AgentMoveTo(clickedPoint);
            }

            MovementController.MovementUpdate(InputManager.horizontal, InputManager.vertical, InputManager.holdingShift);

            if (InputManager.horizontal != 0f && InputManager.vertical != 0f)
            {
                CancelVerbExecution();
            }
        }
        else
        {
            MovementController.MovementUpdate(0f, 0f, InputManager.holdingShift);
        }

        return false;
    }

    public void MakeInvisible(bool invisible)
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();

        foreach(Renderer render in renders)
        {
            render.enabled = !invisible;
        }
    }

    #region Animation callbacks

    public void ExecuteMainAnimationCallback()
    {
        if(mainAnimationCallback != null)
            mainAnimationCallback();
    }

    public void ExecuteSecondAnimationCallback()
    {
        if (secondAnimationCallback != null)
            secondAnimationCallback();
    }    

    #endregion

    public void SavePCData()
    {
        pcData.newScene = newScene;
        pcData.location = location;
        pcData.position = transform.position;
        DataManager.Instance.pcData = new PCData(pcData);
    }
}
