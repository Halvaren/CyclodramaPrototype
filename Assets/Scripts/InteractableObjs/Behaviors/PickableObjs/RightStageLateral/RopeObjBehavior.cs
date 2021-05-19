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

    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    protected override void InitializeObjBehavior()
    {
        base.InitializeObjBehavior();

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
            DialogueUIController.PrepareDialogueUI(this, cannotPickComment);
            yield return StartCoroutine(_BeginDialogue(cannotPickComment));
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
            DialogueUIController.PrepareDialogueUI(this, cannotPickComment);
            yield return StartCoroutine(_BeginDialogue(cannotPickComment));
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
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultUseComment);
            yield return StartCoroutine(_BeginDialogue(uncutDefaultUseComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, useObjRelations);

            yield return base.UseMethod(targetObj);
        }
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultDrawComment);
            yield return StartCoroutine(_BeginDialogue(uncutDefaultDrawComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, drawObjRelations);

            yield return base.DrawMethod(targetObj);
        }
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultGiveComment);
            yield return StartCoroutine(_BeginDialogue(uncutDefaultGiveComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, giveObjRelations);

            yield return base.GiveMethod(targetObj);
        }
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultHitComment);
            yield return StartCoroutine(_BeginDialogue(uncutDefaultHitComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, hitObjRelations);

            yield return base.HitMethod(targetObj);
        }
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultThrowComment);
            yield return StartCoroutine(_BeginDialogue(uncutDefaultThrowComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, throwObjRelations);

            yield return base.ThrowMethod(targetObj);
        }
    }

    public void Fall()
    {
        Animator.SetTrigger("Fall");
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
            currentSet.GetComponent<NavMeshSurface>().BuildNavMesh();
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
            currentSet.GetComponent<NavMeshSurface>().BuildNavMesh();
        }

        gameObject.SetActive(inScene);
    }
}
