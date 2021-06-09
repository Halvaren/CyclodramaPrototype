using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCanEmitterObjBehavior : OpenableEmitterObjBehavior
{
    public VIDE_Assign throwGarbageComment;

    public AudioClip throwGarbageSound;

    public IEnumerator _ThrowGarbage(PickableObjBehavior behavior)
    {
        Animator.SetTrigger("open");
        PlayOpenSound();
        StartCoroutine(PlayThrowGarbageSound(0.2f));

        bool found = false;
        foreach(DropObject dropObj in dropObjs)
        {
            if(dropObj.obj == behavior.obj)
            {
                dropObj.quantity++;
                found = true;
                break;
            }
        }

        if(!found)
        {
            DropObject dropObj = new DropObject();
            dropObj.obj = behavior.obj;
            dropObj.banObjs = new List<InteractableObj>();
            if(behavior is CupObjBehavior cupBehavior)
            {
                dropObj.banObjs.Add(cupBehavior.basicCup);
                dropObj.banObjs.Add(cupBehavior.cupWithWater);
                dropObj.banObjs.Add(cupBehavior.cupWithCoffee);
                dropObj.banObjs.Add(cupBehavior.cutCup);
                dropObj.banObjs.Add(cupBehavior.cutCuptWithWater);
                dropObj.banObjs.Add(cupBehavior.cutCupWithCoffee);
            }
            else
            {
                dropObj.banObjs.Add(behavior.obj);
            }

            dropObj.quantity = 1;

            dropObjs.Add(dropObj);
        }

        yield return StartCoroutine(_StartConversation(throwGarbageComment));

        Animator.SetTrigger("close");
        PlayCloseSound();
    }

    IEnumerator PlayThrowGarbageSound(float delay)
    {
        yield return new WaitForSeconds(delay);

        AudioManager.PlaySound(throwGarbageSound, SoundType.Set);
    }
}
