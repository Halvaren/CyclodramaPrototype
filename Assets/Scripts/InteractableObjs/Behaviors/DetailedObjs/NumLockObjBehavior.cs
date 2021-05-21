using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NumLockWheel
{
    Left, Center, Right
}

public class NumLockObjBehavior : DetailedObjBehavior
{
    int leftNumber = 0;
    int centerNumber = 0;
    int rightNumber = 0;

    public int correctCombination;
    int correctCombFirstDigit = -1;
    int correctCombSecondDigit = -1;
    int correctCombThirdDigit = -1;

    public float zeroOffsetAngle = -18f;
    public float incrementAngle = -36f;

    public GameObject leftWheel;
    public GameObject centerWheel;
    public GameObject rightWheel;

    public VIDE_Assign cantOpenComment;
    public VIDE_Assign openComment;
    public VIDE_Assign inspectZeroWheelCorrectComment;
    public VIDE_Assign inspectOneWheelCorrectComment;
    public VIDE_Assign inspectTwoWheelCorrectComment;
    public VIDE_Assign inspectThreeWheelCorrectComment;

    Coroutine turnCoroutine;

    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    public void TurnWheel(NumLockWheel wheel, bool up)
    {
        switch(wheel)
        {
            case NumLockWheel.Left:
                if(TurnWheel(leftWheel, up))
                {
                    if (up) leftNumber++;
                    else leftNumber--;

                    if (leftNumber > 9) leftNumber = 0;
                    if (leftNumber < 0) leftNumber = 9;
                }
                break;
            case NumLockWheel.Center:
                if(TurnWheel(centerWheel, up))
                {
                    if (up) centerNumber++;
                    else centerNumber--;

                    if (centerNumber > 9) centerNumber = 0;
                    if (centerNumber < 0) centerNumber = 9;
                }
                break;
            case NumLockWheel.Right:
                if(TurnWheel(rightWheel, up))
                {
                    if (up) rightNumber++;
                    else rightNumber--;

                    if (rightNumber > 9) rightNumber = 0;
                    if (rightNumber < 0) rightNumber = 9;
                }
                break;
        }
    }

    public bool TurnWheel(GameObject wheel, bool up)
    {
        if(turnCoroutine == null)
        {
            Quaternion initialRotation = wheel.transform.rotation;
            Vector3 incrementAngles = new Vector3(incrementAngle * ((up) ? 1 : -1), 0f, 0f);
            Quaternion finalRotation = wheel.transform.rotation * Quaternion.Euler(incrementAngles);
            turnCoroutine = StartCoroutine(TurnWheelCoroutine(wheel.transform, initialRotation, finalRotation, 0.25f));
            return true;
        }
        return false;
    }

    IEnumerator TurnWheelCoroutine(Transform wheel, Quaternion initialRotation, Quaternion finalRotation, float time)
    {
        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            wheel.rotation = Quaternion.Slerp(initialRotation, finalRotation, elapsedTime / time);

            yield return null;
        }
        wheel.rotation = finalRotation;

        turnCoroutine = null;
    }

    public void Open()
    {
        int currentCombination = leftNumber * 100 + centerNumber * 10 + rightNumber;
        if (currentCombination == correctCombination)
        {
            StartCoroutine(ShowComment(openComment, true));
            PlayOpenAnimation();
        }
        else
        {
            StartCoroutine(ShowComment(cantOpenComment));
            PlayShakeAnimation();
        }
    }

    public void Inspect()
    {
        if(correctCombFirstDigit == -1)
        {
            correctCombFirstDigit = correctCombination / 100;
            correctCombSecondDigit = (correctCombination - correctCombFirstDigit * 100) / 10;
            correctCombThirdDigit = correctCombination - correctCombFirstDigit * 100 - correctCombSecondDigit * 10;

            Debug.Log(correctCombFirstDigit + " " + correctCombSecondDigit + " " + correctCombThirdDigit);
        }

        int nCorrect = 0;
        if (leftNumber == correctCombFirstDigit) nCorrect++;
        if (centerNumber == correctCombSecondDigit) nCorrect++;
        if (rightNumber == correctCombThirdDigit) nCorrect++;

        switch(nCorrect)
        {
            case 0:
                StartCoroutine(ShowComment(inspectZeroWheelCorrectComment));
                break;
            case 1:
                StartCoroutine(ShowComment(inspectOneWheelCorrectComment));
                break;
            case 2:
                StartCoroutine(ShowComment(inspectTwoWheelCorrectComment));
                break;
            case 3:
                StartCoroutine(ShowComment(inspectThreeWheelCorrectComment));
                break;
        }
    }

    IEnumerator ShowComment(VIDE_Assign comment, bool open = false)
    {
        BlockInput(true);

        DialogueUIController.PrepareDialogueUI(this, comment);
        yield return StartCoroutine(_BeginDialogue(comment));

        BlockInput(false);

        if(open)
        {
            GetBack();
            gameObject.SetActive(false);
            inScene = false;
        }
    }

    public void PlayOpenAnimation()
    {
        Animator.SetTrigger("open");
    }

    public void PlayShakeAnimation()
    {
        Animator.SetTrigger("shake");
    }
}
