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

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        //Water dispenser
        if(index == 1)
        {
            WaterDispenserObjBehavior waterDispenser = (WaterDispenserObjBehavior)targetObj;

            if(content != CupContent.Empty)
            {
                DialogueUIController.PrepareDialogueUI(this, defaultUseComment);
                yield return StartCoroutine(_BeginDialogue(defaultUseComment));
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
                DialogueUIController.PrepareDialogueUI(this, defaultUseComment);
                yield return StartCoroutine(_BeginDialogue(defaultUseComment));
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
            if(notan.incidentOcurred)
            {
                DialogueUIController.PrepareDialogueUI(notan, notan.afterIncidentConv);
                yield return StartCoroutine(notan._BeginDialogue(notan.afterIncidentConv));
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
                    DialogueUIController.PrepareDialogueUI(notan, notan.defaultConvinceAnswer);
                    yield return StartCoroutine(notan._BeginDialogue(notan.defaultConvinceAnswer));
                }
            }            
        }
        //Anyone
        else if(index == 9)
        {
            NPCBehavior npc = (NPCBehavior)targetObj;

            DialogueUIController.PrepareDialogueUI(npc, npc.defaultConvinceAnswer);
            yield return StartCoroutine(npc._BeginDialogue(npc.defaultConvinceAnswer));
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
            if (content != CupContent.Empty && !notan.incidentOcurred)
            {
                pointToThrow = notan.GetPointToThrow();
                PCController.mainAnimationCallback += ThrowCup;
                PCController.AnimationController.ThrowCup();

                gotTarget = false;

                while(!gotTarget)
                {
                    yield return null;
                }

                notan.incidentOcurred = true;

                PCController.mainAnimationCallback -= ThrowCup;

                DialogueUIController.PrepareDialogueUI(this, notan.throwDrinkConv);
                yield return StartCoroutine(notan._BeginDialogue(notan.throwDrinkConv));

                notan.kpopRecord.notanPresent = false;

                yield return StartCoroutine(notan.GoToBathroomAndLeaveClothes());

                PCController.InventoryController.RemoveItemFromInventory(obj);
            }
            else
            {
                DialogueUIController.PrepareDialogueUI(this, defaultThrowComment);
                yield return StartCoroutine(_BeginDialogue(defaultThrowComment));
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
