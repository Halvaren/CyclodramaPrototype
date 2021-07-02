using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

/// <summary>
/// Possible set locations in demo
/// </summary>
public enum SetLocation
{
    Stage, StageLeftSide, StageRightSide, BehindStage,
    AtrezzoWarehouse, AtrezzoWorkshop,
    Corridor1, DressingRoom1, Bathroom1,
    Corridor2, EmployeeZone, CostumeWorkshop
}

/// <summary>
/// Manages PC behavior
/// </summary>
public class PCController : MonoBehaviour
{
    #region Variables

    public PCData pcData;
    public bool newScene;
    public SetLocation location;
    public SetBehavior currentSet;

    public GameObject inventoryGO;
    public Light thinkSpotLight;

    protected SetDoorBehavior lastPointedDoor;

    //Stacks actions to execute when player presses Esc key. For example, if player opens Pause menu and then Data menu, first it will push an action for exiting 
    //Pause menu and then an action for exiting Data menu. When player presses Esc once, the action for exiting Data menu will pop up from the stack and be executed.
    //If player presses Esc again, the action for exiting Pause menu will pop up from the stack and be executed
    public Stack<Action> getBackActionStack;

    //Stacks the different coroutines that are being executed before verb reaction starts. That is, movement to the interaction point and rotation to look at the
    //interacted object. It is done to be able to stop them in case that action is interrumpted
    public Stack<IEnumerator> verbExecutionCoroutines;

    //These events are used when an animation has a key trigger that executes methods ExecuteMainAnimationCallback or ExecuteSecondAnimationCallback. Before playing
    //the animation, a method or methods are added to one of the events, and when animation reaches its key trigger, it will execute the methods subscribed to the 
    //corresponding event. That allows to make generic key triggers and adapt them according to the situation where the animation is played
    public delegate void AnimationCallback();
    public event AnimationCallback mainAnimationCallback;
    public event AnimationCallback secondAnimationCallback;

    #region Input variables

    //Since there are many situations that block the gameplay input that can occur at the same time (a cutscene starts, blocking the input, and during it, player 
    //opens pause menu, blocking the input again), a single boolean variable is not enough because when the top input-blocking situation is finished, the gameplay
    //input would be enable though the second input-blocking situation is not finished. That's why a stack is used
    Stack<bool> gameplayInputBlocks;
    public bool processMovementInput = true;
    public bool processPauseInput = true;
    public bool processInventoryInput = true;
    public bool processRunningInput = true;

    #endregion

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

    #endregion

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Initializes PC behavior: loads PCData, initializes stacks and PCComponents
    /// </summary>
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

    #region PC loop methods

    private void Update()
    {
        if (InventoryInput(InputManager.clicked)) return;
        if (GameplayInput(InputManager.clicked)) return;
    }

    /// <summary>
    /// Manages PC loop when inventoryInput is true
    /// </summary>
    /// <param name="clicked"></param>
    /// <returns></returns>
    bool InventoryInput(bool clicked)
    {
        if (processInventoryInput)
        {
            if (InputManager.pressedInventoryKey)
            {
                if (GeneralUIController.displayingInventoryUI)
                {
                    InventoryController.CloseInventory();
                    //If inventory gets closed, stops this method and will execute GameplayInput method
                    return false;
                }
                else
                {
                    InventoryController.OpenInventory();
                }
            }
            
            //If inventory is opened
            if(GeneralUIController.displayingInventoryUI)
            {
                UseOfVerb currentVerb = ActionController.GetCurrentVerb();

                PointingResult pointingResult = InputManager.pointingResult;
                GameObject pointedGO = InputManager.pointedGO;
                PickableObjBehavior objBehavior = null;
                UseOfVerb pointedGOUseOfVerb = null;

                //Checks if any inventory item is pointed. It will change cursor state and the object names to be displayed in action bar verb info
                bool somethingPointed = false;
                if (pointingResult == PointingResult.Object)
                {
                    objBehavior = pointedGO.GetComponent<PickableObjBehavior>();

                    //Checks if the selected verb has a corresponding reaction in the pointed item behavior
                    if (objBehavior != null && objBehavior.CheckUseOfVerb(ActionController.GetSelectedVerb(), false))
                    {
                        pointedGOUseOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());
                        CursorManager.instance.ChangeCursorState(CursorState.Highlighted);

                        //If the selected verb is multi-object and player has already clicked the actuator obj and it is not pointing to this actuator obj again
                        if(currentVerb != null && currentVerb.multiObj && currentVerb.actuatorObj != objBehavior)
                        {
                            ActionVerbsUIController.SetSecondFocusedObj(objBehavior.GetObjName());
                            somethingPointed = true;
                        }
                        //If the selected verb is not multi-object
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

                //If nothing is being pointed, it resets the object names to be displayed in action bar verb info
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

                //Updates the verb to be displayed in action bar verb info
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

                //If player has clicked and is pointing something
                if (clicked && pointingResult != PointingResult.Nothing)
                {
                    if (objBehavior == null) objBehavior = pointedGO.GetComponent<PickableObjBehavior>();

                    if(objBehavior.obj != null)
                    {
                        UseOfVerb useOfVerb;
                        UseOfVerb targetUseOfVerb;
                        //If the selected verb is multi-object and player has already clicked the actuatorObj
                        if (ActionController.GetCurrentVerb() != null)
                        {
                            //It gets the UseOfVerb of the corresponding selected verb from the targetObj (the one that player has clicked this time)
                            targetUseOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());

                            //Gets the main UseOfVerb, which has been saved when player clicked the actuatorObj
                            useOfVerb = ActionController.GetCurrentVerb();
                            //Assigns as the main UseOfVerb targetObj the new clicked object
                            useOfVerb.targetObj = objBehavior;
                        }
                        //If the selected verb is not multi-object or it is but player has not clicked any object yet
                        else
                        {
                            //Gets the main UseOfVerb of the corresponding selected verb from the actuatorObj clicked
                            useOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());
                            targetUseOfVerb = null;
                        }

                        //If there is a main UseOfVerb
                        if (useOfVerb != null)
                        {
                            //Stores the main UseOfVerb
                            ActionController.SetCurrentVerb(useOfVerb);

                            //If it is multi-object and there is already a target obj or it is not a multi-object
                            if (!useOfVerb.multiObj || (useOfVerb.multiObj && useOfVerb.targetObj != null))
                            {
                                //Executes the verb action
                                IEnumerator executeVerbCoroutine = ActionController.ExecuteVerb(useOfVerb, targetUseOfVerb);

                                StartCoroutine(executeVerbCoroutine);
                                verbExecutionCoroutines.Push(executeVerbCoroutine);
                            }

                            //It closes the inventory where it executes the verb action or not. In case of a multi-object verb, player wants to use a inventory item
                            //and, when inventory closes after clicking on that item, they will have the opportunity of using it with a gameworld object. If they
                            //want to use it with another inventory item, they just have to open the inventory again and click the targetObj
                            InventoryController.CloseInventory();
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Manages PC loop when gameplayInput or movementInput are true
    /// </summary>
    /// <param name="clicked"></param>
    /// <returns></returns>
    bool GameplayInput(bool clicked)
    {
        bool executedBackAction = false;

        UseOfVerb currentVerb = ActionController.GetCurrentVerb();

        PointingResult pointingResult = InputManager.pointingResult;
        Vector3 clickedPoint = InputManager.clickedPoint;
        GameObject pointedGO = InputManager.pointedGO;
        UseOfVerb pointedGOUseOfVerb = null;

        InteractableObjBehavior objBehavior = null;

        //Resets last door sign blinking
        if (lastPointedDoor != null && lastPointedDoor.gameObject != pointedGO)
        {
            lastPointedDoor.SetSignBlink(false);
            lastPointedDoor = null;
        }

        //If there isn't any gameplay input block
        if (IsEnableGameplayInput)
        {
            //Checks if any object is pointed. It will change cursor state and the object names to be displayed in action bar verb info
            bool somethingPointed = false;
            if (pointingResult == PointingResult.Object)
            {
                objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                //Checks if the selected verb has a corresponding reaction in the pointed object behavior
                if (objBehavior != null && objBehavior.CheckUseOfVerb(ActionController.GetSelectedVerb()))
                {
                    pointedGOUseOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());
                    CursorManager.instance.ChangeCursorState(CursorState.Highlighted);

                    //In case it is a door
                    if (objBehavior is SetDoorBehavior door)
                    {
                        //Starts blinking its sign
                        door.SetSignBlink(true);
                        lastPointedDoor = door;
                    }

                    //If the selected verb is multi-object and player has already clicked the actuator obj and it is not pointing to this actuator obj again
                    if (currentVerb != null && currentVerb.multiObj && currentVerb.actuatorObj != objBehavior)
                    {
                        ActionVerbsUIController.SetSecondFocusedObj(objBehavior.GetObjName());
                        somethingPointed = true;
                    }
                    //If the selected verb is not multi-object
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

            //If nothing is being pointed, it resets the object names to be displayed in action bar verb info
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

            //Updates the verb to be displayed in action bar verb info
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

            //If player has clicked and is pointing something
            if (clicked && pointingResult != PointingResult.Nothing)
            {
                if (pointingResult == PointingResult.Object)
                {
                    if (objBehavior == null) objBehavior = pointedGO.GetComponent<InteractableObjBehavior>();

                    //If player has already clicked an object and the corresponding UseOfVerb is not multi-object (PC is executing an action)
                    if (currentVerb != null && !currentVerb.multiObj)
                    {
                        //If player is clicking the actuatorObj of the executing action, does nothing
                        if (objBehavior == currentVerb.actuatorObj) return true;
                        //If player is clicking other object, cancels the executing action
                        else CancelVerbExecution();
                    }
                    
                    if (objBehavior.obj != null)
                    {
                        UseOfVerb useOfVerb;
                        UseOfVerb targetUseOfVerb;

                        //If the selected verb is multi-object and player has already clicked the actuatorObj
                        if (ActionController.GetCurrentVerb() != null)
                        {
                            //It gets the UseOfVerb of the corresponding selected verb from the targetObj (the one that player has clicked this time)
                            targetUseOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());

                            //Gets the main UseOfVerb, which has been saved when player clicked the actuatorObj
                            useOfVerb = ActionController.GetCurrentVerb();
                            //Assigns as the main UseOfVerb targetObj the new clicked object
                            useOfVerb.targetObj = objBehavior;
                        }
                        //If the selected verb is not multi-object or it is but player has not clicked any object yet
                        else
                        {
                            //Gets the main UseOfVerb of the corresponding selected verb from the actuatorObj clicked
                            useOfVerb = objBehavior.GetUseOfVerb(ActionController.GetSelectedVerb());
                            targetUseOfVerb = null;
                        }

                        //If there is a main UseOfVerb
                        if (useOfVerb != null)
                        {
                            //Stores the main UseOfVerb
                            ActionController.SetCurrentVerb(useOfVerb);

                            //If it is multi-object and there is already a target obj or it is not a multi-object
                            if (!useOfVerb.multiObj || (useOfVerb.multiObj && targetUseOfVerb != null))
                            {
                                //Executes the verb action
                                IEnumerator executeVerbCoroutine = ActionController.ExecuteVerb(useOfVerb, targetUseOfVerb);

                                StartCoroutine(executeVerbCoroutine);
                                verbExecutionCoroutines.Push(executeVerbCoroutine);
                            }

                            return true;
                        }
                    }
                }
            }

            //If player has pressed Esc key and there's any action in the stack
            if (InputManager.pressedEscape && getBackActionStack.Count > 0)
            {
                //Executes the action
                Action getBackAction = getBackActionStack.Peek();

                getBackAction();
                executedBackAction = true;
            }
        }

        if (processPauseInput)
        {
            //If player has pressed Esc key and it hasn't executed before a back action (that means back action stack was empty)
            if (InputManager.pressedEscape && !executedBackAction)
            {
                //Shows Pause menu
                GeneralUIController.ShowPauseUI();
            }
        }

        if (processMovementInput)
        {
            //If player has clicked on nothing (floor, a wall...)
            if (clicked && pointingResult != PointingResult.Nothing)
            {
                //Cancels the current verb in execution and moves the PC to the clickedPoint
                CancelVerbExecution();
                MovementController.AgentMoveTo(clickedPoint);
            }

            //Transfer to the MovementController the key movement inputs
            MovementController.MovementUpdate(InputManager.horizontal, InputManager.vertical, InputManager.holdingShift);

            //If there's movement from key movement inputs
            if (InputManager.horizontal != 0f || InputManager.vertical != 0f)
            {
                //Cancels the current verb in execution
                CancelVerbExecution();
            }
        }
        else
        {
            //If player can't move PC, still calls MovementController update method because that method makes the PC fall because of gravity when it is moved with
            //CharacterController, for example
            MovementController.MovementUpdate(0f, 0f, InputManager.holdingShift);
        }

        return false;
    }

    /// <summary>
    /// When verb execution is interrupted, manages that everything that need to get reset, get reset
    /// </summary>
    void CancelVerbExecution()
    {
        while (verbExecutionCoroutines.Count > 0)
        {
            //Stops all coroutines in the stack and clears the stack
            StopCoroutine(verbExecutionCoroutines.Peek());
            verbExecutionCoroutines.Pop();
        }
        verbExecutionCoroutines.Clear();

        ActionController.CancelCurrentVerb();
    }

    #endregion

    #region Input methods

    /// <summary>
    /// Adds or removes a gameplay input block
    /// </summary>
    /// <param name="value"></param>
    /// <param name="cancelVerbExecution">There are cases where the current verb in execution must not be cancelled</param>
    public void EnableGameplayInput(bool value, bool cancelVerbExecution = true)
    {
        if (value)
        {
            if (gameplayInputBlocks.Count > 0) gameplayInputBlocks.Pop();
        }
        else gameplayInputBlocks.Push(true);

        Debug.Log((value ? "Removed" : "Added") + " gameplay block. Current count: " + gameplayInputBlocks.Count);

        //If gameplay input is enable and there's no detail camera active, it will enable movement input
        EnableMovementInput(CameraManager.usingMainCamera && IsEnableGameplayInput, cancelVerbExecution);
    }

    /// <summary>
    /// Returns if gameplay input is enable
    /// </summary>
    public bool IsEnableGameplayInput
    {
        get { return gameplayInputBlocks.Count == 0; }
    }

    /// <summary>
    /// Enables or disables movement input
    /// </summary>
    /// <param name="value"></param>
    /// <param name="cancelVerbExecution">There are cases where the current verb in execution must not be cancelled</param>
    public void EnableMovementInput(bool value, bool cancelVerbExecution = true)
    {
        AnimationController.StopMovement();
        processMovementInput = value;
        //If movement input is enable, then the NavMeshAgent must be activated
        MovementController.ActivateAgent(value);

        if (!value && cancelVerbExecution)
        {
            CancelVerbExecution();
        }
    }

    /// <summary>
    /// Enables or disables inventory input
    /// </summary>
    /// <param name="value"></param>
    public void EnableInventoryInput(bool value)
    {
        processInventoryInput = value;
    }

    /// <summary>
    /// Returns if inventory input is enable
    /// </summary>
    public bool IsEnableInventoryInput
    {
        get { return processInventoryInput; }
    }

    /// <summary>
    /// Enables or disables pause input
    /// </summary>
    /// <param name="value"></param>
    public void EnablePauseInput(bool value)
    {
        processPauseInput = value;
    }

    #endregion

    #region Movement Intermediary Methods

    bool onWaypoint = false;

    /// <summary>
    /// It is called before a set transition is performed. It disables gameplay and inventory input
    /// </summary>
    /// <param name="stopMovementAnim"></param>
    public void PrepareForMovementBetweenSets(bool stopMovementAnim = false)
    {
        EnableGameplayInput(false);
        EnableInventoryInput(false);
        if (stopMovementAnim) AnimationController.StopMovement();
    }

    /// <summary>
    /// Makes MovementController to move linearly the PC between two sets transitioning, using CharacterController
    /// </summary>
    /// <param name="nextSet"></param>
    /// <param name="target"></param>
    /// <param name="xMovement"></param>
    /// <param name="yMovement"></param>
    /// <param name="zMovement"></param>
    public void LinearMovementBetweenSets(Transform nextSet, Transform target, bool xMovement = true, bool yMovement = true, bool zMovement = true)
    {
        MovementController.ControllerMoveTo(target, xMovement, yMovement, zMovement, SetTransitionSystem.instance.SetCharacterMovementDone);
        MovementController.SetParentWhenAbove(nextSet);
    }

    /// <summary>
    /// Makes MovementController to move PC to a specific point using CharacterController
    /// </summary>
    /// <param name="target"></param>
    public void MoveToPoint(Transform target)
    {
        MovementController.ControllerMoveTo(target, true, false, true, SetTransitionSystem.instance.SetCharacterMovementDone);
    }

    /// <summary>
    /// Makes MovementController to move PC following a list of waypoints, using CharacterController
    /// </summary>
    /// <param name="waypoints"></param>
    /// <param name="nextSet"></param>
    public void FollowWaypoints(List<Transform> waypoints, Transform nextSet)
    {
        StartCoroutine(FollowWaypointsCoroutine(waypoints));
        MovementController.SetParentWhenAbove(nextSet);
    }

    /// <summary>
    /// Coroutine that makes MovementController to move PC following a list of waypoints, using CharacterController
    /// </summary>
    /// <param name="waypoints"></param>
    /// <returns></returns>
    IEnumerator FollowWaypointsCoroutine(List<Transform> waypoints)
    {
        int currentWaypoint = 0;

        while (currentWaypoint < waypoints.Count)
        {
            onWaypoint = false;

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

    /// <summary>
    /// It is executed when the set transition finishes
    /// </summary>
    /// <param name="setID"></param>
    public void SetTransitionDone(int setID)
    {
        location = (SetLocation)setID;
        transform.parent = null;
    }

    #endregion

    #region Play sounds methods

    public void PlayPickSound()
    {
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

    /// <summary>
    /// Generic method that executes the methods subscribed to the mainAnimationCallback event. It is called from a key trigger of an animation
    /// </summary>
    public void ExecuteMainAnimationCallback()
    {
        if(mainAnimationCallback != null)
            mainAnimationCallback();
    }

    /// <summary>
    /// Secondary generic method that executes the methods subscribed to the mainAnimationCallback event. It is called from a key trigger of an animation
    /// </summary>
    public void ExecuteSecondAnimationCallback()
    {
        if (secondAnimationCallback != null)
            secondAnimationCallback();
    }

    #endregion

    #region Other methods

    /// <summary>
    /// Adds an action to the getBackActionStack
    /// </summary>
    /// <param name="getBackAction"></param>
    public void AddGetBackAction(Action getBackAction)
    {
        getBackActionStack.Push(getBackAction);
    }

    /// <summary>
    /// Removes an action from the getBackActionStack
    /// </summary>
    public void RemoveGetBackAction()
    {
        getBackActionStack.Pop();
    }

    /// <summary>
    /// Saves in PCData the PC information and sets it to the DataManager reference
    /// </summary>
    public void SavePCData()
    {
        pcData.newScene = newScene;
        pcData.location = location;
        pcData.position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        DataManager.Instance.pcData = new PCData(pcData);
    }

    /// <summary>
    /// Makes the renderer components in PC hierarchy invisible
    /// </summary>
    /// <param name="invisible"></param>
    public void MakeInvisible(bool invisible)
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();

        foreach (Renderer render in renders)
        {
            render.enabled = !invisible;
        }
    }

    /// <summary>
    /// Turns on or off the local spot light when PC starts or ends Think action
    /// </summary>
    /// <param name="on"></param>
    public void TurnOnOffThinkLight(bool on)
    {
        thinkSpotLight.enabled = on;
        if (on)
        {
            AudioManager.PlaySound(turnOnLightClip, SoundType.MetaTheater);
        }
    }

    #endregion
}
