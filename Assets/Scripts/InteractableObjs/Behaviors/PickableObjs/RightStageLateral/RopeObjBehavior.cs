using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RopeObjBehavior : PickableObjBehavior
{
    [Header("Object state")]
    public bool cut = false;

    [Header("Multi-object verbs fields")]
    public VIDE_Assign uncutDefaultUseComment;
    [Space(10)]
    public VIDE_Assign uncutDefaultDrawComment;
    [Space(10)]
    public VIDE_Assign uncutDefaultGiveComment;
    [Space(10)]
    public VIDE_Assign uncutDefaultHitComment;
    [Space(10)]
    public VIDE_Assign uncutDefaultThrowComment;

    [Header("Other variables")]
    public BoxCollider secondTriggerCollider;
    public BoxCollider secondObstacleCollider;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        if(cut && !inventoryObj)
        {
            SetFallen();
        }

        if(triggerCollider != null)
        {
            triggerCollider.enabled = !cut;
            obstacleCollider.enabled = !cut;
            secondTriggerCollider.enabled = cut;
            secondObstacleCollider.enabled = cut;
        }
    }

    public override UseOfVerb GetUseOfVerb(ActionVerb verb)
    {
        UseOfVerb originalUseOfVerb = base.GetUseOfVerb(verb);

        if(originalUseOfVerb != null)
        {
            UseOfVerb useOfVerb = originalUseOfVerb.CopyUseOfVerb();

            if (useOfVerb.multiObj)
                useOfVerb.multiObj = cut;

            return useOfVerb;
        }
        return null;
    }

    public override IEnumerator _GetPicked()
    {
        if(!cut)
        {
            yield return StartCoroutine(_StartConversation(cannotPickComment));
        }
        else
        {
            yield return base._GetPicked();
        }
    }

    public override IEnumerator _GetStolen()
    {
        if (!cut)
        {
            yield return StartCoroutine(_StartConversation(cannotPickComment));
        }
        else
        {
            yield return base._GetStolen();
        }
    }

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            yield return StartCoroutine(_StartConversation(uncutDefaultUseComment));
        }
        else
        {
            yield return base.UseMethod(targetObj);
        }
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            yield return StartCoroutine(_StartConversation(uncutDefaultDrawComment));
        }
        else
        {
            yield return base.DrawMethod(targetObj);
        }
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            yield return StartCoroutine(_StartConversation(uncutDefaultGiveComment));
        }
        else
        {
            yield return base.GiveMethod(targetObj);
        }
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            yield return StartCoroutine(_StartConversation(uncutDefaultHitComment));
        }
        else
        {
            yield return base.HitMethod(targetObj);
        }
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            yield return StartCoroutine(_StartConversation(uncutDefaultThrowComment));
        }
        else
        {
            yield return base.ThrowMethod(targetObj);
        }
    }

    public void Fall()
    {
        Animator.SetTrigger("fall");
    }

    public void SetFallen()
    {
        Animator.SetTrigger("fallen");
    }

    public void SetCut(bool value)
    {
        cut = value;
        triggerCollider.enabled = !cut;
        secondTriggerCollider.enabled = cut;

        if(obstacleCollider)
        {
            obstacleCollider.enabled = !cut;
        }
        if(secondObstacleCollider)
        {
            secondObstacleCollider.enabled = cut;
        }

        if(obstacleCollider || secondObstacleCollider)
        {
            currentSet.GetComponent<SetBehavior>().RecalculateMesh();
        }
    }

    protected override void MakeObjectInvisible(bool invisible, bool recalculateNavMesh = true)
    {
        inScene = !invisible;

        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = inScene;
        }

        if (secondObstacleCollider != null) secondObstacleCollider.enabled = inScene;

        if((obstacleCollider || secondObstacleCollider) && recalculateNavMesh)
        {
            currentSet.GetComponent<SetBehavior>().RecalculateMesh();
        }

        gameObject.SetActive(inScene);
    }

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is RopeObjData ropeObjData)
        {
            cut = ropeObjData.cut;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new RopeObjData(inScene, inventoryObj, cut);
    }
}
