using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public enum CharacterLocation
{
    Stage, StageLeftSide, StageRightSide, BehindStage,
    AtrezzoWarehouse, AtrezzoWorkshop,
    Corridor1, DressingRoom1, Bathroom1,
    Corridor2, EmployeeZone, CostumeWorkshop
}

public class PCController : MonoBehaviour
{
    #region Input variables

    [HideInInspector]
    Stack<bool> gameplayInputBlocks;
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
    public Light thinkSpotLight;

    protected SetDoorBehavior lastPointedDoor;

    public bool newScene;
    public CharacterLocation location;

    public PCData pcData;

    public SetBehavior currentSet;

    public delegate void AnimationCallback();
    public event AnimationCallback mainAnimationCallback;
    public event AnimationCallback secondAnimationCallback;

    #region Audio variables

    public AudioClip pickClip;
    public AudioClip[] footstepClips;
    public AudioClip swordSlashingClip;
    int footstepClipPointer = 0;

    public AudioClip chairSittingClip;
    public AudioClip chairStandUpClip;
    public AudioClip couchSittingClip;
    public AudioClip couchStandUpClip;

    public AudioClip turnOnLightClip;

    AudioClip sittingClip;
    AudioClip standUpClip;

    #endregion

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

    private SpeakersController speakersController;
    public SpeakersController SpeakersController
    {
        get
        {
            if (speakersController == null) speakersController = SpeakersController.instance;
            return speakersController;
        }
    }

    private AudioManager audioManager;
    public AudioManager AudioManager
    {
        get
        {
            if (audioManager == null) audioManager = AudioManager.instance;
            return audioManager;
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

        gameplayInputBlocks = new Stack<bool>();

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
        transform.parent = null;
    }

    #endregion

    public void EnableGameplayInput(bool value, bool cancelVerbExecution = true)
    {
        if (value)
        {
            if (gameplayInputBlocks.Count > 0) gameplayInputBlocks.Pop();
        }
        else gameplayInputBlocks.Push(true);

        Debug.Log((value ? "Removed" : "Added") + " gameplay block. Current count: " + gameplayInputBlocks.Count);

        EnableMovementInput(CameraManager.usingMainCamera && value, cancelVerbExecution);
    }

    public bool IsEnableGameplayInput
    {
        get { return gameplayInputBlocks.Count == 0; }
    }

    public void EnableMovementInput(bool value, bool cancelVerbExecution = true)
    {
        AnimationController.StopMovement();
        processMovementInput = value;
        MovementController.ActivateAgent(value);

        if (!value && cancelVerbExecution)
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

        ActionController.CancelCurrentVerb();
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
                        else if(currentVerb == null || (currentVerb != null && !currentVerb.multiObj))
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

        if (IsEnableGameplayInput)
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
                    else if (currentVerb == null || (currentVerb != null && !currentVerb.multiObj))
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

                    if(currentVerb != null && !currentVerb.multiObj)
                    {
                        if (objBehavior == currentVerb.actuatorObj) return true;
                        if (objBehavior != currentVerb.actuatorObj) CancelVerbExecution();
                    }
                    
                    if (objBehavior.obj != null)
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

                            if (!useOfVerb.multiObj || (useOfVerb.multiObj && targetUseOfVerb != null))
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

            if (InputManager.horizontal != 0f || InputManager.vertical != 0f)
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

    public void TurnOnOffThinkLight(bool on)
    {
        thinkSpotLight.enabled = on;
        if(on)
        {
            AudioManager.PlaySound(turnOnLightClip, SoundType.MetaTheater);
        }
    }

    #region Play sounds methods

    public void PlayPickSound()
    {
        Debug.Log("hola");
        SpeakersController.PlaySoundOnSpeakers(pickClip);
    }

    public void PlayFootstepSound()
    {
        AudioManager.PlaySound(footstepClips[footstepClipPointer++], SoundType.Footstep);

        if (footstepClipPointer >= footstepClips.Length) footstepClipPointer = 0;
    }

    public void SetSittingSound(SeatType type)
    {
        switch(type)
        {
            case SeatType.Chair:
                sittingClip = chairSittingClip;
                break;
            case SeatType.Couch:
                sittingClip = couchSittingClip;
                break;
        }
    }

    public void SetStandUpSound(SeatType type)
    {
        switch (type)
        {
            case SeatType.Chair:
                standUpClip = chairStandUpClip;
                break;
            case SeatType.Couch:
                standUpClip = couchStandUpClip;
                break;
        }
    }

    public void PlaySittingSound()
    {
        if (sittingClip != null) AudioManager.PlaySound(sittingClip, SoundType.Character);
    }

    public void PlayStandUpSound()
    {
        if (standUpClip != null) AudioManager.PlaySound(standUpClip, SoundType.Character);
    }

    public void PlaySwordSlashingSound()
    {
        AudioManager.PlaySound(swordSlashingClip, SoundType.Character);
    }

    #endregion

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

    public void AddGetBackAction(Action getBackAction)
    {
        getBackActionStack.Push(getBackAction);
    }

    public void RemoveGetBackAction()
    {
        getBackActionStack.Pop();
    }

    public void SavePCData()
    {
        pcData.newScene = newScene;
        pcData.location = location;
        pcData.position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        DataManager.Instance.pcData = new PCData(pcData);
    }
}
