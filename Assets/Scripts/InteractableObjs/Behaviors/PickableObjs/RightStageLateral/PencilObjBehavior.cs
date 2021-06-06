using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class PencilObjBehavior : PickableObjBehavior
{
    public VIDE_Assign whatDrawComment;
    public VIDE_Assign drawOptionsDialogue;
    public VIDE_Assign artisticalSkillsComment;

    public List<DrawOption> drawOptionList;

    InteractableObjBehavior drawingStandObj;

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        yield return base.UseMethod(targetObj);

        if (index == 1)
        {
            if(artisticalSkillsComment != null)
            {
                yield return StartCoroutine(_StartConversation(artisticalSkillsComment));
            }
        }
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, drawObjRelations);
        //Paper and toilet paper
        if (index == 1)
        {
            if(PCController.pcData.CanDrawAnything())
            {
                drawingStandObj = targetObj;
                yield return StartCoroutine(_StartConversation(drawOptionsDialogue));
                Debug.Log("hola");
            }
            else
            {
                yield return StartCoroutine(_StartConversation(whatDrawComment));
            }
        }
        //Inspiring drawing
        else if(index == 2)
        {
            InspiringDrawingObjBehavior inspiringDrawing = (InspiringDrawingObjBehavior)targetObj;
            VIDE_Assign drawComment = inspiringDrawing.defaultDrawComment;
            yield return StartCoroutine(inspiringDrawing._StartConversation(drawComment));
        }
        //Villain drawing
        else if(index == 3)
        {
            VillainDrawingObjBehavior villainDrawing = (VillainDrawingObjBehavior)targetObj;
            VIDE_Assign drawComment = villainDrawing.defaultDrawComment;
            yield return StartCoroutine(villainDrawing._StartConversation(drawComment));
        }
        else
        {
            yield return base.DrawMethod(targetObj);
        }
    }

    public override void SetPlayerOptions(VD.NodeData data, DialogueUINode node)
    {
        if(VD.assigned == drawOptionsDialogue && data.extraVars.ContainsKey("drawOptions"))
        {
            Dictionary<int, string> optionList = new Dictionary<int, string>();
            for (int i = 0; i < data.comments.Length; i++)
            {
                if(data.extraData[i] == "belindaInspiration" && PCController.pcData.needBelindaInspiration)
                    optionList.Add(i, data.comments[i]);
            }

            node.options = optionList;
        }
        else
        {
            base.SetPlayerOptions(data, node);
        }
    }

    public override IEnumerator _NextDialogue(VIDE_Assign dialogue)
    {
        VD.NodeData data = VD.nodeData;
        if (VD.assigned == drawOptionsDialogue && data.extraVars.ContainsKey("drawOptions"))
        {
            foreach (DrawOption drawOption in drawOptionList)
            {
                if (data.extraData[data.commentIndex] == drawOption.drawOptionName)
                {
                    if (PCController.InventoryController.IsItemInInventory(drawOption.drawResult))
                    {
                        //Already have one
                        VD.SetNode(2);
                    }
                    else
                    {
                        //Done it!
                        VD.SetNode(3);

                        PCController.InventoryController.AddItemToInventory(new List<InteractableObj> { drawOption.drawResult });
                        if (drawingStandObj is PaperObjBehavior paperObj && paperObj.consumable)
                            PCController.InventoryController.RemoveItemFromInventory(drawingStandObj.obj);
                    }
                }
            }
        }
        else
        {
            yield return base._NextDialogue(dialogue);
        }
    }

    public override bool OnChoosePlayerOption(int commentIndex)
    {
        VD.NodeData data = VD.nodeData;
        if (VD.assigned == drawOptionsDialogue && data.extraVars.ContainsKey("drawOptions"))
        {
            data.commentIndex = commentIndex;
            return true;
        }
        else
        {
            return base.OnChoosePlayerOption(commentIndex);
        }
    }
}

[System.Serializable]
public class DrawOption
{
    public string drawOptionName;
    public InteractableObj drawResult;
}
