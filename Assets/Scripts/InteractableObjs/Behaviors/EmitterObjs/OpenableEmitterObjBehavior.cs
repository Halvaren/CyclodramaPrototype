using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableEmitterObjBehavior : EmitterObjBehavior
{
    public VIDE_Assign inspectComment;

    public bool locked;

    public VIDE_Assign lockedComment;
    public VIDE_Assign nowUnlockedComment;
    public VIDE_Assign alreadyUnlockedComment;

    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip lockedClip;
    public AudioClip unlockClip;

    public virtual IEnumerator OpenMethod()
    {
        if(locked)
        {
            PlayLockedSound();
            yield return StartCoroutine(_StartConversation(lockedComment));
        }
        else
        {
            Open();
            PlayOpenSound();

            yield return StartCoroutine(DropObjs(PlayPickAnimation()));

            Close();
            PlayCloseSound();
        }
    }

    public virtual IEnumerator InspectInside()
    {
        if(locked)
        {
            PlayLockedSound();
            yield return StartCoroutine(_StartConversation(lockedComment));
        }
        else
        {
            Open();
            PlayOpenSound();

            if (inspectComment != null)
            {
                yield return StartCoroutine(_StartConversation(inspectComment));
            }

            Close();
            PlayCloseSound();
        }
    }

    //Force lock method
    public IEnumerator Unlock()
    {
        if (locked)
        {
            PlayUnlockSound();
            yield return StartCoroutine(_StartConversation(nowUnlockedComment));

            locked = false;
        }
        else
        {
            yield return StartCoroutine(_StartConversation(alreadyUnlockedComment));
        }
    }

    public void Open()
    {
        Animator.SetTrigger("open");
    }

    public void Close()
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

    public void PlayUnlockSound()
    {
        AudioManager.PlaySound(unlockClip, SoundType.Set);
    }

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is OpenableEmmitterObjData openableEmmitterObjData)
        {
            locked = openableEmmitterObjData.locked;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new OpenableEmmitterObjData(inScene, dropObjs, locked);
    }
}
