using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages the behavior of a door
/// </summary>
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
    public AudioClip lockedClip;
    [HideInInspector]
    public AudioClip unlockClip;

    [HideInInspector]
    public VIDE_Assign lockedComment;
    [HideInInspector]
    public VIDE_Assign unlockComment;
    [HideInInspector]
    public VIDE_Assign alreadyUnlockedComment;

    /// <summary>
    /// Initializes the behavior
    /// </summary>
    /// <param name="currentSet"></param>
    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        SetOpenedClosedDoor(opened);
    }

    /// <summary>
    /// Opens the physical door
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator OpenDoor()
    {
        if(locked)
        {
            PlayLockedSound();
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

    /// <summary>
    /// Closes the physical door
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Forces the possible lock of the door
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator ForceLock()
    {
        if(locked)
        {
            PlayUnlockSound();
            yield return StartCoroutine(_StartConversation(unlockComment));
            locked = false;
        }
        else
        {
            yield return StartCoroutine(_StartConversation(alreadyUnlockedComment));
        }
    }

    /// <summary>
    /// Opens or closes the door, depending is closed or opened
    /// </summary>
    /// <returns></returns>
    public IEnumerator UseDoor()
    {
        if (opened) yield return StartCoroutine(CloseDoor());
        else yield return StartCoroutine(OpenDoor());
    }

    /// <summary>
    /// Plays open animation
    /// </summary>
    public void PlayOpenAnimation()
    {
        Animator.SetTrigger("open");
    }

    /// <summary>
    /// Plays close animation
    /// </summary>
    public void PlayCloseAnimation()
    {
        Animator.SetTrigger("close");
    }

    /// <summary>
    /// Sets the state of the door
    /// </summary>
    /// <param name="value"></param>
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

    /// <summary>
    /// Plays open sound clip
    /// </summary>
    public void PlayOpenSound()
    {
        AudioManager.PlaySound(openClip, SoundType.Set);
    }

    /// <summary>
    /// Plays close sound clip
    /// </summary>
    public void PlayCloseSound()
    {
        AudioManager.PlaySound(closeClip, SoundType.Set);
    }

    /// <summary>
    /// Plays locked sound clip
    /// </summary>
    public void PlayLockedSound()
    {
        AudioManager.PlaySound(lockedClip, SoundType.Set);
    }

    /// <summary>
    /// Plays unlock sound clip
    /// </summary>
    public void PlayUnlockSound()
    {
        AudioManager.PlaySound(unlockClip, SoundType.Set);
    }

    #region Data methods

    /// <summary>
    /// Loads the data received as a parameter in the variables
    /// </summary>
    /// <param name="data"></param>
    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is DoorData doorData)
        {
            opened = doorData.opened;
            locked = doorData.locked;
        }
    }

    /// <summary>
    /// Returns a data object with the info of the behavior
    /// </summary>
    /// <returns></returns>
    public override InteractableObjData GetObjData()
    {
        return new DoorData(inScene, opened, locked);
    }

    #endregion
}
