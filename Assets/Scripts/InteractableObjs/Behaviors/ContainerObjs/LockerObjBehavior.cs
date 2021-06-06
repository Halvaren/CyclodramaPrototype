using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerObjBehavior : ContainerObjBehavior
{
    public VIDE_Assign cantUnlockComment;
    public VIDE_Assign alreadyUnlockComment;
    public VIDE_Assign lockedComment;
    public NumLockObjBehavior numLock;

    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip lockedClip;

    public override IEnumerator LookInto()
    {
        if(numLock.gameObject.activeSelf)
        {
            PlayLockedSound();
            yield return StartCoroutine(_StartConversation(lockedComment));
        }
        else
        {
            AddAnimationLock();
            mainAnimationCallback += ReleaseAnimationLock;
            PlayOpenAnimation();
            PlayOpenSound();

            while(animationLocks.Count > 0)
            {
                yield return null;
            }

            mainAnimationCallback -= ReleaseAnimationLock;

            yield return base.LookInto();
        }
    }

    public IEnumerator ForceLock()
    {
        if(numLock.gameObject.activeSelf)
        {
            PlayLockedSound();
            yield return StartCoroutine(_StartConversation(cantUnlockComment));
        }
        else
        {
            yield return StartCoroutine(_StartConversation(alreadyUnlockComment));
        }
    }

    public override void GetBack()
    {
        PlayCloseSound();
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

    public void PlayOpenSound()
    {
        AudioManager.PlaySound(openClip, SoundType.Set);
    }

    public void PlayCloseSound()
    {
        AudioManager.PlaySound(closeClip, SoundType.Set);
    }

    public void PlayLockedSound()
    {
        AudioManager.PlaySound(lockedClip, SoundType.Set);
    }
}
