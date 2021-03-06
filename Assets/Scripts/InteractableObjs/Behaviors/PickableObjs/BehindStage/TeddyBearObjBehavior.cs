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

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        if(fallen && !inventoryObj)
        {
            SetFallenAnimation();
        }
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

    public void SetFallenAnimation()
    {
        Animator.SetTrigger("fallen");
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
            yield return base.UseMethod(targetObj);
        }
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultDrawComment));
        }
        else
        {
            yield return base.DrawMethod(targetObj);
        }
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultGiveComment));
        }
        else
        {
            yield return base.GiveMethod(targetObj);
        }
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultHitComment));
        }
        else
        {
            yield return base.HitMethod(targetObj);
        }
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            yield return StartCoroutine(_StartConversation(unfallenDefaultThrowComment));
        }
        else
        {
            yield return base.ThrowMethod(targetObj);
        }
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

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is TeddyBearObjData teddyBearObjData)
        {
            fallen = teddyBearObjData.fallen;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new TeddyBearObjData(inScene, inventoryObj, fallen);
    }
}
