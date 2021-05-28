using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableLockedEmitterObjBehavior : OpenableEmitterObjBehavior
{
    public bool locked;

    public VIDE_Assign lockedComment;
    public VIDE_Assign nowUnlockedComment;
    public VIDE_Assign alreadyUnlockedComment;

    //Open method

    public override IEnumerator OpenMethod()
    {
        if (locked)
        {
            yield return StartCoroutine(_StartConversation(lockedComment));
        }
        else
            yield return base.OpenMethod();
    }

    public override IEnumerator InspectInside()
    {
        if (locked)
        {
            yield return StartCoroutine(_StartConversation(lockedComment));
        }
        else
            yield return base.OpenMethod();
    }

    //Force lock method
    public IEnumerator Unlock()
    {
        if(locked)
        {
            yield return StartCoroutine(_StartConversation(nowUnlockedComment));

            locked = false;
        }
        else
        {
            yield return StartCoroutine(_StartConversation(alreadyUnlockedComment));
        }
    }
}
