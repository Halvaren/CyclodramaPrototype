using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIDE_Data;
using TMPro;
using UnityEngine.EventSystems;

public class DialogueUIController : MonoBehaviour
{
    public GameObject dialogueContainer;

    private RectTransform dialogueContainerRectTransform;
    public RectTransform DialogueContainerRectTransform
    {
        get
        {
            if (dialogueContainerRectTransform == null) dialogueContainerRectTransform = dialogueContainer.GetComponent<RectTransform>();
            return dialogueContainerRectTransform;
        }
    }

    public GameObject NPC_Container;
    public GameObject playerContainer;

    public TextMeshProUGUI NPC_Text;
    public TextMeshProUGUI NPC_Label;
    public Image NPCSprite;
    public DialogueUIPlayerOption playerOptionPrefab;
    public Image playerSprite;
    public TextMeshProUGUI playerLabel;

    public RectTransform showingPosition;
    public RectTransform unshowingPosition;

    public PCController PlayerCharacter
    {
        get { return PCController.instance; }
    }

    bool animatingText = false;
    bool autoNextDialogue = false;

    private List<DialogueUIPlayerOption> currentChoices = new List<DialogueUIPlayerOption>();
    int currentChoice = 0;

    DialogueUINode currentNode;

    IEnumerator NPC_TextAnimator;

    InteractableObjBehavior currentBehavior;
    VIDE_Assign currentDialogue;

    public GeneralUIController GeneralUI
    {
        get { return GeneralUIController.Instance; }
    }

    public bool showingDialogue
    {
        get { return dialogueContainer.activeSelf; }
    }

    public void ShowUnshow(bool show)
    {
        if (show && !showingDialogue)
        {
            StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.25f, show));
        }
        else if (!show && showingDialogue)
        {
            StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.25f, show));
        }
    }

    IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show)
    {
        if (show)
        {
            dialogueContainer.SetActive(true);
        }

        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            DialogueContainerRectTransform.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        DialogueContainerRectTransform.position = finalPos;

        if (!show)
        {
            dialogueContainer.SetActive(false);
        }
    }

    public void PrepareDialogueUI(InteractableObjBehavior behavior, VIDE_Assign dialogue)
    {
        currentBehavior = behavior;
        currentDialogue = dialogue;

        ShowUnshow(true);

        NPC_Text.text = "";
        NPC_Label.text = "";
        playerLabel.text = "";
    }

    public void CallNext()
    {
        if (animatingText) { if (!autoNextDialogue) CutTextAnim();  return; }

        currentBehavior.NextDialogue(currentDialogue);
    }

    private void Update()
    {
        if(VD.isActive)
        {
            if(currentNode != null)
            {
                if (currentNode.isPlayer)
                {
                    int previousChoice = currentChoice;

                    if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (currentChoice == -1) currentChoice = 0;
                        else
                        {
                            currentChoice++;
                            if (currentChoice >= currentChoices.Count)
                                currentChoice = 0;
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (currentChoice == -1) currentChoice = 0;
                        else
                        {
                            currentChoice--;
                            if (currentChoice < 0)
                                currentChoice = currentChoices.Count - 1;
                        }
                    }

                    if (previousChoice != currentChoice)
                    {
                        if (previousChoice >= 0 && previousChoice < currentChoices.Count) currentChoices[previousChoice].Highlight(false);
                        if (currentChoice >= 0 && currentChoice < currentChoices.Count) currentChoices[currentChoice].Highlight(true);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (currentNode.isPlayer)
                    {
                        currentBehavior.OnChoosePlayerOption(currentChoice == -1 ? 0 : currentChoice);
                    }
                    else
                    {
                        CallNext();
                    }
                }
            }

            if(Input.GetKeyDown(KeyCode.Return))
            {
                if (!autoNextDialogue && !animatingText) CallNext();
                autoNextDialogue = !autoNextDialogue;
            }
        }
    }

    public void OnHoverPlayerOption(int index)
    {
        int previousChoice = currentChoice;
        currentChoice = index;

        if (previousChoice != currentChoice)
        {
            if (previousChoice >= 0 && previousChoice < currentChoices.Count) currentChoices[previousChoice].Highlight(false);
            if (currentChoice >= 0 && currentChoice < currentChoices.Count) currentChoices[currentChoice].Highlight(true);
        }
    }

    public void OnClickPlayerOption(int index)
    {
        if (VD.isActive)
        {
            currentBehavior.OnChoosePlayerOption(index == -1 ? 0 : index);
            CallNext();
        }
    }

    public void UpdateUI(DialogueUINode node)
    {
        currentNode = node;

        foreach (DialogueUIPlayerOption op in currentChoices)
            Destroy(op.gameObject);

        currentChoices = new List<DialogueUIPlayerOption>();
        NPC_Text.text = "";
        NPC_Container.SetActive(false);
        playerContainer.SetActive(false);
        playerSprite.sprite = null;
        NPCSprite.sprite = null;

        if(node.isPlayer)
        {
            playerSprite.sprite = node.sprite;

            SetOptions(node.options);

            playerLabel.text = node.tag;

            playerContainer.SetActive(true);
        }
        else
        {
            NPCSprite.sprite = node.sprite;

            NPC_TextAnimator = DrawText(node.message, 0.02f);
            StartCoroutine(NPC_TextAnimator);

            NPC_Label.text = node.tag;

            NPC_Container.SetActive(true);
        }
    }

    public void SetOptions(string[] options)
    {
        for(int i = 0; i < options.Length; i++)
        {
            DialogueUIPlayerOption newOp = Instantiate(playerOptionPrefab.gameObject, playerOptionPrefab.transform.position, Quaternion.identity).GetComponent<DialogueUIPlayerOption>();
            newOp.transform.SetParent(playerOptionPrefab.transform.parent, true);

            RectTransform newOpRT = newOp.GetComponent<RectTransform>();
            RectTransform playerChoicePrefabRT = playerOptionPrefab.GetComponent<RectTransform>();

            RectTransformExtensions.SetLeft(newOpRT, RectTransformExtensions.GetLeft(playerChoicePrefabRT));
            RectTransformExtensions.SetRight(newOpRT, RectTransformExtensions.GetRight(playerChoicePrefabRT));

            newOpRT.anchoredPosition += new Vector2(0, - (110 * i));
            newOpRT.localScale = new Vector3(1, 1, 1);
            newOp.playerOptionText.text = options[i];
            newOp.gameObject.SetActive(true);

            newOp.dialogueUIController = this;
            newOp.optionIndex = i;

            currentChoices.Add(newOp);
        }

        currentChoice = 0;
        currentChoices[currentChoice].Highlight(true);
    }

    public void EndDialogue()
    {
        StopAllCoroutines();

        ShowUnshow(false);

        currentBehavior = null;
        currentDialogue = null;
        currentNode = null;
    }

    IEnumerator DrawText(string text, float time)
    {
        animatingText = true;

        string[] words = text.Split(' ');

        int letters = 0;

        for(int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (i != words.Length - 1) word += " ";

            string previousText = NPC_Text.text;

            float lastHeight = NPC_Text.preferredHeight;
            NPC_Text.text += word;

            for(int j = 0; j < word.Length; j++)
            {
                NPC_Text.text = previousText + word.Substring(0, j + 1);
                letters++;
                yield return new WaitForSeconds(time);
            }
        }
        NPC_Text.text = text;
        animatingText = false;        

        if(autoNextDialogue)
        {
            float waitingTime = time * letters;
            yield return new WaitForSeconds(waitingTime > 1f ? waitingTime : 1f);

            CallNext();
        }
    }

    void CutTextAnim()
    {
        StopCoroutine(NPC_TextAnimator);
        NPC_Text.text = currentNode.message;
        animatingText = false;
    }
}

public class DialogueUINode
{
    public string tag;
    public bool isPlayer;
    public string message;
    public string[] options;
    public Sprite sprite;
}
