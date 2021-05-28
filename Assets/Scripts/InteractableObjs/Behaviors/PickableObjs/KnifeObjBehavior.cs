using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeObjBehavior : PickableObjBehavior
{
    //[Header("Object state")]

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        yield return base.UseMethod(targetObj);

        //Rope
        if(index == 1)
        {
            RopeObjBehavior rope = (RopeObjBehavior)targetObj;
            if(!rope.cut)
            {
                AddAnimationLock();
                PCController.mainAnimationCallback += ReleaseAnimationLock;
                PCController.AnimationController.UseKnife();

                while (animationLocks.Count > 0)
                {
                    yield return null;
                }

                PCController.mainAnimationCallback -= ReleaseAnimationLock;

                AddAnimationLock();
                rope.mainAnimationCallback += ReleaseAnimationLock;
                rope.Fall();

                while (animationLocks.Count > 0)
                {
                    yield return null;
                }

                rope.mainAnimationCallback -= ReleaseAnimationLock;
                rope.SetCut(true);
            }
            else
            {
                yield return StartCoroutine(_StartConversation(defaultUseComment));
            }
        }
        //Cups
        else if(index == 2)
        {
            CupObjBehavior cup = (CupObjBehavior)targetObj;

            if(!cup.cut)
            {
                PCController.InventoryController.RemoveItemFromInventory(cup.obj);

                switch(cup.content)
                {
                    case CupContent.Empty:
                        PCController.InventoryController.AddItemToInventory(cup.cutCup);
                        break;
                    case CupContent.Water:
                        PCController.InventoryController.AddItemToInventory(cup.cutCuptWithWater);
                        break;
                    case CupContent.Coffee:
                        PCController.InventoryController.AddItemToInventory(cup.cutCupWithCoffee);
                        break;
                }
            }
            else
            {
                yield return StartCoroutine(_StartConversation(defaultUseComment));
            }
        }
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        yield return base.GiveMethod(targetObj);
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, hitObjRelations);

        yield return base.HitMethod(targetObj);
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, drawObjRelations);

        yield return base.DrawMethod(targetObj);
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, throwObjRelations);

        yield return base.ThrowMethod(targetObj);
    }
}
