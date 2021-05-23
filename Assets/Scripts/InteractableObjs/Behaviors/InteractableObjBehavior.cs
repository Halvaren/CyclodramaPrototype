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
    public event AnimationCallback animationCallback;

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

    private DialogueUIController dialogueUIController;
    public DialogueUIController DialogueUIController
    {
        get
        {
            if (dialogueUIController == null) dialogueUIController = GeneralUIController.Instance.dialogueUIController;
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

    protected void Start()
    {
        InitializeObjBehavior();
    }

    protected virtual void InitializeObjBehavior()
    {
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
        PCController.instance.mainAnimationCallback += ReleaseAnimationLock;
        PCController.instance.AnimationController.PickObject(objHeight, objWeight);

        while (animationLocks.Count > 0)
        {
            yield return null;
        }

        PCController.instance.mainAnimationCallback -= ReleaseAnimationLock;
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
        PCController.instance.mainAnimationCallback += ReleaseAnimationLock;
        PCController.instance.AnimationController.StealObject(objHeight, objWeight);

        while (animationLocks.Count > 0)
        {
            yield return null;
        }

        PCController.instance.mainAnimationCallback -= ReleaseAnimationLock;
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

    #region Data methods

    public void _LoadData(InteractableObjData data)
    {
        _ApplyData(data.inScene);
    }

    public void _ApplyData(bool inScene)
    {
        MakeObjectInvisible(!inScene, false);
    }

    protected virtual void MakeObjectInvisible(bool invisible, bool recalculateNavMesh = true)
    {
        inScene = !invisible;

        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = inScene;
            if(recalculateNavMesh) currentSet.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
        gameObject.SetActive(inScene);
    }

    public virtual InteractableObjData _GetObjData()
    {
        return new InteractableObjData(inScene);
    }

    #endregion

    #region Dialogue methods

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

    public virtual void NextDialogue(VIDE_Assign dialogue)
    {
        if(VD.assigned == dialogue)
            VD.Next();
    }

    public virtual void _OnChoosePlayerOption(int commentIndex)
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

    public virtual void SetPlayerOptions(VD.NodeData data, DialogueUINode node)
    {
        node.options = new string[data.comments.Length];
        for (int i = 0; i < data.comments.Length; i++)
        {
            node.options[i] = data.comments[i];
        }
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
        animationCallback();
    }

    protected void AddAnimationLock()
    {
        animationLocks.Push(true);
    }

    protected void ReleaseAnimationLock()
    {
        animationLocks.Pop();
    }

    protected void EnablePlayerInput(bool value)
    {
        PCController.EnableGameplayInput(value);
        PCController.EnableInventoryInput(value);
    }
}


public enum VerbMovement
{
    DontMove, MoveAround, MoveToExactPoint
}

public enum VerbResult
{
    StartConversation, PickObject, StealObject, ExecuteMethod
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
