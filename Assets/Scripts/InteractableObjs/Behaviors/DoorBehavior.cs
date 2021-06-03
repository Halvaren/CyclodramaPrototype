using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public bool opened = false;
    [HideInInspector]
    public bool locked = false;

    [HideInInspector]
    public AudioClip openClip;
    [HideInInspector]
    public AudioClip closeClip;

    [HideInInspector]
    public VIDE_Assign lockedComment;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        SetOpenedClosedDoor(opened);
    }

    public virtual IEnumerator OpenDoor()
    {
        if(locked)
        {
            yield return StartCoroutine(_StartConversation(lockedComment));
        }
        else
        {
            if (!opened && Animator != null)
            {
                AddAnimationLock();
                mainAnimationCallback += ReleaseAnimationLock;
                PlayOpenAnimation();
                PlayOpenSound();

                while (animationLocks.Count > 0)
                {
                    yield return null;
                }

                SetOpenedClosedDoor(true);
                mainAnimationCallback -= ReleaseAnimationLock;
            }
        }
    }

    public IEnumerator CloseDoor()
    {
        if (opened && Animator != null)
        {
            AddAnimationLock();
            mainAnimationCallback += ReleaseAnimationLock;
            PlayCloseAnimation();
            PlayCloseSound();

            while (animationLocks.Count > 0)
            {
                yield return null;
            }

            SetOpenedClosedDoor(false);
            mainAnimationCallback -= ReleaseAnimationLock;
        }
    }

    public IEnumerator UseDoor()
    {
        if (opened) yield return StartCoroutine(CloseDoor());
        else yield return StartCoroutine(OpenDoor());
    }

    public void PlayOpenAnimation()
    {
        Animator.SetTrigger("open");
    }

    public void PlayOpenSound()
    {
        AudioSource.Stop();
        AudioSource.clip = openClip;
        AudioSource.Play();
    }

    public void PlayCloseAnimation()
    {
        Animator.SetTrigger("close");
    }

    public void PlayCloseSound()
    {
        AudioSource.Stop();
        AudioSource.clip = closeClip;
        AudioSource.Play();
    }

    public virtual void SetOpenedClosedDoor(bool value)
    {
        opened = value;

        if (obstacleCollider)
        {
            obstacleCollider.enabled = !opened;
            currentSet.GetComponent<SetBehavior>().RecalculateMesh();
        }

        if(Animator != null)
        {
            if (opened)
                Animator.SetTrigger("opened");
            else
                Animator.SetTrigger("closed");
        }
    }

    #region Data methods

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is DoorData doorData)
        {
            opened = doorData.opened;
            locked = doorData.locked;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new DoorData(inScene, opened, locked);
    }

    #endregion
}
