using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ActionBarVisibility
{
    FullShown, HalfShown, Unshown
}

public class ActionVerbsUIController : MonoBehaviour
{
    public GameObject actionContainer;

    private RectTransform actionContainerRectTransform;
    public RectTransform ActionContainerRectTransform
    {
        get
        {
            if (actionContainerRectTransform == null) actionContainerRectTransform = actionContainer.GetComponent<RectTransform>();
            return actionContainerRectTransform;
        }
    }

    public List<ActionVerbBarElement> BasicVerbBarElements;
    public List<ActionVerbBarElement> ImprovisationVerbBarElements;
    public TextMeshProUGUI actionText;

    int selectedVerb = 0;
    string selectedVerbInfo = "";
    string firstFocusedObj;
    string secondFocusedObj;

    public float scrollDeltaNeededToNext = 0.5f;
    public float timeToScroll = 0.5f;
    public float fullToHalfVisibilityTime = 1f;

    public RectTransform fullShownPosition;
    public RectTransform halfShownPosition;
    public RectTransform unshownPosition;

    public GameObject basicVerbBar;
    public GameObject improvisationVerbBar;

    float scrollElapsedTime = 0.0f;
    float scrolled = 0.0f;

    float fullToHalfVisibilityElapsedTime = 0.0f;
    bool fullShown = false;
    bool showingBasicVerbs = true;

    IEnumerator changeVisibilityCoroutine;

    public ActionBarVisibility currentVisibility = ActionBarVisibility.Unshown;

    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    public bool showingActionVerbs
    {
        get { return actionContainer.activeSelf; }
    }

    private DialogueUIController dialogueUIController;
    public DialogueUIController DialogueUIController
    {
        get
        {
            if (dialogueUIController == null) dialogueUIController = GeneralUIController.Instance.dialogueUIController;
            return dialogueUIController;
        }
    }

    private InventoryUIController inventoryUIController;
    public InventoryUIController InventoryUIController
    {
        get
        {
            if (inventoryUIController == null) inventoryUIController = GeneralUIController.Instance.inventoryUIController;
            return inventoryUIController;
        }
    }

    private DetailedUIController detailedUIController;
    public DetailedUIController DetailedUIController
    {
        get
        {
            if (detailedUIController == null) detailedUIController = GeneralUIController.Instance.detailedUIController;
            return detailedUIController;
        }
    }

    private RectTransform rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }

    private void Start()
    {
        if (BasicVerbBarElements != null && BasicVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            OnNewVerbSelected();

        ActionContainerRectTransform.position = unshownPosition.position;

        SetActionBarVisibility(ActionBarVisibility.FullShown);
        fullShown = true;

        basicVerbBar.SetActive(showingBasicVerbs);
        improvisationVerbBar.SetActive(!showingBasicVerbs);
    }

    private void Update()
    {
        if(currentVisibility != ActionBarVisibility.Unshown && !DialogueUIController.showingDialogue && !DetailedUIController.showingAnyDetailedUI && (!InventoryUIController.showingInventory || (InventoryUIController.showingInventory && !InventoryUIController.pointerIn)))
        {
            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0)
            {
                scrolled -= Input.mouseScrollDelta.y;
            }

            if (Mathf.Abs(scrolled) > 0)
            {
                scrollElapsedTime += Time.deltaTime;
            }

            if (scrollElapsedTime > timeToScroll)
            {
                scrolled = 0;
                scrollElapsedTime = 0;
            }

            if (Mathf.Abs(scrolled) > scrollDeltaNeededToNext)
            {
                SetActionBarVisibility(ActionBarVisibility.FullShown);
                fullShown = true;
                fullToHalfVisibilityElapsedTime = 0.0f;

                if (showingBasicVerbs && BasicVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
                    BasicVerbBarElements[selectedVerb].SetSelected(false);
                else if (!showingBasicVerbs && ImprovisationVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
                    ImprovisationVerbBarElements[selectedVerb].SetSelected(false);

                if (scrolled > 0) selectedVerb++;
                else selectedVerb--;

                if(showingBasicVerbs)
                {
                    if (selectedVerb >= BasicVerbBarElements.Count) selectedVerb = 0;
                    else if (selectedVerb < 0) selectedVerb = BasicVerbBarElements.Count - 1;
                }
                else
                {
                    if (selectedVerb >= ImprovisationVerbBarElements.Count) selectedVerb = 0;
                    else if (selectedVerb < 0) selectedVerb = ImprovisationVerbBarElements.Count - 1;
                }                

                OnNewVerbSelected();

                scrolled = 0;
                scrollElapsedTime = 0;
            }

            if(fullShown && !InventoryUIController.showingInventory && currentVisibility == ActionBarVisibility.FullShown)
            {
                fullToHalfVisibilityElapsedTime += Time.deltaTime;

                if(fullToHalfVisibilityElapsedTime > fullToHalfVisibilityTime)
                {
                    SetActionBarVisibility(ActionBarVisibility.HalfShown);
                }
            }

            if(Input.GetKeyDown(KeyCode.C))
            {
                ChangeVerbs();
            }
        }
    }

    public void ChangeVerbs()
    {
        if (changeVisibilityCoroutine != null) return;
        StartCoroutine(ChangeVerbCoroutine());
    }

    IEnumerator ChangeVerbCoroutine()
    {
        yield return StartCoroutine(SetActionBarVisibilityCoroutine(ActionBarVisibility.HalfShown, true));

        if (showingBasicVerbs && BasicVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            BasicVerbBarElements[selectedVerb].SetSelected(false);
        else if (!showingBasicVerbs && ImprovisationVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            ImprovisationVerbBarElements[selectedVerb].SetSelected(false);

        showingBasicVerbs = !showingBasicVerbs;

        basicVerbBar.SetActive(showingBasicVerbs);
        improvisationVerbBar.SetActive(!showingBasicVerbs);

        OnNewVerbSelected();

        yield return StartCoroutine(SetActionBarVisibilityCoroutine(ActionBarVisibility.FullShown, true));

        fullShown = true;
        fullToHalfVisibilityElapsedTime = 0.0f;
    }

    public void SetActionBarVisibility(ActionBarVisibility visibility, bool waitFinishing = false, bool ignoreOtherCoroutines = false)
    {
        if (changeVisibilityCoroutine != null)
        {
            if(ignoreOtherCoroutines)
            {
                StopCoroutine(changeVisibilityCoroutine);
            }
            else
                return;
        }
        StartCoroutine(SetActionBarVisibilityCoroutine(visibility, waitFinishing));
    }

    public IEnumerator SetActionBarVisibilityCoroutine(ActionBarVisibility visibility, bool waitFinishing)
    {
        if (currentVisibility == visibility)
        { 
            yield break;
        }

        Vector3 initialPos = Vector3.zero;
        Vector3 finalPos = Vector3.zero;

        switch (currentVisibility)
        {
            case ActionBarVisibility.FullShown:
                initialPos = fullShownPosition.position;
                break;
            case ActionBarVisibility.HalfShown:
                initialPos = halfShownPosition.position;
                break;
            case ActionBarVisibility.Unshown:
                initialPos = unshownPosition.position;
                break;
        }

        switch (visibility)
        {
            case ActionBarVisibility.FullShown:
                finalPos = fullShownPosition.position;
                break;
            case ActionBarVisibility.HalfShown:
                finalPos = halfShownPosition.position;
                break;
            case ActionBarVisibility.Unshown:
                finalPos = unshownPosition.position;
                break;
        }

        currentVisibility = visibility;

        if(waitFinishing)
        {
            changeVisibilityCoroutine = ChangeActionBarVisibilityCoroutine(initialPos, finalPos, 0.25f, visibility);
            yield return StartCoroutine(changeVisibilityCoroutine);
        }
        else
        {
            changeVisibilityCoroutine = ChangeActionBarVisibilityCoroutine(initialPos, finalPos, 0.25f, visibility);
            StartCoroutine(changeVisibilityCoroutine);
        }
    }

    IEnumerator ChangeActionBarVisibilityCoroutine(Vector3 initialPos, Vector3 finalPos, float time, ActionBarVisibility newVisibility)
    {
        if (newVisibility == ActionBarVisibility.HalfShown || newVisibility == ActionBarVisibility.FullShown) actionContainer.SetActive(true);

        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            ActionContainerRectTransform.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        ActionContainerRectTransform.position = finalPos;

        if (newVisibility == ActionBarVisibility.Unshown) actionContainer.SetActive(false);

        changeVisibilityCoroutine = null;
    }

    void OnNewVerbSelected()
    {
        ActionVerbBarElement verbElement = showingBasicVerbs ? BasicVerbBarElements[selectedVerb] : ImprovisationVerbBarElements[selectedVerb];
        verbElement.SetSelected(true);

        CursorManager.instance.SetCursors(verbElement.normalCursor, verbElement.disableCursor, verbElement.hlCursor);
        PCController.instance.ActionController.SetSelectedVerb(verbElement.verb);
    }

    void UpdateActionText()
    {
        actionText.text = SpecialTrim(string.Format(selectedVerbInfo, 
            string.IsNullOrEmpty(firstFocusedObj) ? "" : firstFocusedObj, 
            string.IsNullOrEmpty(secondFocusedObj) ? "" : secondFocusedObj));
    }

    string SpecialTrim(string input)
    {
        input = input.Trim(' ');
        string output = "";

        bool space = false;
        foreach(char c in input)
        {
            if(c == ' ')
            {
                if (!space)
                {
                    space = true;
                    output += c;
                }
            }
            else
            {
                output += c;
                space = false;
            }
        }

        return output;
    }

    public void SetSelectedVerbInfo(string selectedVerbInfo = null)
    {
        string previousVerbInfo = this.selectedVerbInfo;

        if (selectedVerbInfo == null)
            this.selectedVerbInfo = showingBasicVerbs ? BasicVerbBarElements[selectedVerb].verb.singleObjActionInfo : ImprovisationVerbBarElements[selectedVerb].verb.singleObjActionInfo;
        else
            this.selectedVerbInfo = selectedVerbInfo;

        if(this.selectedVerbInfo != previousVerbInfo)
            UpdateActionText();
    }

    public void SetFirstFocusedObj(string name)
    {
        string previousName = firstFocusedObj;
        firstFocusedObj = name;

        if(previousName != firstFocusedObj)
            UpdateActionText();
    }

    public void ResetFirstFocusedObj()
    {
        SetFirstFocusedObj("");
    }

    public void SetSecondFocusedObj(string name)
    {
        string previousName = secondFocusedObj;
        secondFocusedObj = name;

        if(previousName != secondFocusedObj)
            UpdateActionText();
    }

    public void ResetSecondFocusedObj()
    {
        SetSecondFocusedObj("");
    }
}

[System.Serializable]
public class ActionVerbBarElement
{
    public Image image;
    public Image icon;
    public Image mask;

    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public Sprite selectedIcon;
    public Sprite unselectedIcon;
    public Sprite selectedMask;
    public Sprite unselectedMask;

    public ActionVerb verb;

    public Texture2D normalCursor;
    public Texture2D hlCursor;
    public Texture2D disableCursor;

    public void SetSelected(bool value)
    {
        image.sprite = value ? selectedSprite : unselectedSprite;
        icon.sprite = value ? selectedIcon : unselectedIcon;
        mask.sprite = value ? selectedMask : unselectedMask;
    }
}
