using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using VIDE_Data;

public class InteractableObjBehavior : MonoBehaviour
{
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

    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    private AudioSource audioSource;
    public AudioSource AudioSource
    {
        get
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            return audioSource;
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

    public virtual IEnumerator _PlayInitialBehavior()
    {
        currentSet.GetComponent<SetBehavior>().ReleaseCutsceneLock();
        yield return null;
    }

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

    public virtual Transform GetInteractionPoint()
    {
        return interactionPoint;
    }

    public virtual Transform GetLookAtPoint()
    {
        if (lookAtPoint == null)
            return transform;

        return lookAtPoint;
    }

    public void UpdateMethods()
    {
        UpdateMethodInfo();
        UpdateMethodNames();
    }

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

    protected void UpdateMethodNames()
    {
        methodNames = new string[Methods.Length];
        for (int i = 0; i < methodNames.Length; i++)
        {
            methodNames[i] = Methods[i].methodInfo.Name;
        }
    }

    public virtual IEnumerator _GetPicked()
    {
        if (characterVisibleToPick)
        {
            yield return StartCoroutine(PlayPickAnimation());
        }

        MakeObjectInvisible(true);
        yield return null;
    }

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

    public virtual IEnumerator _GetStolen()
    {
        if (characterVisibleToPick)
        {
            yield return StartCoroutine(PlayPickAnimation());
        }

        MakeObjectInvisible(true);
        yield return null;
    }

    protected IEnumerator PlayStealAnimation()
    {
        AddAnimationLock();
        PCController.mainAnimationCallback += ReleaseAnimationLock;
        PCController.AnimationController.StealObject(objHeight, objWeight);

        while (animationLocks.Count > 0)
        {
            yield return null;
        }

        PCController.mainAnimationCallback -= ReleaseAnimationLock;
    }

    public virtual IEnumerator _Think(VIDE_Assign conversation)
    {
        yield return PCController.MovementController.MoveAndRotateToDirection(PCController.transform.position, Vector3.forward);

        PCController.currentSet.TurnOnOffLights(false);
        yield return new WaitForSeconds(0.5f);
        PCController.thinkSpotLight.enabled = true;

        yield return _StartConversation(conversation);

        PCController.thinkSpotLight.enabled = false;
        yield return new WaitForSeconds(0.5f);
        PCController.currentSet.TurnOnOffLights(true);
    }

    public Vector3 GetPointAroundObject(Vector3 PCPosition, float interactionRadius)
    {
        Vector3 direction = PCPosition - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        Vector3 point = transform.position + direction * interactionRadius;
        point.y = PCPosition.y;

        return point;
    }

    public virtual string GetObjName()
    {
        return obj.name;
    }

    public virtual Sprite GetInventorySprite()
    {
        return obj.GetInventorySprite();
    }

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

    public virtual void LoadData(InteractableObjData data)
    {
        inScene = data.inScene;
    }

    public virtual InteractableObjData GetObjData()
    {
        return new InteractableObjData(inScene);
    }

    #endregion

    #region Dialogue methods

    public virtual IEnumerator _StartConversation(VIDE_Assign dialogue)
    {
        DialogueUIController.PrepareDialogueUI(this, dialogue);
        yield return StartCoroutine(_BeginDialogue(dialogue));
    }

    public virtual IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        VD.OnNodeChange += OnNodeChange;
        VD.OnEnd += EndDialogue;

        VD.BeginDialogue(dialogue);

        while(VD.isActive)
        {
            yield return null;
        }

        while(animationLocks.Count > 0)
        {
            yield return null;
        }
    }

    public virtual IEnumerator _NextDialogue(VIDE_Assign dialogue)
    {
        if (VD.assigned == dialogue)
        {
            VD.Next();
        }

        yield return null;
    }

    public virtual void OnChoosePlayerOption(int commentIndex)
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
    }

    public virtual void SetPlayerOptions(VD.NodeData data, DialogueUINode node)
    {
        node.options = new Dictionary<int, string>();
        for (int i = 0; i < data.comments.Length; i++)
        {
            node.options.Add(i, data.comments[i]);
        }
    }

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

    public virtual void EndDialogue(VD.NodeData data)
    {
        VD.OnNodeChange -= OnNodeChange;
        VD.OnEnd -= EndDialogue;

        DialogueUIController.EndDialogue();

        VD.EndDialogue();
    }

    #endregion

    public void ExecuteAnimationCallback()
    {
        if(mainAnimationCallback != null)
            mainAnimationCallback();
    }

    public void ExecuteSecondAnimationCallback()
    {
        if(secondAnimationCallback != null)
            secondAnimationCallback();
    }

    protected void AddAnimationLock()
    {
        animationLocks.Push(true);
    }

    protected void ReleaseAnimationLock()
    {
        animationLocks.Pop();
    }
}


public enum VerbMovement
{
    DontMove, MoveAround, MoveToExactPoint
}

public enum VerbResult
{
    StartConversation, PickObject, StealObject, ExecuteMethod, Think
}

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
