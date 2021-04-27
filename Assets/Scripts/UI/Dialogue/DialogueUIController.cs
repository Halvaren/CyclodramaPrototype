using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VIDE_Data;
using TMPro;

public class DialogueUIController : MonoBehaviour
{
    public GameObject dialogueContainer;
    public GameObject NPC_Container;
    public GameObject playerContainer;

    public TextMeshProUGUI NPC_Text;
    public TextMeshProUGUI NPC_Label;
    public Image NPCSprite;
    public GameObject playerChoicePrefab;
    public Image playerSprite;
    public TextMeshProUGUI playerLabel;

    public PCController playerCharacter;

    bool dialoguePaused = false;
    bool animatingText = false;

    private List<TextMeshProUGUI> currentChoices = new List<TextMeshProUGUI>();

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

    public void Interact(InteractableObjBehavior behavior, VIDE_Assign dialogue)
    {
        currentBehavior = behavior;
        currentDialogue = dialogue;
        if(!VD.isActive)
        {
            Begin();
        }
        else
        {
            CallNext();
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

        playerCharacter.EnableGameplayInput(false);
        playerCharacter.EnableInventoryInput(false);


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
            if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                CallNext();
            }
            /*if(!data.pausedAction && data.isPlayer)
            {
                if(Input.GetKeyDown(KeyCode.S))
                {
                    if (data.commentIndex < currentChoices.Count - 1)
                        data.commentIndex++;
                }
                if(Input.GetKeyDown(KeyCode.W))
                {
                    if (data.commentIndex > 0)
                        data.commentIndex--;
                }

                for(int i = 0; i < currentChoices.Count; i++)
                {
                    currentChoices[i].color = Color.white;
                    if (i == data.commentIndex) currentChoices[i].color = Color.yellow;
                }
            }*/
        }
    }

    void UpdateUI(VD.NodeData data)
    {
        foreach (TextMeshProUGUI op in currentChoices)
            Destroy(op.gameObject);

        currentChoices = new List<TextMeshProUGUI>();
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
            GameObject newOp = Instantiate(playerChoicePrefab.gameObject, playerChoicePrefab.transform.position, Quaternion.identity) as GameObject;
            newOp.transform.SetParent(playerChoicePrefab.transform.parent, true);

            RectTransform newOpRT = newOp.GetComponent<RectTransform>();
            RectTransform playerChoicePrefabRT = playerChoicePrefab.GetComponent<RectTransform>();

            RectTransformExtensions.SetLeft(newOpRT, RectTransformExtensions.GetLeft(playerChoicePrefabRT));
            RectTransformExtensions.SetRight(newOpRT, RectTransformExtensions.GetRight(playerChoicePrefabRT));

            newOp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, - (110 * i));
            newOp.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            newOp.GetComponent<TextMeshProUGUI>().text = choices[i];
            newOp.SetActive(true);

            currentChoices.Add(newOp.GetComponent<TextMeshProUGUI>());
        }
    }

    void EndDialogue(VD.NodeData data)
    {
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;

        GeneralUI.DisplayGameplayUI();

        playerCharacter.EnableGameplayInput(true);
        playerCharacter.EnableInventoryInput(true);

        currentBehavior = null;
        currentDialogue = null;

        VD.EndDialogue();
    }

    void OnDisable()
    {
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;
        if(dialogueContainer != null)
            GeneralUI.DisplayGameplayUI();

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
