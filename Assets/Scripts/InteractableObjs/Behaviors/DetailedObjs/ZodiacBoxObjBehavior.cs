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

    List<DropObject> droppedObjs;

    public override IEnumerator _GetPicked()
    {
        yield return StartCoroutine(DropObjs(PlayPickAnimation()));
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
                DialogueUIController.PrepareDialogueUI(this, haveEnoughComment);
                yield return StartCoroutine(_BeginDialogue(haveEnoughComment));
            }
            else if (alreadyHaveThree && alreadyHaveThreeComment != null)
            {
                DialogueUIController.PrepareDialogueUI(this, alreadyHaveThreeComment);
                yield return StartCoroutine(_BeginDialogue(alreadyHaveThreeComment));
            }
            else
            {
                if (selectColorDialogue != null)
                {
                    this.droppedObjs = droppedObjs;

                    DialogueUIController.PrepareDialogueUI(this, selectColorDialogue);
                    yield return StartCoroutine(_BeginDialogue(selectColorDialogue));
                }
                else
                {
                    if (dropObjsComment != null)
                    {
                        DialogueUIController.PrepareDialogueUI(this, dropObjsComment);
                        yield return StartCoroutine(_BeginDialogue(dropObjsComment));
                    }

                    if (characterVisibleToPick)
                    {
                        yield return StartCoroutine(animationMethod);
                    }

                    foreach (DropObject droppedObj in droppedObjs)
                    {
                        droppedObj.quantity--;
                        InventoryController.AddItemToInventory(droppedObj.obj);
                    }
                }
            }
        }
        else
        {
            if (emptyComment != null)
            {
                DialogueUIController.PrepareDialogueUI(this, emptyComment);
                yield return StartCoroutine(_BeginDialogue(emptyComment));
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

    public override void _OnChoosePlayerOption(int commentIndex)
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

            foreach (DropObject droppedObj in droppedObjs)
            {
                droppedObj.quantity--;
                InventoryController.AddItemToInventory((FabricObj)droppedObj.obj, color);
            }
        }
        else
        {
            base._OnChoosePlayerOption(commentIndex);
        }
    }

    public override IEnumerator LookInto()
    {
        yield return base.LookInto();

        DialogueUIController.PrepareDialogueUI(this, lookIntoComment);
        yield return StartCoroutine(_BeginDialogue(lookIntoComment));
    }
}
