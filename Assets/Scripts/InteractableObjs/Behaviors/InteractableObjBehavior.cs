using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using VIDE_Data;

/// <summary>
/// Manages the behavior of any interactable object in the gameworld or PC's inventory
/// </summary>
public class InteractableObjBehavior : MonoBehaviour
{
    #region Variables

    //These events are used when an animation has a key trigger that executes methods ExecuteAnimationCallback or ExecuteSecondAnimationCallback. Before playing
    //the animation, a method or methods are added to one of the events, and when animation reaches its key trigger, it will execute the methods subscribed to the 
    //corresponding event. That allows to make generic key triggers and adapt them according to the situation where the animation is played
    public delegate void AnimationCallback();
    public event AnimationCallback mainAnimationCallback;
    public event AnimationCallback secondAnimationCallback;

    protected Stack<bool> animationLocks = new Stack<bool>();

    [SerializeField, HideInInspector]
    protected SerializableMethodInfo[] methods;
    public SerializableMethodInfo[] Methods
    {
        get
        {
            if (methods == null)
            {
                UpdateMethodInfo();
            }
            return methods;
        }
    }

    [SerializeField, HideInInspector]
    protected string[] methodNames;
    public string[] MethodNames
    {
        get
        {
            if (methodNames == null)
            {
                UpdateMethodNames();
            }

            return methodNames;
        }
    }

    [HideInInspector]
    public InteractableObj obj;

    [HideInInspector]
    public List<UseOfVerb> useOfVerbs;

    [HideInInspector]
    public bool inScene = true;

    [HideInInspector]
    public Collider triggerCollider;
    public Collider TriggerCollider
    {
        get
        {
            if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
            return triggerCollider;
        }
    }

    [HideInInspector]
    public Collider obstacleCollider;

    [HideInInspector]
    public Transform interactionPoint;
    [HideInInspector]
    public Transform lookAtPoint;

    private DialogueUIController dialogueUIController;
    public DialogueUIController DialogueUIController
    {
        get
        {
            if (dialogueUIController == null) dialogueUIController = GeneralUIController.instance.dialogueUIController;
            return dialogueUIController;
        }
    }

    private PCController pcController;
    public PCController PCController
    {
        get
        {
            if (pcController == null) pcController = PCController.instance;
            return pcController;
        }
    }

    protected Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    protected AudioManager audioManager;
    public AudioManager AudioManager
    {
        get
        {
            if (audioManager == null) audioManager = AudioManager.instance;
            return audioManager;
        }
    }

    protected CameraManager cameraManager;
    public CameraManager CameraManager
    {
        get
        {
            if (cameraManager == null) cameraManager = CameraManager.instance;
            return cameraManager;
        }
    }

    [HideInInspector]
    public InteractableObjBehavior copyVerbsFromBehavior;

    [HideInInspector]
    public GameObject currentSet;

    [HideInInspector]
    public PickAnimationWeight objWeight;
    [HideInInspector]
    public PickAnimationHeight objHeight;
    [HideInInspector]
    public bool characterVisibleToPick;

    #endregion

    #region Methods

    /// <summary>
    /// Initializes the behavior
    /// </summary>
    /// <param name="currentSet"></param>
    public virtual void InitializeObjBehavior(GameObject currentSet)
    {
        this.currentSet = currentSet;

        MakeObjectInvisible(!inScene, false);

        UpdateMethods();
        if (obj != null)
        {
            obj.behavior = this;

            foreach (UseOfVerb verb in useOfVerbs)
            {
                verb.actuatorObj = this;
                verb.targetObj = null;
            }
        }
    }

    /// <summary>
    /// Plays the possible initial behavior the object has when sets that containes it is spawned and on stage
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator _PlayInitialBehavior()
    {
        currentSet.GetComponent<SetBehavior>().ReleaseCutsceneLock();
        yield return null;
    }

    /// <summary>
    /// Returns the verb reaction to the verb passed as parameter
    /// </summary>
    /// <param name="verb"></param>
    /// <returns></returns>
    public virtual UseOfVerb GetUseOfVerb(ActionVerb verb)
    {
        UseOfVerb result = null;
        foreach (UseOfVerb useOfVerb in useOfVerbs)
        {
            if (useOfVerb.verb == verb)
            {
                result = useOfVerb;

                result.actuatorObj = this;
                result.targetObj = null;

                if(result.useType == VerbResult.ExecuteMethod)
                {
                    result.methodToExecute = Methods[result.methodID];
                }
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Checkes if the verb passed as parameter has a corresponding verb reaction
    /// </summary>
    /// <param name="verb"></param>
    /// <param name="ignoreWalk"></param>
    /// <returns></returns>
    public virtual bool CheckUseOfVerb(ActionVerb verb, bool ignoreWalk = true)
    {
        if (ignoreWalk && verb == DataManager.Instance.verbsDictionary["walk"])
        {
                return true;
        }

        foreach(UseOfVerb useOfVerb in useOfVerbs)
        {
            if(useOfVerb.verb == verb)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the point where the PC will interact with the object
    /// </summary>
    /// <returns></returns>
    public virtual Transform GetInteractionPoint()
    {
        return interactionPoint;
    }

    /// <summary>
    /// Returns the point where the PC will look at when interacts with the object
    /// </summary>
    /// <returns></returns>
    public virtual Transform GetLookAtPoint()
    {
        if (lookAtPoint == null)
            return transform;

        return lookAtPoint;
    }

    /// <summary>
    /// Update the methods data
    /// </summary>
    public void UpdateMethods()
    {
        UpdateMethodInfo();
        UpdateMethodNames();
    }

    /// <summary>
    /// Collects all the info from the coroutine methods whose name doesn't start with character "_"
    /// </summary>
    protected void UpdateMethodInfo()
    {
        List<MethodInfo> methodList = new List<MethodInfo>();
        Type type = GetType();
        while (type != typeof(MonoBehaviour))
        {
            IEnumerable<MethodInfo> aux = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && m.Name[0] != '_' && m.ReturnType == typeof(IEnumerator));
            methodList.AddRange(aux);
            type = type.BaseType;
        }

        methods = new SerializableMethodInfo[methodList.Count];

        for (int i = 0; i < methodList.Count; i++)
        {
            methods[i] = new SerializableMethodInfo(methodList[i]);
        }
    }

    /// <summary>
    /// Stores the name of the methods collected in Methods
    /// </summary>
    protected void UpdateMethodNames()
    {
        methodNames = new string[Methods.Length];
        for (int i = 0; i < methodNames.Length; i++)
        {
            methodNames[i] = Methods[i].methodInfo.Name;
        }
    }

    /// <summary>
    /// Executed when player picks up the object
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator _GetPicked()
    {
        if (characterVisibleToPick)
        {
            yield return StartCoroutine(PlayPickAnimation());
        }

        MakeObjectInvisible(true);
        yield return null;
    }

    /// <summary>
    /// Executes the PC pick animation, stopping the execution until is done
    /// </summary>
    /// <returns></returns>
    protected IEnumerator PlayPickAnimation()
    {
        AddAnimationLock();
        PCController.mainAnimationCallback += ReleaseAnimationLock;
        PCController.AnimationController.PickObject(objHeight, objWeight);

        while (animationLocks.Count > 0)
        {
            yield return null;
        }

        PCController.mainAnimationCallback -= ReleaseAnimationLock;
    }

    /// <summary>
    /// Executed when player steals the object
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator _GetStolen()
    {
        if (characterVisibleToPick)
        {
            yield return StartCoroutine(PlayStealAnimation());
        }

        MakeObjectInvisible(true);
        yield return null;
    }

    /// <summary>
    /// Executes the PC steal animation, stopping the execution until is done
    /// </summary>
    /// <returns></returns>
    protected IEnumerator PlayStealAnimation()
    {
        AddAnimationLock();
        PCController.mainAnimationCallback += ReleaseAnimationLock;
        PCController.AnimationController.StealObject(objHeight);

        while (animationLocks.Count > 0)
        {
            yield return null;
        }

        PCController.mainAnimationCallback -= ReleaseAnimationLock;
    }

    /// <summary>
    /// Executed when PC thinks about the object
    /// </summary>
    /// <param name="conversation"></param>
    /// <returns></returns>
    public virtual IEnumerator _Think(VIDE_Assign conversation)
    {
        yield return PCController.MovementController.MoveAndRotateToDirection(PCController.transform.position, Vector3.forward);

        PCController.currentSet.TurnOnOffLights(false);
        yield return new WaitForSeconds(0.5f);
        PCController.TurnOnOffThinkLight(true);

        yield return _StartConversation(conversation);

        PCController.TurnOnOffThinkLight(false);
        yield return new WaitForSeconds(0.5f);
        PCController.currentSet.TurnOnOffLights(true);
    }

    /// <summary>
    /// Returns a point around the object at a given distance
    /// </summary>
    /// <param name="PCPosition"></param>
    /// <param name="interactionRadius"></param>
    /// <returns></returns>
    public Vector3 GetPointAroundObject(Vector3 PCPosition, float interactionRadius)
    {
        Vector3 direction = PCPosition - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        Vector3 point = transform.position + direction * interactionRadius;
        point.y = PCPosition.y;

        return point;
    }

    /// <summary>
    /// Returns the name of the object
    /// </summary>
    /// <returns></returns>
    public virtual string GetObjName()
    {
        return obj.name;
    }

    /// <summary>
    /// Returns the sprite that represents the object
    /// </summary>
    /// <returns></returns>
    public virtual Sprite GetInventorySprite()
    {
        return obj.GetInventorySprite();
    }

    /// <summary>
    /// Turns the object invisible. Also deactivates its possible obstacle collider and recalculates the set NavMeshSurface
    /// </summary>
    /// <param name="invisible"></param>
    /// <param name="recalculateNavMesh"></param>
    protected virtual void MakeObjectInvisible(bool invisible, bool recalculateNavMesh = true)
    {
        inScene = !invisible;

        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = inScene;
            if (recalculateNavMesh) currentSet.GetComponent<SetBehavior>().RecalculateMesh();
        }
        gameObject.SetActive(inScene);
    }

    #region Data methods

    /// <summary>
    /// Loads the data received as a parameter in the variables
    /// </summary>
    /// <param name="data"></param>
    public virtual void LoadData(InteractableObjData data)
    {
        inScene = data.inScene;
    }

    /// <summary>
    /// Returns a data object with the info of the behavior
    /// </summary>
    /// <returns></returns>
    public virtual InteractableObjData GetObjData()
    {
        return new InteractableObjData(inScene);
    }

    #endregion

    #region Dialogue methods

    /// <summary>
    /// Coroutine that displays the dialogue UI and starts a conversation. It's not done until the conversation is finished
    /// </summary>
    /// <param name="dialogue"></param>
    /// <returns></returns>
    public virtual IEnumerator _StartConversation(VIDE_Assign dialogue)
    {
        DialogueUIController.PrepareDialogueUI(this, dialogue);
        CameraManager.LockUnlockCurrentDetailCamera(false);
        yield return StartCoroutine(_BeginDialogue(dialogue));
    }

    /// <summary>
    /// Starts a conversation. The coroutine is not done until the conversation is finished
    /// </summary>
    /// <param name="dialogue"></param>
    /// <returns></returns>
    public virtual IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        VD.OnNodeChange += OnNodeChange;
        VD.OnEnd += EndDialogue;

        VD.BeginDialogue(dialogue);

        while(VD.isActive)
        {
            yield return null;
        }

        //If any animation is still playing at the end of the conversation, wait until they're finished
        while(animationLocks.Count > 0)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Calls for the next dialogue of the conversation
    /// </summary>
    /// <param name="dialogue"></param>
    /// <returns></returns>
    public virtual IEnumerator _NextDialogue(VIDE_Assign dialogue)
    {
        if (VD.assigned == dialogue)
        {
            VD.Next();
        }

        yield return null;
    }

    /// <summary>
    /// It is called when the player chooses an dialogue option of a player dialogue node
    /// </summary>
    /// <param name="commentIndex"></param>
    /// <returns>Returns if it has to automatically pass to the next node of the Dialogue or not</returns>
    public virtual bool OnChoosePlayerOption(int commentIndex)
    {
        VD.NodeData data = VD.nodeData;
        data.commentIndex = commentIndex;

        DialogueUINode node = new DialogueUINode();

        node.isPlayer = false;

        if (data.sprite != null)
            node.sprite = data.sprite;
        else if (VD.assigned.defaultPlayerSprite != null)
            node.sprite = VD.assigned.defaultPlayerSprite;

        node.message = data.comments[data.commentIndex]; 
        
        if (data.tag.Length > 0)
            node.tag = data.tag;
        else
            node.tag = VD.assigned.alias;

        DialogueUIController.UpdateUI(node);

        return false;
    }

    /// <summary>
    /// Sets the player options of the next dialogue
    /// </summary>
    /// <param name="data"></param>
    /// <param name="node"></param>
    public virtual void SetPlayerOptions(VD.NodeData data, DialogueUINode node)
    {
        node.options = new Dictionary<int, string>();
        for (int i = 0; i < data.comments.Length; i++)
        {
            node.options.Add(i, data.comments[i]);
        }
    }

    /// <summary>
    /// Executed when a new dialogue node is needed
    /// </summary>
    /// <param name="data"></param>
    public virtual void OnNodeChange(VD.NodeData data)
    {
        DialogueUINode node = new DialogueUINode();

        node.isPlayer = data.isPlayer;

        if (data.isPlayer)
        {
            if (data.sprite != null)
                node.sprite = data.sprite;
            else if (VD.assigned.defaultPlayerSprite != null)
                node.sprite = VD.assigned.defaultPlayerSprite;

            SetPlayerOptions(data, node);

            if (data.tag.Length > 0)
                node.tag = data.tag;
            else
                node.tag = VD.assigned.alias;
        }
        else
        {
            if (data.sprite != null)
            {
                if (data.extraVars.ContainsKey("sprite"))
                {
                    if (data.commentIndex == (int)data.extraVars["sprite"])
                        node.sprite = data.sprite;
                    else
                        node.sprite = VD.assigned.defaultNPCSprite;
                }
                else
                {
                    node.sprite = data.sprite;
                }
            }
            else if (VD.assigned.defaultNPCSprite != null)
                node.sprite = VD.assigned.defaultNPCSprite;

            node.message = data.comments[data.commentIndex];

            if (data.tag.Length > 0)
                node.tag = data.tag;
            else
                node.tag = VD.assigned.alias;
        }

        DialogueUIController.UpdateUI(node);
    }

    /// <summary>
    /// Executed at the end of the dialogue
    /// </summary>
    /// <param name="data"></param>
    public virtual void EndDialogue(VD.NodeData data)
    {
        VD.OnNodeChange -= OnNodeChange;
        VD.OnEnd -= EndDialogue;

        CameraManager.LockUnlockCurrentDetailCamera(true);
        DialogueUIController.EndDialogue();

        VD.EndDialogue();
    }

    #endregion

    /// <summary>
    /// Generic method that executes the methods subscribed to the mainAnimationCallback event. It is called from a key trigger of an animation
    /// </summary>
    public void ExecuteAnimationCallback()
    {
        if(mainAnimationCallback != null)
            mainAnimationCallback();
    }

    /// <summary>
    /// Secondary generic method that executes the methods subscribed to the mainAnimationCallback event. It is called from a key trigger of an animation
    /// </summary>
    public void ExecuteSecondAnimationCallback()
    {
        if(secondAnimationCallback != null)
            secondAnimationCallback();
    }

    /// <summary>
    /// Adds a lock to the animationLocks stack, and that will block the execution of a coroutine until the lock is released
    /// </summary>
    protected void AddAnimationLock()
    {
        animationLocks.Push(true);
    }

    /// <summary>
    /// Releases the last added lock to the animationLocks stack
    /// </summary>
    protected void ReleaseAnimationLock()
    {
        animationLocks.Pop();
    }

    #endregion
}

/// <summary>
/// Determines how the player must move at the start of an object interaction
/// </summary>
public enum VerbMovement
{
    DontMove, MoveAround, MoveToExactPoint
}

/// <summary>
/// Determines how the player interacts with an object
/// </summary>
public enum VerbResult
{
    StartConversation, PickObject, StealObject, ExecuteMethod, Think
}

/// <summary>
/// The reaction of an object to a verb
/// </summary>
[Serializable]
public class UseOfVerb
{
    public InteractableObjBehavior actuatorObj;
    public InteractableObjBehavior targetObj;
    public ActionVerb verb;
    public bool multiObj;

    public VerbMovement verbMovement;
    public float distanceFromObject;
    public Transform overrideInteractionPoint;

    public VerbResult useType;

    public VIDE_Assign conversation;

    public SerializableMethodInfo methodToExecute;
    public int methodID;

    public string GetVerbInfo(bool waitingSecondObj)
    {
        if(waitingSecondObj || (verb == DataManager.Instance.verbsDictionary["hit"] && multiObj))
        {
            return verb.multiObjActionInfo;
        }
        else
        {
            return verb.singleObjActionInfo;
        }
    }

    public UseOfVerb CopyUseOfVerb()
    {
        UseOfVerb copy = new UseOfVerb();

        copy.actuatorObj = actuatorObj;
        copy.targetObj = targetObj;
        copy.verb = verb;
        copy.multiObj = multiObj;

        copy.verbMovement = verbMovement;
        copy.distanceFromObject = distanceFromObject;
        copy.overrideInteractionPoint = overrideInteractionPoint;

        copy.useType = useType;

        copy.conversation = conversation;

        copy.methodToExecute = methodToExecute;
        copy.methodID = methodID;

        return copy;
    }
}
