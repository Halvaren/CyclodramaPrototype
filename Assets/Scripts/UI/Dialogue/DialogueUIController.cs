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
    public GameObject NPC_Container;
    public GameObject playerContainer;

    public TextMeshProUGUI NPC_Text;
    public TextMeshProUGUI NPC_Label;
    public Image NPCSprite;
    public DialogueUIPlayerOption playerOptionPrefab;
    public Image playerSprite;
    public TextMeshProUGUI playerLabel;

    public PCController PlayerCharacter
    {
        get { return PCController.instance; }
    }

    bool dialoguePaused = false;
    bool animatingText = false;

    private List<DialogueUIPlayerOption> currentChoices = new List<DialogueUIPlayerOption>();
    int currentChoice = 0;

    IEnumerator NPC_TextAnimator;

    InteractableObjBehavior currentBehavior;
    VIDE_Assign currentDialogue;

    public GeneralUIController GeneralUI
    {
        get { return GeneralUIController.Instance; }
    }

    public bool showingDialogue
    {
        get { return gameObject.activeSelf; }
    }

    public void ShowUnshow(bool value)
    {
        gameObject.SetActive(value);
    }

    public void StartDialogue(InteractableObjBehavior behavior, VIDE_Assign dialogue)
    {
        PlayerCharacter.EnableGameplayInput(false);
        PlayerCharacter.EnableInventoryInput(false);

        currentBehavior = behavior;
        currentDialogue = dialogue;
        if(!VD.isActive)
        {
            Begin();
        }
    }

    void Begin()
    {
        GeneralUI.DisplayDialogueUI();

        NPC_Text.text = "";
        NPC_Label.text = "";
        playerLabel.text = "";

        VD.OnActionNode += ActionHandler;
        VD.OnNodeChange += UpdateUI;
        VD.OnEnd += EndDialogue;

        currentBehavior.BeginDialogue(currentDialogue);
    }

    public void CallNext()
    {
        if (animatingText) { CutTextAnim(); return; }

        if(!dialoguePaused)
        {
            currentBehavior.NextDialogue(currentDialogue);
        }
    }

    private void Update()
    {
        VD.NodeData data = VD.nodeData;

        if(VD.isActive)
        {
            if(data.isPlayer)
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

                if(previousChoice != currentChoice)
                {
                    if (previousChoice >= 0 && previousChoice < currentChoices.Count) currentChoices[previousChoice].Highlight(false);
                    if (currentChoice >= 0 && currentChoice < currentChoices.Count) currentChoices[currentChoice].Highlight(true);
                }
            }

            if(Input.GetKeyDown(KeyCode.Return))
            {
                if(data.isPlayer)
                    data.commentIndex = currentChoice == -1 ? 0 : currentChoice;
                CallNext();
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
        VD.NodeData data = VD.nodeData;

        if (VD.isActive)
        {
            data.commentIndex = index == -1 ? 0 : index;
            CallNext();
        }
    }

    void UpdateUI(VD.NodeData data)
    {
        foreach (DialogueUIPlayerOption op in currentChoices)
            Destroy(op.gameObject);

        currentChoices = new List<DialogueUIPlayerOption>();
        NPC_Text.text = "";
        NPC_Container.SetActive(false);
        playerContainer.SetActive(false);
        playerSprite.sprite = null;
        NPCSprite.sprite = null;

        if(data.isPlayer)
        {
            if (data.sprite != null)
                playerSprite.sprite = data.sprite;
            else if (VD.assigned.defaultPlayerSprite != null)
                playerSprite.sprite = VD.assigned.defaultPlayerSprite;

            SetOptions(data.comments);

            if (data.tag.Length > 0)
                playerLabel.text = data.tag;

            playerContainer.SetActive(true);
        }
        else
        {
            if (data.sprite != null)
            {
                if(data.extraVars.ContainsKey("sprite"))
                {
                    if (data.commentIndex == (int)data.extraVars["sprite"])
                        NPCSprite.sprite = data.sprite;
                    else
                        NPCSprite.sprite = VD.assigned.defaultNPCSprite;
                }
                else
                {
                    NPCSprite.sprite = data.sprite;
                }
            }
            else if(VD.assigned.defaultNPCSprite != null)
                NPCSprite.sprite = VD.assigned.defaultNPCSprite;

            NPC_TextAnimator = DrawText(data.comments[data.commentIndex], 0.02f);
            StartCoroutine(NPC_TextAnimator);

            if (data.tag.Length > 0)
                NPC_Label.text = data.tag;
            else
                NPC_Label.text = VD.assigned.alias;

            NPC_Container.SetActive(true);
        }
    }

    public void SetOptions(string[] choices)
    {
        for(int i = 0; i < choices.Length; i++)
        {
            DialogueUIPlayerOption newOp = Instantiate(playerOptionPrefab.gameObject, playerOptionPrefab.transform.position, Quaternion.identity).GetComponent<DialogueUIPlayerOption>();
            newOp.transform.SetParent(playerOptionPrefab.transform.parent, true);

            RectTransform newOpRT = newOp.GetComponent<RectTransform>();
            RectTransform playerChoicePrefabRT = playerOptionPrefab.GetComponent<RectTransform>();

            RectTransformExtensions.SetLeft(newOpRT, RectTransformExtensions.GetLeft(playerChoicePrefabRT));
            RectTransformExtensions.SetRight(newOpRT, RectTransformExtensions.GetRight(playerChoicePrefabRT));

            newOp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, - (110 * i));
            newOp.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            newOp.playerOptionText.text = choices[i];
            newOp.gameObject.SetActive(true);

            newOp.dialogueUIController = this;
            newOp.optionIndex = i;

            currentChoices.Add(newOp);
        }

        currentChoice = 0;
        currentChoices[currentChoice].Highlight(true);
    }

    void EndDialogue(VD.NodeData data)
    {
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;

        GeneralUI.DisplayGameplayUI();

        currentBehavior = null;
        currentDialogue = null;

        PlayerCharacter.EnableGameplayInput(true);
        PlayerCharacter.EnableInventoryInput(true);

        VD.EndDialogue();
    }

    void OnDisable()
    {
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;
        if(dialogueContainer != null)
            GeneralUI.DisplayGameplayUI();

        PlayerCharacter.EnableGameplayInput(true);
        PlayerCharacter.EnableInventoryInput(true);

        VD.EndDialogue();
    }

    void OnLoadedAction()
    {
        Debug.Log("Finished loading all dialogues");
        VD.OnLoaded -= OnLoadedAction;
    }

    void ActionHandler(int actionNodeID)
    {
        Debug.Log("Action triggered: " + actionNodeID.ToString());
    }

    IEnumerator DrawText(string text, float time)
    {
        animatingText = true;

        string[] words = text.Split(' ');

        for(int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (i != words.Length - 1) word += " ";

            string previousText = NPC_Text.text;

            float lastHeight = NPC_Text.preferredHeight;
            NPC_Text.text += word;
            /*if(NPC_Text.preferredHeight > lastHeight)
            {
                previousText += System.Environment.NewLine;
            }*/

            for(int j = 0; j < word.Length; j++)
            {
                NPC_Text.text = previousText + word.Substring(0, j + 1);
                yield return new WaitForSeconds(time);
            }
        }
        NPC_Text.text = text;
        animatingText = false;
    }

    void CutTextAnim()
    {
        StopCoroutine(NPC_TextAnimator);
        NPC_Text.text = VD.nodeData.comments[VD.nodeData.commentIndex];
        animatingText = false;
    }
}
