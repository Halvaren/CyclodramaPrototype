using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VIDE_Data;

public class NotanBehavior : NPCBehavior
{
    [Header("State")]
    public bool firstTimeTalk = true;
    public bool convinced = false;
    public bool incidentOcurred = false;

    public bool goneToBeMeasured;

    [Header("Location")]
    public bool inDressingRooms;
    public bool inCostumeWorkshop;

    [Header("Conversations")]
    public VIDE_Assign firstTimeConv;
    public VIDE_Assign secondTimeConv;
    public VIDE_Assign afterConvinceConv;
    public VIDE_Assign afterIncidentConv;

    public VIDE_Assign convinceConv;
    public VIDE_Assign giveDrinkConv;
    public VIDE_Assign throwDrinkConv;

    public Renderer chestRenderer;

    [Header("Conversation variables")]
    public int notCutCupNodeIndex;
    public int cutCupWithCoffeeNodeIndex;
    public int cutCupWithWaterNodeIndex;

    [Header("Other variables")]
    public Transform standUpPoint;
    public SetDoorBehavior doorToCorridor;
    public SetDoorBehavior doorToBathroom;
    public NotanClothesObjBehavior stainedClothes;
    public KPopRecordObjBehavior kpopRecord;

    CupObjBehavior cup;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);

        if (inDressingRooms)
        {
            gameObject.SetActive(!goneToBeMeasured);

            if (!goneToBeMeasured) Sit();
        }

        if (inCostumeWorkshop)
        {
            gameObject.SetActive(goneToBeMeasured);
        }

        stainedClothes.gameObject.SetActive(false);
    }

    public override IEnumerator PlayInitialBehavior()
    {
        if(inCostumeWorkshop && goneToBeMeasured)
        {
            currentSet.GetComponent<SetBehavior>().AddCutsceneLock();

            yield return null;
        }
    }

    public override IEnumerator TalkMethod()
    {
        if(firstTimeTalk)
        {
            yield return StartCoroutine(_StartConversation(firstTimeConv));

            firstTimeTalk = false;
        }
        else if(convinced)
        {
            yield return StartCoroutine(_StartConversation(afterConvinceConv));
        }
        else if (incidentOcurred)
        {
            yield return StartCoroutine(_StartConversation(afterIncidentConv));
        }
        else
        {
            yield return StartCoroutine(_StartConversation(secondTimeConv));
        }
    }

    public IEnumerator ConvinceMethod()
    {
        if(PCController.oliverKnowledge.NotanDontWantToGetMeasured)
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
            incidentOcurred = true;

            kpopRecord.notanPresent = false;

            yield return GoToBathroomAndLeaveClothes();
        }
    }

    public override IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        SetTalking(true);

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

                //Diferenciar entre dos/tres animaciones según el estado del vaso
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

    public override void OnNodeChange(VD.NodeData data)
    {
        if(data.extraVars.ContainsKey("NotanDontWantToGetMeasured"))
        {
            PCController.oliverKnowledge.NotanDontWantToGetMeasured = true;
        }

        base.OnNodeChange(data);
    }

    public IEnumerator GoToBathroomAndLeaveClothes()
    {
        yield return StartCoroutine(StandUpCoroutine());

        yield return StartCoroutine(GoToDoorAndExit(true, doorToBathroom, Vector3.left, true));

        yield return StartCoroutine(doorToBathroom.OpenDoor());

        stainedClothes.gameObject.SetActive(true);
        stainedClothes.Emit();

        while (stainedClothes.inEmission)
        {
            yield return null;
        }

        RecalculateMesh();
        yield return StartCoroutine(doorToBathroom.CloseDoor());

        gameObject.SetActive(false);
    }

    IEnumerator GoToGetMeasured()
    {
        yield return StartCoroutine(StandUpCoroutine());

        yield return StartCoroutine(GoToDoorAndExit(false, doorToCorridor, Vector3.right));

        gameObject.SetActive(false);
    }

    IEnumerator StandUpCoroutine()
    {
        yield return StartCoroutine(MovementController.RotateToDirectionCoroutine(standUpPoint.position - transform.position));

        AddAnimationLock();
        mainAnimationCallback += ReleaseAnimationLock;
        secondAnimationCallback += MoveForwardToStandUp;
        StandUp();

        while (animationLocks.Count > 0)
            yield return null;

        mainAnimationCallback -= ReleaseAnimationLock;
        secondAnimationCallback -= MoveForwardToStandUp;
    }

    IEnumerator GoToDoorAndExit(bool running, SetDoorBehavior door, Vector3 exitDirection, bool closeDoor = false)
    {
        PCController.MovementController.ActivateAgent(false);
        PCController.MovementController.ActivateObstacle(true);
        MovementController.ActivateObstacle(false);
        RecalculateMesh();
        MovementController.ActivateAgent(true);

        SetWalking(true);
        SetRunning(running);
        MovementController.MoveTo(door.interactionPoint.position);

        while (!MovementController.IsOnPoint(door.interactionPoint.position))
        {
            yield return null;
        }

        bool doorClosed = !door.opened;

        if (doorClosed || closeDoor)
        {
            SetWalking(false);
            if(doorClosed) yield return StartCoroutine(door.OpenDoor());

            MovementController.ActivateAgent(false);
            MovementController.ExitScene(3f, exitDirection, running);
            SetWalking(true);

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(door.CloseDoor());

        }
        else
        {
            MovementController.ActivateAgent(false);
            MovementController.ExitScene(3f, exitDirection, running);

            yield return new WaitForSeconds(1f);
        }

        PCController.MovementController.ActivateObstacle(false);
        RecalculateMesh();
        PCController.MovementController.ActivateAgent(true);
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

    public void StandUp()
    {
        Animator.SetTrigger("standUp");
    }

    public void SetWalking(bool value)
    {
        Animator.SetBool("walking", value);
    }

    public void SetRunning(bool value)
    {
        Animator.SetBool("running", value);
    }

    public void SetTalking(bool value)
    {
        Animator.SetBool("talking", value);
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
}
