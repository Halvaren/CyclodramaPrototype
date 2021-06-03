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

    public virtual IEnumerator OpenMethod()
    {
        if(locked)
        {
            yield return StartCoroutine(_StartConversation(lockedComment));
        }
        else
        {
            Animator.SetTrigger("open");

            yield return StartCoroutine(DropObjs(PlayPickAnimation()));

            Animator.SetTrigger("close");
        }
    }

    public virtual IEnumerator InspectInside()
    {
        if(locked)
        {
            yield return StartCoroutine(_StartConversation(lockedComment));
        }
        else
        {
            Animator.SetTrigger("open");

            if (inspectComment != null)
            {
                yield return StartCoroutine(_StartConversation(inspectComment));
                yield return StartCoroutine(DropObjs(PlayPickAnimation()));
            }

            Animator.SetTrigger("close");
        }
    }

    //Force lock method
    public IEnumerator Unlock()
    {
        if (locked)
        {
            yield return StartCoroutine(_StartConversation(nowUnlockedComment));

            locked = false;
        }
        else
        {
            yield return StartCoroutine(_StartConversation(alreadyUnlockedComment));
        }
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
