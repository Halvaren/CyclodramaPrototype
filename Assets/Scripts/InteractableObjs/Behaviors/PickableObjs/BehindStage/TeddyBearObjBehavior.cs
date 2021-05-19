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
    protected override void InitializeObjBehavior()
    {
        base.InitializeObjBehavior();
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

        currentSet.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    public override IEnumerator _GetPicked()
    {
        if(!fallen)
        {
            DialogueUIController.PrepareDialogueUI(this, cannotPickComment);
            yield return StartCoroutine(_BeginDialogue(cannotPickComment));
        }
        else
            yield return base._GetPicked();
    }

    public override IEnumerator _GetStolen()
    {
        if (!fallen)
        {
            DialogueUIController.PrepareDialogueUI(this, cannotPickComment);
            yield return StartCoroutine(_BeginDialogue(cannotPickComment));
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
            currentSet.GetComponent<NavMeshSurface>().BuildNavMesh();
        }

        gameObject.SetActive(inScene);
    }
    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            DialogueUIController.PrepareDialogueUI(this, unfallenDefaultUseComment);
            yield return StartCoroutine(_BeginDialogue(unfallenDefaultUseComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, useObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, defaultUseComment);
                yield return StartCoroutine(_BeginDialogue(defaultUseComment));
            }
        }

        yield return null;
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            DialogueUIController.PrepareDialogueUI(this, unfallenDefaultDrawComment);
            yield return StartCoroutine(_BeginDialogue(unfallenDefaultDrawComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, drawObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, defaultDrawComment);
                yield return StartCoroutine(_BeginDialogue(defaultDrawComment));
            }
        }

        yield return null;
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            DialogueUIController.PrepareDialogueUI(this, unfallenDefaultGiveComment);
            yield return StartCoroutine(_BeginDialogue(unfallenDefaultGiveComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, giveObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, defaultGiveComment);
                yield return StartCoroutine(_BeginDialogue(defaultGiveComment));
            }
        }

        yield return null;
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            DialogueUIController.PrepareDialogueUI(this, unfallenDefaultHitComment);
            yield return StartCoroutine(_BeginDialogue(unfallenDefaultHitComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, hitObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, defaultHitComment);
                yield return StartCoroutine(_BeginDialogue(defaultHitComment));
            }
        }

        yield return null;
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        if (!fallen)
        {
            DialogueUIController.PrepareDialogueUI(this, unfallenDefaultThrowComment);
            yield return StartCoroutine(_BeginDialogue(unfallenDefaultThrowComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, throwObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, defaultThrowComment);
                yield return StartCoroutine(_BeginDialogue(defaultThrowComment));
            }
        }

        yield return null;
    }

    public IEnumerator InspectMethod()
    {
        if(!fallen)
        {
            DialogueUIController.PrepareDialogueUI(this, unfallenInspectComment);
            yield return StartCoroutine(_BeginDialogue(unfallenInspectComment));
        }
        else
        {
            DialogueUIController.PrepareDialogueUI(this, fallenInspectComment);
            yield return StartCoroutine(_BeginDialogue(fallenInspectComment));
        }
    }
}
