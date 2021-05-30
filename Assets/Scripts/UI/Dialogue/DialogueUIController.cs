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
    public bool pausedDialogue = false;

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

    public void HideUnhide(bool hide)
    {
        pausedDialogue = hide;
        if (!hide)
        {
            StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.25f, hide, true));
        }
        else
        {
            StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.25f, hide, true));
        }
    }

    IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show, bool hiding = false)
    {
        if (show && !hiding)
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

        if (!show && !hiding)
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

        if (!pausedDialogue)
        {
            StartCoroutine(currentBehavior._NextDialogue(currentDialogue));
        }
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
                        if(!pausedDialogue)
                        {
                            int commentIndex = currentChoice == -1 ? currentChoices[0].commentIndex : currentChoices[currentChoice].commentIndex;
                            currentBehavior.OnChoosePlayerOption(commentIndex);
                        }

                        CallNext();
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
            if (!pausedDialogue)
            {
                int commentIndex = index == -1 ? currentChoices[0].commentIndex : currentChoices[index].commentIndex;
                currentBehavior.OnChoosePlayerOption(commentIndex);
            }
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

    public void SetOptions(Dictionary<int, string> options)
    {
        int i = 0;
        foreach(int commentIndex in options.Keys)
        {
            DialogueUIPlayerOption newOp = Instantiate(playerOptionPrefab.gameObject, playerOptionPrefab.transform.position, Quaternion.identity).GetComponent<DialogueUIPlayerOption>();
            newOp.transform.SetParent(playerOptionPrefab.transform.parent, true);

            RectTransform newOpRT = newOp.GetComponent<RectTransform>();
            RectTransform playerChoicePrefabRT = playerOptionPrefab.GetComponent<RectTransform>();

            RectTransformExtensions.SetLeft(newOpRT, RectTransformExtensions.GetLeft(playerChoicePrefabRT));
            RectTransformExtensions.SetRight(newOpRT, RectTransformExtensions.GetRight(playerChoicePrefabRT));

            newOpRT.anchoredPosition += new Vector2(0, - (110 * i));
            newOpRT.localScale = new Vector3(1, 1, 1);
            newOp.playerOptionText.text = options[commentIndex];
            newOp.gameObject.SetActive(true);

            newOp.dialogueUIController = this;
            newOp.optionIndex = i++;
            newOp.commentIndex = commentIndex;

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
    public Dictionary<int, string> options;
    public Sprite sprite;
}
