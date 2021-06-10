using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CupContent
{
    Empty, Water, Coffee
}

public class CupObjBehavior : PickableObjBehavior
{
    public bool cut;
    public CupContent content;

    public InteractableObj basicCup;
    public InteractableObj cupWithCoffee;
    public InteractableObj cupWithWater;
    public InteractableObj cutCup;
    public InteractableObj cutCupWithCoffee;
    public InteractableObj cutCuptWithWater;

    [HideInInspector]
    public Vector3 pointToThrow;

    [Header("Throw settings")]
    public ThrowableCup throwableCupPrefab;
    public Transform throwableCupEmissionPoint;

    public VIDE_Assign alreadyHaveNotanMeasures;

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        //Water dispenser
        if(index == 1)
        {
            WaterDispenserObjBehavior waterDispenser = (WaterDispenserObjBehavior)targetObj;

            if(content != CupContent.Empty)
            {
                yield return StartCoroutine(_StartConversation(defaultUseComment));
            }
            else
            {
                yield return StartCoroutine(waterDispenser._FillCup(this));
            }
        }
        //CoffeeMachine
        else if(index == 2)
        {
            CoffeeMachineObjBehavior coffeeMachine = (CoffeeMachineObjBehavior)targetObj;

            if(content != CupContent.Empty)
            {
                yield return StartCoroutine(_StartConversation(defaultUseComment));
            }
            else
            {
                yield return StartCoroutine(coffeeMachine._FillCup(this));
            }
        }
        else
            yield return base.UseMethod(targetObj);
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        //Notan
        if(index == 1)
        {
            NotanBehavior notan = (NotanBehavior)targetObj;
            if(notan.incidentOccurred)
            {
                yield return StartCoroutine(notan._StartConversation(notan.afterIncidentConv));
            }
            else
            {
                if (content != CupContent.Empty)
                {
                    yield return StartCoroutine(notan._Drink(this));
                    PCController.InventoryController.RemoveItemFromInventory(obj);
                }
                else
                {
                    yield return StartCoroutine(notan._StartConversation(notan.defaultGiveAnswer));
                }
            }            
        }
        //Anyone
        else if(index == 9)
        {
            NPCBehavior npc = (NPCBehavior)targetObj;

            yield return StartCoroutine(npc._StartConversation(npc.defaultGiveAnswer));
        }
        else
        {
            yield return base.GiveMethod(targetObj);
        }
    }

    bool gotTarget = false;

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, throwObjRelations);

        //Notan
        if(index == 1)
        {
            NotanBehavior notan = (NotanBehavior)targetObj;
            if (content != CupContent.Empty && !notan.incidentOccurred)
            {
                pointToThrow = notan.GetPointToThrow();
                PCController.mainAnimationCallback += ThrowCup;
                PCController.AnimationController.ThrowCup();

                gotTarget = false;

                while (!gotTarget)
                {
                    yield return null;
                }

                notan.incidentOccurred = true;

                PCController.mainAnimationCallback -= ThrowCup;

                yield return StartCoroutine(notan._StartConversation(notan.throwDrinkConv));

                notan.kpopRecord.notanPresent = false;

                yield return StartCoroutine(notan._GoToBathroomAndLeaveClothes());

                PCController.InventoryController.RemoveItemFromInventory(obj);
            }
            else
            {
                yield return StartCoroutine(_StartConversation(defaultThrowComment));
            }
        }
        else
        {
            yield return base.ThrowMethod(targetObj);
        }
    }
    public void ThrowCup()
    {
        ThrowableCup throwableCup = Instantiate(throwableCupPrefab, throwableCupEmissionPoint.position, throwableCupEmissionPoint.rotation);
        throwableCup.Emit((pointToThrow - throwableCup.transform.position).normalized);
        throwableCup.gotTargetAction = GotTarget;
    }

    public void GotTarget()
    {
        gotTarget = true;
    }
}
