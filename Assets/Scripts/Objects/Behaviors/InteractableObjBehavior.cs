using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum ObjState
{
    Normal, Picked
}

public class InteractableObjBehavior : MonoBehaviour
{
    [SerializeField]
    protected SerializableMethodInfo[] methods;
    public SerializableMethodInfo[] Methods
    {
        get
        {
            if (methods == null)
            {
                _UpdateMethodInfo();
            }
            return methods;
        }
    }

    [SerializeField]
    protected string[] methodNames;
    public string[] MethodNames
    {
        get
        {
            if (methodNames == null)
            {
                _UpdateMethodNames();
            }

            return methodNames;
        }
    }

    public InteractableObj obj;

    public List<UseOfVerb> useOfVerbs;

    public ObjState objState = ObjState.Normal;

    public Collider triggerCollider;
    public Collider TriggerCollider
    {
        get
        {
            if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
            return triggerCollider;
        }
    }

    protected void Start()
    {
        InitializeObjBehavior();
    }

    protected virtual void InitializeObjBehavior()
    {
        _UpdateMethods();
        if (obj != null)
        {
            obj.behavior = this;

            foreach (UseOfVerb verb in useOfVerbs)
                verb.target = this;
        }
    }

    public UseOfVerb _GetUseOfVerb(ActionVerb verb)
    {
        UseOfVerb result = null;
        foreach (UseOfVerb useOfVerb in useOfVerbs)
        {
            if (useOfVerb.verb == verb)
            {
                result = useOfVerb;
                result.target = this;
                if(result.useType == VerbResult.ExecuteMethod)
                {
                    result.methodToExecute = Methods[result.methodID];
                }
                break;
            }
        }

        return result;
    }

    public void _UpdateMethods()
    {
        _UpdateMethodInfo();
        _UpdateMethodNames();
    }

    protected void _UpdateMethodInfo()
    {
        List<MethodInfo> methodList = new List<MethodInfo>();
        Type type = GetType();
        while (type != typeof(MonoBehaviour))
        {
            IEnumerable<MethodInfo> aux = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && m.Name[0] != '_');
            methodList.AddRange(aux);
            type = type.BaseType;
        }

        methods = new SerializableMethodInfo[methodList.Count];

        for (int i = 0; i < methodList.Count; i++)
        {
            methods[i] = new SerializableMethodInfo(methodList[i]);
        }
    }

    protected void _UpdateMethodNames()
    {
        methodNames = new string[Methods.Length];
        for (int i = 0; i < methodNames.Length; i++)
        {
            methodNames[i] = Methods[i].methodInfo.Name;
        }
    }

    public virtual void _GetPicked()
    {
        _ApplyState(ObjState.Picked);
    }

    public Vector3 _GetPointAroundObject(Vector3 PCPosition, float interactionRadius)
    {
        Vector3 direction = PCPosition - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        Vector3 point = transform.position + direction * interactionRadius;
        point.y = PCPosition.y;

        return point;
    }

    public void _LoadData(InteractableObjData data)
    {
        _ApplyState(data.state);
    }

    public void _ApplyState(ObjState state)
    {
        objState = state;

        if (state == ObjState.Picked)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public InteractableObjData _GetInteractableObjData()
    {
        return new InteractableObjData(objState);
    }
}


public enum VerbMovement
{
    DontMove, MoveAround, MoveToExactPoint
}

public enum VerbResult
{
    StartConversation, PickObject, ExecuteMethod
}

[Serializable]
public class UseOfVerb
{
    public InteractableObjBehavior target;
    public ActionVerb verb;

    public VerbMovement verbMovement;
    public float distanceFromObject;
    public Transform pointToMove;

    public VerbResult useType;

    public VIDE_Assign conversation;

    public SerializableMethodInfo methodToExecute;
    public int methodID;
}
