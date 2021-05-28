using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class BelindaBehavior : NPCBehavior
{
    [Header("State")]
    public bool firstTimeTalk = true;
    public bool givenInspiration = false;
    public bool notanMeasured = false;
    public bool givenFabrics = false;

    [Header("Conversations")]
    public VIDE_Assign firstTimeConv;
    public VIDE_Assign secondTimeConv;
    public VIDE_Assign notanMeasuredConv;

    [Header("Give reactions")]
    public VIDE_Assign giveInspiringDrawingConv;
    public VIDE_Assign giveVillainDrawingConv;
    public VIDE_Assign giveKpopRecordConv;
    public VIDE_Assign giveNotanClothesConv;
    public VIDE_Assign giveOneFabricConv;

    [Header("Node triggers")]
    public string belindaNeedsInspiration = "belindaNeedsInspiration";
    public string setPlayerOptions = "setPlayerOptions";
    public string playerOptions = "playerOptions";
    public string getBackToPlayerOptions = "backToOptions";
    public string fabricCheckingResult = "checkFabrics";
    public string inspirationElection = "chooseInspiration";

    public string givenInspirationTrigger = "givenInspiration";
    public string givenNotanClothesTrigger = "givenNotanClothes";
    public string givenFabricsTrigger = "givenFabrics";

    public string bringInspirationOption = "bringInspiration";
    public string bringNotanMeasuresOption = "bringNotanMeasures";
    public string bringFabricsOption = "bringFabrics";

    public string kPopRecordOption = "kPopRecord";
    public string inspiringDrawingOption = "inspiringDrawing";
    public string villainDrawingOption = "villainDrawing";

    [Header("Node IDs to jump"), Header("First quest")]
    public int noInspirationNoClothesNodeID = 10;
    public int yesInspirtationNoClothesNodeID = 11;
    public int noInspirtationYesClothesNodeID = 12;
    public int yesInspirationYesClothesNodeID = 13;

    [Header("Second quest")]
    public int noThreeFabricsNodeID = 15;
    public int yesThreeFabricsNodeID = 14;

    [Space(15)]
    public int clearedFirstQuestNodeID = 32;
    public int clearedQuestsNodeID = 43;

    [Header("Checking fabrics")]
    public int zeroOutOfThreeNodeID = 36;
    public int oneOutOfThreeNodeID = 35;
    public int twoOutOfThreeNodeID = 33;
    public int threeOutOfThreeNodeID = 30;

    [Header("Quests objs")]
    public InteractableObj kPopRecord;
    public InteractableObj inspiringDrawing;
    public InteractableObj villainDrawing;
    public InteractableObj notanClothes;

    public FabricObj lycraFabric;
    public FabricObj vinylFabric;
    public FabricObj cottonFabric;

    bool executeGivePickAnimations = false;
    bool ExecuteGivePickAnimations
    {
        get
        {
            return executeGivePickAnimations;
        }
        set { executeGivePickAnimations = value; }
    }

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        StartSeated();
    }

    public override IEnumerator TalkMethod()
    {
        if(firstTimeTalk)
        {
            yield return StartCoroutine(_StartConversation(firstTimeConv));

            firstTimeTalk = false;
        }
        else
        {
            yield return StartCoroutine(_StartConversation(secondTimeConv));
        }
    }

    public override IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        SetTalking(true);

        yield return base._BeginDialogue(dialogue);

        SetTalking(false);
    }

    public IEnumerator _GiveObj(InteractableObj obj)
    {
        if (obj == notanClothes && notanMeasured ||
            ((obj == inspiringDrawing || obj == villainDrawing || obj == kPopRecord) && givenInspiration) ||
            (obj is FabricObj && givenFabrics))
        {
            yield return StartCoroutine(PlayGive());

            yield return StartCoroutine(_StartConversation(defaultGiveAnswer));

            PCController.AnimationController.GivenObj();
        }

        else if (obj is FabricObj)
        {
            yield return StartCoroutine(PlayGive());

            yield return StartCoroutine(_StartConversation(giveOneFabricConv));

            PCController.AnimationController.GivenObj();
        }
        else 
        {
            yield return StartCoroutine(PlayGiveAndPick());

            VIDE_Assign conv;
            InteractableObj objToRemove = null;
            if (obj == notanClothes)
            {
                conv = giveNotanClothesConv;
                objToRemove = notanClothes;
            }
            else if(obj == inspiringDrawing)
            {
                conv = giveInspiringDrawingConv;
                objToRemove = inspiringDrawing;
            }
            else if(obj == villainDrawing)
            {
                conv = giveVillainDrawingConv;
                objToRemove = villainDrawing;
            }
            else if(obj == kPopRecord)
            {
                conv = giveKpopRecordConv;
                objToRemove = kPopRecord;
            }
            else
            {
                conv = defaultGiveAnswer;
            }

            if(objToRemove != null)
            {
                PCController.InventoryController.RemoveItemFromInventory(objToRemove);
            }

            yield return StartCoroutine(_StartConversation(conv));
        }
    }

    public override IEnumerator _NextDialogue(VIDE_Assign dialogue)
    {
        if (ExecuteGivePickAnimations)
        {
            ExecuteGivePickAnimations = false;

            DialogueUIController.pausedDialogue = true;

            yield return StartCoroutine(PlayGiveAndPick());

            DialogueUIController.pausedDialogue = false;
        }

        
        VD.NodeData data = VD.nodeData;

        if (data.extraVars.ContainsKey(setPlayerOptions) || data.extraVars.ContainsKey(getBackToPlayerOptions))
        {
            if(!givenInspiration || !notanMeasured)
            {
                bool hasInspiration = !givenInspiration && (PCController.InventoryController.IsItemInInventory(kPopRecord) 
                    || PCController.InventoryController.IsItemInInventory(inspiringDrawing) 
                    || PCController.InventoryController.IsItemInInventory(villainDrawing));

                bool hasNotanClothes = !notanMeasured && PCController.InventoryController.IsItemInInventory(notanClothes);

                if (hasInspiration && hasNotanClothes)
                    VD.SetNode(yesInspirationYesClothesNodeID);
                else if (!hasInspiration && hasNotanClothes)
                    VD.SetNode(noInspirtationYesClothesNodeID);
                else if (hasInspiration && !hasNotanClothes)
                    VD.SetNode(yesInspirtationNoClothesNodeID);
                else
                    VD.SetNode(noInspirationNoClothesNodeID);
            }
            else if(!givenFabrics)
            {
                if (PCController.InventoryController.HasThreeFabrics())
                {
                    VD.SetNode(yesThreeFabricsNodeID);
                }
                else
                    VD.SetNode(noThreeFabricsNodeID);
            }
            else
            {
                VD.SetNode(clearedQuestsNodeID);
            }
        }
        else if(data.extraVars.ContainsKey(fabricCheckingResult))
        {
            List<FabricObjBehavior> fabrics = PCController.InventoryController.GetFabrics();

            if(fabrics.Count == 3)
            {
                int nCorrects = 0;
                foreach(FabricObjBehavior fabric in fabrics)
                {
                    if(fabric.obj == cottonFabric || fabric.obj == vinylFabric || fabric.obj == lycraFabric)
                    {
                        nCorrects++;
                    }
                }

                switch(nCorrects)
                {
                    case 0:
                        VD.SetNode(zeroOutOfThreeNodeID);
                        break;
                    case 1:
                        VD.SetNode(oneOutOfThreeNodeID);
                        break;
                    case 2:
                        VD.SetNode(twoOutOfThreeNodeID);
                        break;
                    case 3:
                        VD.SetNode(threeOutOfThreeNodeID);
                        break;
                }
            }
            else
            {
                Debug.LogError("Incorrect number of fabrics in inventory");
            }

            foreach (FabricObjBehavior fabric in fabrics)
            {
                PCController.InventoryController.RemoveItemFromInventory(fabric.obj);
            }
        }
        else if(data.extraVars.ContainsKey(inspirationElection))
        {
            if (data.extraData[data.commentIndex] == kPopRecordOption)
            {
                PCController.InventoryController.RemoveItemFromInventory(kPopRecord);
            }
            else if(data.extraData[data.commentIndex] == inspiringDrawingOption)
            {
                PCController.InventoryController.RemoveItemFromInventory(inspiringDrawing);
            }
            else if(data.extraData[data.commentIndex] == villainDrawingOption)
            {
                PCController.InventoryController.RemoveItemFromInventory(villainDrawing);
            }                

            yield return base._NextDialogue(dialogue);
        }
        else if(data.extraVars.ContainsKey(givenInspirationTrigger))
        {
            givenInspiration = true;

            if (notanMeasured)
            {
                VD.SetNode(clearedFirstQuestNodeID);
            }
            else
            {
                yield return base._NextDialogue(dialogue);
            }
        }
        else if(data.extraVars.ContainsKey(givenNotanClothesTrigger))
        {
            PCController.InventoryController.RemoveItemFromInventory(notanClothes);
            notanMeasured = true;

            if(givenInspiration)
            {
                VD.SetNode(clearedFirstQuestNodeID);
            }
            else
            {
                yield return base._NextDialogue(dialogue);
            }
        }
        else if(data.extraVars.ContainsKey(givenFabricsTrigger))
        {
            givenFabrics = true;

            yield return base._NextDialogue(dialogue);
        }
        else
        {
            yield return base._NextDialogue(dialogue);
        }
    }

    public override void SetPlayerOptions(VD.NodeData data, DialogueUINode node)
    {
        if(VD.assigned == secondTimeConv && data.extraVars.ContainsKey(inspirationElection))
        {
            List<string> optionList = new List<string>();
            for(int i = 0; i < data.comments.Length; i++)
            {
                if ((data.extraData[i] == kPopRecordOption && PCController.InventoryController.IsItemInInventory(kPopRecord))
                    || (data.extraData[i] == inspiringDrawingOption && PCController.InventoryController.IsItemInInventory(inspiringDrawing))
                    || (data.extraData[i] == villainDrawingOption && PCController.InventoryController.IsItemInInventory(villainDrawing)))

                    optionList.Add(data.comments[i]);
            }

            node.options = optionList.ToArray();
        }
        else
        {
            base.SetPlayerOptions(data, node);
        }
    }

    public override void OnChoosePlayerOption(int commentIndex)
    {
        VD.NodeData data = VD.nodeData;

        bool printOptionNode = true;

        if(VD.assigned == secondTimeConv)
        {
            if(data.extraVars.ContainsKey(playerOptions)
            && (data.extraData[commentIndex] == bringNotanMeasuresOption
                    || data.extraData[commentIndex] == bringFabricsOption))
                ExecuteGivePickAnimations = true;
            else if(data.extraVars.ContainsKey(inspirationElection))
            {
                ExecuteGivePickAnimations = true;
                printOptionNode = false;
            }    
        }

        if (printOptionNode)
            base.OnChoosePlayerOption(commentIndex);
        else
            data.commentIndex = commentIndex;
    }

    public override void OnNodeChange(VD.NodeData data)
    {
        if(data.extraVars.ContainsKey(belindaNeedsInspiration))
        {
            PCController.oliverKnowledge.needBelindaInspiration = true;
        }

        base.OnNodeChange(data);
    }

    IEnumerator PlayGiveAndPick()
    {
        yield return StartCoroutine(PlayGive());

        yield return StartCoroutine(PlayPick());
    }

    IEnumerator PlayGive()
    {
        AddAnimationLock();
        PCController.mainAnimationCallback += ReleaseAnimationLock;
        PCController.AnimationController.GiveObj();

        while (animationLocks.Count > 0)
        {
            yield return null;
        }

        PCController.mainAnimationCallback -= ReleaseAnimationLock;
    }

    IEnumerator PlayPick()
    {
        AddAnimationLock();
        mainAnimationCallback += ReleaseAnimationLock;
        secondAnimationCallback += PCController.AnimationController.GivenObj;
        Pick();

        while (animationLocks.Count > 0)
        {
            yield return null;
        }

        mainAnimationCallback -= ReleaseAnimationLock;
        secondAnimationCallback -= PCController.AnimationController.GivenObj;
    }

    #region Animations

    public void StartSeated()
    {
        Animator.SetTrigger("startSeated");
    }

    public void Sit()
    {
        Animator.SetTrigger("sit");
    }

    public void Pick()
    {
        Animator.SetTrigger("pick");
    }

    public void StandUp()
    {
        Animator.SetTrigger("standUp");
    }

    public void SetWalking(bool value)
    {
        Animator.SetBool("walking", value);
    }

    public void SetTalking(bool value)
    {
        Animator.SetBool("talking", value);
    }

    #endregion
}
