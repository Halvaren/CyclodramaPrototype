using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TeddyBearObjBehavior : PickableObjBehavior
{
    [Header("Object state")]
    public bool fallen = false;

    [Header("Multi-object verbs fields")]
    public VIDE_Assign unfallenDefaultUseComment;
    [Space(10)]
    public VIDE_Assign unfallenDefaultDrawComment;
    [Space(10)]
    public VIDE_Assign unfallenDefaultGiveComment;
    [Space(10)]
    public VIDE_Assign unfallenDefaultHitComment;
    [Space(10)]
    public VIDE_Assign unfallenDefaultThrowComment;

    [Header("Other variables")]
    public Transform secondInteractionPoint;
    public VIDE_Assign unfallenInspectComment;
    public VIDE_Assign fallenInspectComment;

    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);
    }

    public override Transform GetInteractionPoint()
    {
        if (fallen) return secondInteractionPoint;
        return interactionPoint;
    }

    public void Fall()
    {
        Animator.SetTrigger("fall");
    }

    public void SetFallen(bool value)
    {
        fallen = value;

        currentSet.GetComponent<SetBehavior>().RecalculateMesh();
    }

    public override IEnumerator _GetPicked()
    {
        if(!fallen)
        {
            yield return StartCoroutine(_StartConversation(cannotPickComment));
        }
        else
            yield return base._GetPicked();
    }

    public override IEnumerator _GetStolen()
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(cannotPickComment));
        }
        else
            yield return base._GetStolen();
    }

    protected override void MakeObjectInvisible(bool invisible, bool recalculateNavMesh = true)
    {
        inScene = !invisible;

        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = inScene;
        }

        if (recalculateNavMesh)
        {
            currentSet.GetComponent<SetBehavior>().RecalculateMesh();
        }

        gameObject.SetActive(inScene);
    }
    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultUseComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, useObjRelations);

            if (index == 0)
            {
                yield return StartCoroutine(_StartConversation(defaultUseComment));
            }
        }

        yield return null;
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultDrawComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, drawObjRelations);

            if (index == 0)
            {
                yield return StartCoroutine(_StartConversation(defaultDrawComment));
            }
        }

        yield return null;
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultGiveComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, giveObjRelations);

            if (index == 0)
            {
                yield return StartCoroutine(_StartConversation(defaultGiveComment));
            }
        }

        yield return null;
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultHitComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, hitObjRelations);

            if (index == 0)
            {
                yield return StartCoroutine(_StartConversation(defaultHitComment));
            }
        }

        yield return null;
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultThrowComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, throwObjRelations);

            if (index == 0)
            {
                yield return StartCoroutine(_StartConversation(defaultThrowComment));
            }
        }

        yield return null;
    }

    public IEnumerator InspectMethod()
    {
        if(!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenInspectComment));
        }
        else
        {
            yield return StartCoroutine(_StartConversation(fallenInspectComment));
        }
    }
}
