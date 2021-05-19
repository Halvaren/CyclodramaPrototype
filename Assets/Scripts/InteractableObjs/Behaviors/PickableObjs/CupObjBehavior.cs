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
            if(content != CupContent.Empty)
            {
                DialogueUIController.PrepareDialogueUI(this, defaultUseComment);
                yield return StartCoroutine(_BeginDialogue(defaultUseComment));
            }
            else
            {
                //Llamar a un método de la CoffeeMachine
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
            if(content == CupContent.Water)
            {
                if(cut)
                {
                    //Se le da el vaso cortado
                }
                else
                {
                    //Se le da el vaso normal
                }
            }
            else if(content == CupContent.Coffee)
            {
                if(cut)
                {
                    //Se le da el vaso cortado
                }
                else
                {
                    //Se le da el vaso normal
                }
            }
            else
            {
                //Frase por defecto de Notan
            }
        }
        else
        {
            yield return base.GiveMethod(targetObj);
        }
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, throwObjRelations);

        //Notan
        if(index == 1)
        {
            if (content == CupContent.Water)
            {
                //Se le tira el vaso con agua a este hombre
            }
            else if(content == CupContent.Coffee)
            {
                //Se le tira el vaso con café a este hombre
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
}
