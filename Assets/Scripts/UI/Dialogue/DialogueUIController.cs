using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIDE_Data;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the UI that shows the dialogues
/// </summary>
public class DialogueUIController : MonoBehaviour
{
    #region Variables

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

    public ScrollRect scrollRect;
    float scrollIncrement;

    public AudioClip openClip;
    public AudioClip closeClip;

    public AudioClip typingClip;
    public AudioClip returnClip;

    public AudioClip[] markerClips;
    int markerClipPointer = 0;

    Queue<AudioSource> typingSounds;

    bool animatingText = false;
    [HideInInspector]
    public bool pausedDialogue = false;

    private List<DialogueUIPlayerOption> currentChoices = new List<DialogueUIPlayerOption>();
    int currentChoice = 0;

    DialogueUINode currentNode;

    IEnumerator NPC_TextAnimator;

    InteractableObjBehavior currentBehavior;
    VIDE_Assign currentDialogue;

    private InputManager inputManager;
    public InputManager InputManager
    {
        get
        {
            if (inputManager == null) inputManager = InputManager.instance;
            return inputManager;
        }
    }

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get 
        { 
            if(generalUIController == null) generalUIController = GeneralUIController.instance; 
            return generalUIController; 
        }
    }

    #endregion

    private void Start()
    {
        typingSounds = new Queue<AudioSource>();
    }

    /// <summary>
    /// Shows or unshows the UI
    /// </summary>
    /// <param name="show"></param>
    public void ShowUnshow(bool show)
    {
        if (show && !GeneralUIController.displayingDialogueUI)
        {
            GeneralUIController.PlayUISound(openClip);
            StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.25f, show));
        }
        else if (!show && GeneralUIController.displayingDialogueUI)
        {
            GeneralUIController.PlayUISound(closeClip);
            StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.25f, show));
        }
    }

    /// <summary>
    /// Hides or unhides the UI (the difference from showing/unshowing it it's that it doesn't notify it to GeneralUIController)
    /// </summary>
    /// <param name="hide"></param>
    public void HideUnhide(bool hide)
    {
        pausedDialogue = hide;
        if (!hide)
        {
            GeneralUIController.PlayUISound(openClip);
            StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.25f, hide, true));
        }
        else
        {
            GeneralUIController.PlayUISound(closeClip);
            StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.25f, hide, true));
        }
    }

    /// <summary>
    /// Coroutine that shows or unshows (or hides or unhides) the UI
    /// </summary>
    /// <param name="initialPos"></param>
    /// <param name="finalPos"></param>
    /// <param name="time"></param>
    /// <param name="show"></param>
    /// <param name="hiding"></param>
    /// <returns></returns>
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

        if(!hiding)
        {
            if (!show)
            {
                dialogueContainer.SetActive(false);

                GeneralUIController.CurrentUI &= ~DisplayedUI.Dialogue;
            }
            else
            {
                GeneralUIController.CurrentUI |= DisplayedUI.Dialogue;
            }
        }
    }

    /// <summary>
    /// Prepares the next dialogue: it stores a reference to the behavior that starts the dialogue and shows the UI
    /// </summary>
    /// <param name="behavior"></param>
    /// <param name="dialogue"></param>
    public void PrepareDialogueUI(InteractableObjBehavior behavior, VIDE_Assign dialogue)
    {
        currentBehavior = behavior;
        currentDialogue = dialogue;

        GeneralUIController.ShowDialogueUI();

        if (GeneralUIController.displayingGameplayUI)
        {
            GeneralUIController.UnshowGameplayUI();
        }

        NPC_Text.text = "";
        NPC_Label.text = "";
        playerLabel.text = "";
    }

    /// <summary>
    /// Shows the next dialogue
    /// </summary>
    public void CallNext()
    {
        if (animatingText) { CutTextAnim();  return; }

        if (!pausedDialogue)
        {
            StartCoroutine(currentBehavior._NextDialogue(currentDialogue));
        }
    }

    /// <summary>
    /// It is executed each frame
    /// </summary>
    public void DialogueUpdate()
    {
        if(GeneralUIController.displayingDialogueUI && VD.isActive)
        {
            if(currentNode != null)
            {
                if (currentNode.isPlayer)
                {
                    int previousChoice = currentChoice;

                    if (InputManager.pressedDown)
                    {
                        if (currentChoice == -1) currentChoice = 0;
                        else
                        {
                            currentChoice++;

                            if(scrollRect.verticalNormalizedPosition > 0) scrollRect.verticalNormalizedPosition -= scrollIncrement;

                            if (currentChoice >= currentChoices.Count)
                            {
                                currentChoice = 0;
                                scrollRect.verticalNormalizedPosition = 1;
                            }
                        }
                    }
                    if (InputManager.pressedUp)
                    {
                        if (currentChoice == -1) currentChoice = 0;
                        else
                        {
                            currentChoice--;

                            if (scrollRect.verticalNormalizedPosition < 1) scrollRect.verticalNormalizedPosition += scrollIncrement;

                            if (currentChoice < 0)
                            {
                                currentChoice = currentChoices.Count - 1;
                                scrollRect.verticalNormalizedPosition = 0;
                            }
                        }
                    }

                    if (previousChoice != currentChoice)
                    {
                        UpdateMarkerPointer();
                        GeneralUIController.PlayUISound(markerClips[markerClipPointer]);

                        if (previousChoice >= 0 && previousChoice < currentChoices.Count) currentChoices[previousChoice].Highlight(false);
                        if (currentChoice >= 0 && currentChoice < currentChoices.Count) currentChoices[currentChoice].Highlight(true);
                    }
                }

                if (InputManager.pressedSpace)
                {
                    if (currentNode.isPlayer)
                    {
                        bool callNext = false;
                        if(!pausedDialogue)
                        {
                            int commentIndex = currentChoice == -1 ? currentChoices[0].commentIndex : currentChoices[currentChoice].commentIndex;
                            callNext = currentBehavior.OnChoosePlayerOption(commentIndex);
                        }

                        if(callNext) CallNext();
                    }
                    else
                    {
                        CallNext();
                    }
                }
            }
        }
    }

    /// <summary>
    /// It is executed when pointer is over a player option
    /// </summary>
    /// <param name="index"></param>
    public void OnHoverPlayerOption(int index)
    {
        int previousChoice = currentChoice;
        currentChoice = index;

        if (previousChoice != currentChoice)
        {
            UpdateMarkerPointer();
            GeneralUIController.PlayUISound(markerClips[markerClipPointer]);

            if (previousChoice >= 0 && previousChoice < currentChoices.Count) currentChoices[previousChoice].Highlight(false);
            if (currentChoice >= 0 && currentChoice < currentChoices.Count) currentChoices[currentChoice].Highlight(true);
        }
    }

    /// <summary>
    /// It is executed when a player option is clicked
    /// </summary>
    /// <param name="index"></param>
    public void OnClickPlayerOption(int index)
    {
        if (VD.isActive)
        {
            bool callNext = false;
            if (!pausedDialogue)
            {
                int commentIndex = index == -1 ? currentChoices[0].commentIndex : currentChoices[index].commentIndex;
                callNext = currentBehavior.OnChoosePlayerOption(commentIndex);
            }

            if (callNext) CallNext();
        }
    }

    /// <summary>
    /// Updates the UI elements with info received by parameter
    /// </summary>
    /// <param name="node"></param>
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

            string message = GetLongShortMessage(node.message, true);
            NPC_TextAnimator = DrawText(message, 0.02f);
            StartCoroutine(NPC_TextAnimator);

            NPC_Label.text = node.tag;

            NPC_Container.SetActive(true);
        }
    }

    /// <summary>
    /// Sets the player options in case the current node is a player node
    /// </summary>
    /// <param name="options"></param>
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
            newOp.playerOptionText.text = GetLongShortMessage(options[commentIndex], false);
            newOp.gameObject.SetActive(true);

            newOp.dialogueUIController = this;
            newOp.optionIndex = i++;
            newOp.commentIndex = commentIndex;

            currentChoices.Add(newOp);
        }

        if (currentChoices.Count <= 2) scrollIncrement = 0;
        else scrollIncrement = 1f / (currentChoices.Count - 2);
        scrollRect.verticalNormalizedPosition = 1;

        currentChoice = 0;

        UpdateMarkerPointer();
        GeneralUIController.PlayUISound(markerClips[markerClipPointer]);
        currentChoices[currentChoice].Highlight(true);
    }

    /// <summary>
    /// From a long player option, it can extract the actual option or a short version
    /// </summary>
    /// <param name="originalMessage"></param>
    /// <param name="getLong"></param>
    /// <returns></returns>
    string GetLongShortMessage(string originalMessage, bool getLong)
    {
        if (originalMessage[0] != '[') return originalMessage;
        string result = "";

        bool foundBreak = false;
        bool avoidReturnChar = false;
        for(int i = 1; i < originalMessage.Length; i++)
        {
            char c = originalMessage[i];
            if(avoidReturnChar)
            {
                avoidReturnChar = false;
                if(c == '\n')
                    continue;
            }

            if (c == ']')
            {
                if (getLong)
                {
                    foundBreak = true;
                    avoidReturnChar = true;
                    continue;
                }
                else break;
            }

            if((getLong && foundBreak) || !getLong)
                result += c;
        }

        return result;
    }

    /// <summary>
    /// It is called when the dialogue ends
    /// </summary>
    public void EndDialogue()
    {
        StopAllCoroutines();

        GeneralUIController.UnshowDialogueUI();

        currentBehavior = null;
        currentDialogue = null;
        currentNode = null;
    }

    /// <summary>
    /// Coroutine that draws the dialogue letter by letter
    /// </summary>
    /// <param name="text"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator DrawText(string text, float time)
    {
        typingSounds.Enqueue(GeneralUIController.PlayUISound(typingClip, true));
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

        GeneralUIController.StopUISound(typingSounds.Dequeue(), 0.5f);
    }

    /// <summary>
    /// Interrumpts the draw text coroutine
    /// </summary>
    void CutTextAnim()
    {
        StopCoroutine(NPC_TextAnimator);
        GeneralUIController.StopUISound(typingSounds.Dequeue(), 0.5f);
        NPC_Text.text = GetLongShortMessage(currentNode.message, true);
        animatingText = false;
    }

    /// <summary>
    /// Updates the pointer that randomly indicates which marker sound must be played next time
    /// </summary>
    void UpdateMarkerPointer()
    {
        int randNum = Random.Range(0, markerClips.Length);
        if (randNum == markerClipPointer)
        {
            markerClipPointer += (int)Mathf.Pow(-1, Random.Range(0, 1));

            if (markerClipPointer < 0) markerClipPointer = markerClips.Length - 1;
            else if (markerClipPointer >= markerClips.Length) markerClipPointer = 0;
        }
        else markerClipPointer = randNum;
    }
}

/// <summary>
/// Simplified dialogue node data container for UI use
/// </summary>
public class DialogueUINode
{
    public string tag;
    public bool isPlayer;
    public string message;
    public Dictionary<int, string> options;
    public Sprite sprite;
}
