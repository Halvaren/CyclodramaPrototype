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
                DialogueUIController.PrepareDialogueUI(this, artisticalSkillsComment);
                yield return StartCoroutine(_BeginDialogue(artisticalSkillsComment));
            }
        }
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        yield return base.DrawMethod(targetObj);
        
        if (index == 1)
        {
            if(PCController.oliverKnowledge.CanDrawAnything())
            {
                drawingStandObj = targetObj;
                DialogueUIController.PrepareDialogueUI(this, drawOptionsDialogue);
                yield return StartCoroutine(_BeginDialogue(drawOptionsDialogue));
            }
            else
            {
                DialogueUIController.PrepareDialogueUI(this, whatDrawComment);
                yield return StartCoroutine(_BeginDialogue(whatDrawComment));
            }
        }
    }

    public override void SetPlayerOptions(VD.NodeData data, DialogueUINode node)
    {
        if(VD.assigned == drawOptionsDialogue && data.extraVars.ContainsKey("drawOptions"))
        {
            List<string> optionList = new List<string>();
            for (int i = 0; i < data.comments.Length; i++)
            {
                if(data.extraData[i] == "belindaInspiration" && PCController.oliverKnowledge.needBelindaInspiration)
                    optionList.Add(data.comments[i]);
            }

            node.options = optionList.ToArray();
        }
        else
        {
            base.SetPlayerOptions(data, node);
        }
    }

    public override void _OnChoosePlayerOption(int commentIndex)
    {
        VD.NodeData data = VD.nodeData;
        if (VD.assigned == drawOptionsDialogue && data.extraVars.ContainsKey("drawOptions"))
        {
            data.commentIndex = commentIndex;

            foreach(DrawOption drawOption in drawOptionList)
            {
                if(data.extraData[data.commentIndex] == drawOption.drawOptionName)
                {
                    if(PCController.InventoryController.IsItemInInventory(drawOption.drawResult))
                    {
                        //Already have one
                        VD.SetNode(2);
                    }
                    else
                    {
                        //Done it!
                        VD.SetNode(3);

                        PCController.InventoryController.AddItemToInventory(drawOption.drawResult);
                        if(drawingStandObj is PaperObjBehavior paperObj && paperObj.consumable)
                            PCController.InventoryController.RemoveItemFromInventory(drawingStandObj.obj);
                    }
                }
            }
        }
        else
        {
            base._OnChoosePlayerOption(commentIndex);
        }
    }
}

[System.Serializable]
public class DrawOption
{
    public string drawOptionName;
    public InteractableObj drawResult;
}
