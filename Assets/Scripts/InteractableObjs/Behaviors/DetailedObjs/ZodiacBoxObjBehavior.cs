using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;
using System;

public class ZodiacBoxObjBehavior : DetailedEmitterObjBehavior
{
    public DropObject fabricDropObj;

    public VIDE_Assign alreadyHaveThreeComment;
    public VIDE_Assign selectColorDialogue;
    public VIDE_Assign lookIntoComment;

    public VIDE_Assign cantPickComment;

    List<DropObject> droppedObjs;

    public override IEnumerator _GetPicked()
    {
        if(PCController.pcData.givenBelindaInspiration && PCController.pcData.gotNotanMeasurements && !PCController.pcData.givenBelindaFabrics)
        {
            yield return StartCoroutine(DropObjs(PlayPickAnimation()));
        }
        else
        {
            yield return StartCoroutine(_StartConversation(cantPickComment));
        }
    }

    public override IEnumerator _GetStolen()
    {
        if (PCController.pcData.givenBelindaInspiration && PCController.pcData.gotNotanMeasurements && !PCController.pcData.givenBelindaFabrics)
        {
            yield return StartCoroutine(DropObjs(PlayStealAnimation()));
        }
        else
        {
            yield return StartCoroutine(_StartConversation(cantPickComment));
        }
    }

    protected override IEnumerator DropObjs(IEnumerator animationMethod)
    {
        bool theresSomething = false;
        bool haveEnough = false;
        bool alreadyHaveThree = false;

        List<DropObject> droppedObjs = GetDropObjects(ref theresSomething, ref haveEnough, ref alreadyHaveThree);

        if (theresSomething)
        {
            if (haveEnough && haveEnoughComment != null)
            {
                yield return StartCoroutine(_StartConversation(haveEnoughComment));
            }
            else if (alreadyHaveThree && alreadyHaveThreeComment != null)
            {
                yield return StartCoroutine(_StartConversation(alreadyHaveThreeComment));
            }
            else
            {
                if (selectColorDialogue != null)
                {
                    this.droppedObjs = droppedObjs;

                    yield return StartCoroutine(_StartConversation(selectColorDialogue));
                }
                else
                {
                    if (dropObjsComment != null)
                    {
                        yield return StartCoroutine(_StartConversation(dropObjsComment));
                    }

                    if (characterVisibleToPick)
                    {
                        yield return StartCoroutine(animationMethod);
                    }

                    List<InteractableObj> objsToAdd = new List<InteractableObj>();

                    foreach (DropObject droppedObj in droppedObjs)
                    {
                        droppedObj.quantity--;
                        objsToAdd.Add(droppedObj.obj);
                    }

                    InventoryController.AddItemToInventory(objsToAdd);
                }
            }
        }
        else
        {
            if (emptyComment != null)
            {
                yield return StartCoroutine(_StartConversation(emptyComment));
            }
        }
    }

    public List<DropObject> GetDropObjects(ref bool theresSomething, ref bool haveEnough, ref bool alreadyHaveThree)
    {
        List<DropObject> result = new List<DropObject>();
        if (fabricDropObj.quantity != 0)
        {
            theresSomething = true;
            haveEnough = false;

            int objsInInventory = 0;
            foreach (InteractableObj banObj in fabricDropObj.banObjs)
            {
                if (InventoryController.IsItemInInventory(banObj))
                {
                    if (banObj == fabricDropObj.obj)
                    {
                        haveEnough = true;
                        break;
                    }
                    objsInInventory++;
                }

                if (objsInInventory >= 3)
                {
                    alreadyHaveThree = true;
                    break;
                }
            }

            if(!alreadyHaveThree && !haveEnough)
            {
                result.Add(fabricDropObj);
            }
        }

        return result;
    }

    public override IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        yield return base._BeginDialogue(dialogue);

        PCController.instance.mainAnimationCallback -= ReleaseAnimationLock;
    }

    public override bool OnChoosePlayerOption(int commentIndex)
    {
        VD.NodeData data = VD.nodeData;
        if(VD.assigned == selectColorDialogue && data.extraVars.ContainsKey("selectingColor"))
        {
            data.commentIndex = commentIndex;

            FabricColor color;
            Enum.TryParse(data.extraData[data.commentIndex], out color);
            
            if (characterVisibleToPick)
            {
                AddAnimationLock();
                PCController.instance.mainAnimationCallback += ReleaseAnimationLock;
                PCController.instance.AnimationController.PickObject(objHeight, objWeight);
            }

            List<InteractableObj> objsToAdd = new List<InteractableObj>();

            foreach (DropObject droppedObj in droppedObjs)
            {
                droppedObj.quantity--;
                ((FabricObj)droppedObj.obj).color = color;
                objsToAdd.Add(droppedObj.obj);
            }

            InventoryController.AddItemToInventory(objsToAdd);

            return true;
        }
        else
        {
            return base.OnChoosePlayerOption(commentIndex);
        }
    }

    public override IEnumerator LookInto()
    {
        yield return base.LookInto();

        yield return StartCoroutine(_StartConversation(lookIntoComment));
    }
}
