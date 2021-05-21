using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerObjBehavior : ContainerObjBehavior
{
    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    public VIDE_Assign cantUnlockComment;
    public VIDE_Assign alreadyUnlockComment;
    public VIDE_Assign lockedComment;
    public NumLockObjBehavior numLock;

    public override IEnumerator LookInto()
    {
        if(numLock.gameObject.activeSelf)
        {
            DialogueUIController.PrepareDialogueUI(this, lockedComment);
            yield return StartCoroutine(_BeginDialogue(lockedComment));
        }
        else
        {
            AddAnimationLock();
            animationCallback += ReleaseAnimationLock;
            PlayOpenAnimation();

            while(animationLocks.Count > 0)
            {
                yield return null;
            }

            animationCallback -= ReleaseAnimationLock;

            yield return base.LookInto();
        }
    }

    public IEnumerator ForceLock()
    {
        if(numLock.gameObject.activeSelf)
        {
            DialogueUIController.PrepareDialogueUI(this, cantUnlockComment);
            yield return StartCoroutine(_BeginDialogue(cantUnlockComment));
        }
        else
        {
            DialogueUIController.PrepareDialogueUI(this, alreadyUnlockComment);
            yield return StartCoroutine(_BeginDialogue(alreadyUnlockComment));
        }
    }

    public override void GetBack()
    {
        PlayCloseAnimation();
        base.GetBack();
    }

    public void PlayOpenAnimation()
    {
        Animator.SetTrigger("open");
    }

    public void PlayCloseAnimation()
    {
        Animator.SetTrigger("close");
    }
}
