using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
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
        if (ignoreWalk && verb == DataManager.instance.verbsDictionary["walk"])
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
        _ApplyData(false);
        yield return false;
    }

    public virtual IEnumerator _GetStolen()
    {
        _ApplyData(false);
        yield return false;
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

    #region Data methods

    public void _LoadData(InteractableObjData data)
    {
        _ApplyData(data.inScene);
    }

    public void _ApplyData(bool inScene)
    {
        this.inScene = inScene;

        if (!inScene)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public virtual InteractableObjData _GetObjData()
    {
        return new InteractableObjData(inScene);
    }

    #endregion

    #region Dialogue methods

    public virtual IEnumerator BeginDialogue(VIDE_Assign dialogue)
    {
        VD.BeginDialogue(dialogue);

        while(VD.isActive)
        {
            yield return null;
        }
    }

    public virtual void NextDialogue(VIDE_Assign dialogue)
    {
        if(VD.assigned == dialogue)
            VD.Next();
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
        PCController.instance.EnableGameplayInput(value);
        PCController.instance.EnableInventoryInput(value);
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
