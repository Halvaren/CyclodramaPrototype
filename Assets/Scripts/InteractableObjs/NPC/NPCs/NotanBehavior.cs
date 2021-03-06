using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class NotanBehavior : NPCBehavior
{
    #region General variables

    [HideInInspector]
    public bool goneToBeMeasured;

    public AudioClip introNotanMusic;

    #endregion

    #region Dressing room variables

    [HideInInspector]
    public bool convinced = false;
    [HideInInspector]
    public bool incidentOccurred = false;

    [HideInInspector]
    public VIDE_Assign firstTimeConv;
    [HideInInspector]
    public VIDE_Assign secondTimeConv;
    [HideInInspector]
    public VIDE_Assign afterConvinceConv;
    [HideInInspector]
    public VIDE_Assign afterIncidentConv;

    [HideInInspector]
    public VIDE_Assign convinceConv;
    [HideInInspector]
    public VIDE_Assign giveDrinkConv;
    [HideInInspector]
    public VIDE_Assign throwDrinkConv;

    [HideInInspector]
    public Renderer chestRenderer;

    [HideInInspector]
    public int notCutCupNodeIndex;
    [HideInInspector]
    public int cutCupWithCoffeeNodeIndex;
    [HideInInspector]
    public int cutCupWithWaterNodeIndex;

    [HideInInspector]
    public string playerOptionsTrigger = "playerOptions";
    [HideInInspector]
    public string needMeasuresOption = "needMeasures";

    [HideInInspector]
    public Transform standUpPoint;
    [HideInInspector]
    public SetDoorBehavior doorToCorridor1;
    [HideInInspector]
    public SetDoorBehavior doorToBathroom;
    [HideInInspector]
    public NotanClothesObjBehavior stainedClothes;
    [HideInInspector]
    public KPopRecordObjBehavior kpopRecord;

    #endregion

    #region Costume workshop

    [HideInInspector]
    public Transform moveAsidePlayerPosition;
    [HideInInspector]
    public SetDoorBehavior doorToCorridor2;
    [HideInInspector]
    public bool canLeave = false;

    #endregion

    CupObjBehavior cup;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        if (location == SetLocation.DressingRoom1)
        {
            bool isInScene = !goneToBeMeasured && (!incidentOccurred || (incidentOccurred && !kpopRecord.inScene));

            gameObject.SetActive(isInScene);

            if (isInScene) Sit();

            if (incidentOccurred && stainedClothes.inScene)
            {
                stainedClothes.StartInEmittedPosition();
            }
            else
            {
                stainedClothes.gameObject.SetActive(false);
            }
            kpopRecord.notanPresent = isInScene;
            doorToBathroom.locked = !isInScene;
        }

        if (location == SetLocation.CostumeWorkshop)
        {
            gameObject.SetActive(goneToBeMeasured);
        }
    }

    public override IEnumerator _PlayInitialBehavior()
    {
        movementUpdate = true;
        if(location == SetLocation.CostumeWorkshop && goneToBeMeasured)
        {
            while(!canLeave)
            {
                yield return null;
            }

            Vector3 lookDirection = doorToCorridor2.transform.position - transform.position;
            lookDirection.y = 0f;
            yield return StartCoroutine(MovementController.RotateToDirectionCoroutine(lookDirection));

            yield return StartCoroutine(PCController.MovementController.MoveAndRotateToDirection(moveAsidePlayerPosition.position, Vector3.back));

            yield return StartCoroutine(GoToDoorAndExit(false, doorToCorridor2, Vector3.forward, false, 1.5f));

            goneToBeMeasured = false;
        }

        currentSet.GetComponent<SetBehavior>().ReleaseCutsceneLock();
    }

    public override IEnumerator TalkMethod()
    {
        if (incidentOccurred)
        {
            yield return StartCoroutine(_StartConversation(afterIncidentConv));
        }
        else if (!convinced && firstTimeTalk)
        {
            AudioSource notanThemeSource = AudioManager.PlaySound(introNotanMusic, SoundType.ForegroundMusic);
            yield return StartCoroutine(_StartConversation(firstTimeConv));
            AudioManager.FadeOutSound(notanThemeSource, 3f);

            firstTimeTalk = false;
        }
        else if(convinced && firstTimeTalk)
        {
            yield return StartCoroutine(_StartConversation(afterConvinceConv));

            firstTimeTalk = false;
        }
        else
        {
            yield return StartCoroutine(_StartConversation(secondTimeConv));
        }
    }

    public IEnumerator ConvinceMethod()
    {
        if(incidentOccurred)
        {
            yield return StartCoroutine(_StartConversation(afterIncidentConv));
        }
        else
        {
            if (PCController.pcData.NotanDontWantToGetMeasured && !PCController.pcData.gotNotanMeasurements)
            {
                yield return StartCoroutine(_StartConversation(convinceConv));

                convinced = true;

                kpopRecord.notanPresent = false;

                yield return StartCoroutine(GoToGetMeasured());
            }
            else
            {
                yield return StartCoroutine(_StartConversation(defaultConvinceAnswer));
            }
        }
    }

    public IEnumerator _Drink(CupObjBehavior cup)
    {
        this.cup = cup;

        AddAnimationLock();
        PCController.mainAnimationCallback += ReleaseAnimationLock;
        PCController.AnimationController.GiveObj();

        while(animationLocks.Count > 0)
        {
            yield return null;
        }

        PCController.mainAnimationCallback -= ReleaseAnimationLock;

        AddAnimationLock();
        mainAnimationCallback += ReleaseAnimationLock;
        secondAnimationCallback += PCController.AnimationController.GivenObj;
        PickDrink();

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(_StartConversation(giveDrinkConv));

        if(cup.cut && cup.content == CupContent.Coffee)
        {
            incidentOccurred = true;

            kpopRecord.notanPresent = false;

            yield return _GoToBathroomAndLeaveClothes();
        }
    }

    public override IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        yield return base._BeginDialogue(dialogue);

        SetTalking(false);
    }

    public override IEnumerator _NextDialogue(VIDE_Assign dialogue)
    {
        if(dialogue == giveDrinkConv)
        {
            VD.NodeData data = VD.nodeData;
            if(data.extraVars.ContainsKey("drink"))
            {
                DialogueUIController.pausedDialogue = true;

                //Waiting PickDrink animation to be done
                while(animationLocks.Count > 0)
                {
                    yield return null;
                }
                mainAnimationCallback -= ReleaseAnimationLock;
                secondAnimationCallback -= PCController.AnimationController.GivenObj;

                //Diferenciar entre dos/tres animaciones seg?n el estado del vaso
                AddAnimationLock();
                mainAnimationCallback += ReleaseAnimationLock;
                Drink();

                while(animationLocks.Count > 0)
                {
                    yield return null;
                }

                mainAnimationCallback -= ReleaseAnimationLock;

                DialogueUIController.pausedDialogue = false;

                if (cup.cut)
                {
                    if(cup.content == CupContent.Coffee)
                    {
                        VD.SetNode(cutCupWithCoffeeNodeIndex);
                    }
                    else if(cup.content == CupContent.Water)
                    {
                        VD.SetNode(cutCupWithWaterNodeIndex);
                    }
                }
                else
                {
                    VD.SetNode(notCutCupNodeIndex);
                }
            }
            else
            {
                yield return base._NextDialogue(dialogue);
            }
        }
        else
        {
            yield return base._NextDialogue(dialogue);
        }
    }

    public override void SetPlayerOptions(VD.NodeData data, DialogueUINode node)
    {
        if (data.extraVars.ContainsKey(playerOptionsTrigger))
        {
            Dictionary<int, string> optionList = new Dictionary<int, string>();
            for (int i = 0; i < data.comments.Length; i++)
            {
                if (data.extraData[i] == needMeasuresOption && PCController.pcData.gotNotanMeasurements)
                    continue;

                optionList.Add(i, data.comments[i]);
            }

            node.options = optionList;
        }
        else
        {
            base.SetPlayerOptions(data, node);
        }
    }

    public override void OnNodeChange(VD.NodeData data)
    {
        SetTalking(data.tag == obj.name);

        if (data.extraVars.ContainsKey("NotanDontWantToGetMeasured"))
        {
            PCController.pcData.NotanDontWantToGetMeasured = true;
        }

        base.OnNodeChange(data);
    }

    public IEnumerator _GoToBathroomAndLeaveClothes()
    {
        yield return StartCoroutine(StandUpCoroutine());

        yield return StartCoroutine(GoToDoorAndExit(true, doorToBathroom, Vector3.left, true));

        if(stainedClothes.inScene)
        {
            yield return StartCoroutine(doorToBathroom.OpenDoor());

            stainedClothes.gameObject.SetActive(true);
            stainedClothes.Emit();

            while (stainedClothes.inEmission)
            {
                yield return null;
            }

            RecalculateMesh();

            yield return StartCoroutine(doorToBathroom.CloseDoor());
        }

        doorToBathroom.locked = true;

        StartCoroutine(DisappearAfterTime(0.5f));
    }

    IEnumerator GoToGetMeasured()
    {
        yield return StartCoroutine(StandUpCoroutine());

        yield return StartCoroutine(GoToDoorAndExit(false, doorToCorridor1, Vector3.right, false, 0.5f));

        goneToBeMeasured = true;
    }

    IEnumerator StandUpCoroutine()
    {
        yield return StartCoroutine(MovementController.RotateToDirectionCoroutine(standUpPoint.position - transform.position));

        AddAnimationLock();
        mainAnimationCallback += ReleaseAnimationLock;
        secondAnimationCallback += MoveForwardToStandUp;
        StandUp(SeatType.Chair);

        while (animationLocks.Count > 0)
            yield return null;

        mainAnimationCallback -= ReleaseAnimationLock;
        secondAnimationCallback -= MoveForwardToStandUp;
    }

    void MoveForwardToStandUp()
    {
        StartCoroutine(MovementController.MoveToPointCoroutine(standUpPoint.position, MovementController.turnSmoothTime2));
    }

    public Vector3 GetPointToThrow()
    {
        return chestRenderer.bounds.center;
    }

    #region Animations

    public void Sit()
    {
        Animator.SetTrigger("sit");
    }

    public void StandUp(SeatType seatType)
    {
        SetStandUpSound(seatType);

        Animator.SetTrigger("standUp");
    }

    public void PickDrink()
    {
        Animator.SetTrigger("pickDrink");
    }

    public void Drink()
    {
        Animator.SetTrigger("drink");
    }

    public void ThrownCup()
    {
        Animator.SetTrigger("thrownCup");
    }

    #endregion

    #region Data methods

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is NotanData notanData)
        {
            goneToBeMeasured = notanData.goneToBeMeasured;
            convinced = notanData.convinced;
            incidentOccurred = notanData.incidentOccurred;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new NotanData(inScene, firstTimeTalk, goneToBeMeasured, convinced, incidentOccurred);
    }

    #endregion
}
