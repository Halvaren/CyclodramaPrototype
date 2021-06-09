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

    public GameObject escapeIconContainer;

    public AudioClip openClip;
    public AudioClip closeClip;

    public AudioClip[] postItClips;
    int postItClipPointer = 0;

    float scrollElapsedTime = 0.0f;
    float scrolled = 0.0f;

    float fullToHalfVisibilityElapsedTime = 0.0f;
    bool showingBasicVerbs = true;

    IEnumerator changeVisibilityCoroutine;
    ActionBarVisibility previousVisibility;

    ActionBarVisibility currentVisibility = ActionBarVisibility.Unshown;

    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    public Color actionTextHighlightedColor = Color.blue;
    public Color actionTextNormalColor = Color.black;

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        { 
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
        }
    }

    private InventoryUIController inventoryUIController;
    public InventoryUIController InventoryUIController
    {
        get
        {
            if (inventoryUIController == null) inventoryUIController = GeneralUIController.inventoryUIController;
            return inventoryUIController;
        }
    }

    private InputManager inputManager;
    public InputManager InputManager
    {
        get
        {
            if (inputManager == null) inputManager = InputManager.instance;
            return inputManager;
        }
    }

    public void InitializeActionVerbs()
    {
        if (BasicVerbBarElements != null && BasicVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            OnNewVerbSelected();

        SetActionTextHighlighted(false);
        ActionContainerRectTransform.position = unshownPosition.position;
        currentVisibility = ActionBarVisibility.Unshown;

        showingBasicVerbs = true;
        basicVerbBar.SetActive(showingBasicVerbs);
        improvisationVerbBar.SetActive(!showingBasicVerbs);
    }

    public void ActionVerbsUpdate()
    {
        if(GeneralUIController.displayingGameplayUI)
        {
            if(!GeneralUIController.displayingInventoryUI || (GeneralUIController.displayingInventoryUI && !InventoryUIController.pointerIn))
                ManageScroll();

            if(currentVisibility == ActionBarVisibility.FullShown && !GeneralUIController.displayingInventoryUI)
                AutoHide();

            if(InputManager.pressedChangeVerbsKey)
            {
                ChangeVerbs();
            }
        }
    }

    void ManageScroll()
    {
        if (Mathf.Abs(InputManager.deltaScroll) > 0)
        {
            scrolled -= InputManager.deltaScroll;
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
            ChangeVisbility(ActionBarVisibility.FullShown);
            fullToHalfVisibilityElapsedTime = 0.0f;

            if (showingBasicVerbs && BasicVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
                BasicVerbBarElements[selectedVerb].SetSelected(false);
            else if (!showingBasicVerbs && ImprovisationVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
                ImprovisationVerbBarElements[selectedVerb].SetSelected(false);

            if (scrolled > 0) selectedVerb++;
            else selectedVerb--;

            if (showingBasicVerbs)
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
    }

    void AutoHide()
    {
        fullToHalfVisibilityElapsedTime += Time.deltaTime;

        if (fullToHalfVisibilityElapsedTime > fullToHalfVisibilityTime)
        {
            ChangeVisbility(ActionBarVisibility.HalfShown);
        }
    }

    void ChangeVerbs()
    {
        if (changeVisibilityCoroutine != null) return;
        StartCoroutine(ChangeVerbCoroutine());
    }

    IEnumerator ChangeVerbCoroutine()
    {
        yield return StartCoroutine(ChangeVisbilityCoroutine(ActionBarVisibility.HalfShown, true));

        if (showingBasicVerbs && BasicVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            BasicVerbBarElements[selectedVerb].SetSelected(false);
        else if (!showingBasicVerbs && ImprovisationVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            ImprovisationVerbBarElements[selectedVerb].SetSelected(false);

        showingBasicVerbs = !showingBasicVerbs;

        basicVerbBar.SetActive(showingBasicVerbs);
        improvisationVerbBar.SetActive(!showingBasicVerbs);

        OnNewVerbSelected();

        yield return StartCoroutine(ChangeVisbilityCoroutine(ActionBarVisibility.FullShown, true));

        fullToHalfVisibilityElapsedTime = 0.0f;
    }

    public void ChangeVisbility(ActionBarVisibility visibility, bool ignoreOtherCoroutines = false)
    {
        if (changeVisibilityCoroutine != null)
        {
            if(ignoreOtherCoroutines)
            {
                StopCoroutine(changeVisibilityCoroutine);
                currentVisibility = previousVisibility;
            }
            else
                return;
        }

        StartCoroutine(ChangeVisbilityCoroutine(visibility));
    }

    IEnumerator ChangeVisbilityCoroutine(ActionBarVisibility visibility, bool waitFinishing = false)
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

        if(currentVisibility == ActionBarVisibility.FullShown)
        {
            GeneralUIController.PlayUISound(closeClip);
        }
        else if(currentVisibility == ActionBarVisibility.HalfShown)
        {
            if (visibility == ActionBarVisibility.FullShown)
                GeneralUIController.PlayUISound(openClip);
            else
                GeneralUIController.PlayUISound(closeClip);
        }
        else
        {
            GeneralUIController.PlayUISound(openClip);
        }

        previousVisibility = currentVisibility;
        currentVisibility = visibility;

        if(waitFinishing)
        {
            changeVisibilityCoroutine = MoveActionBarCoroutine(initialPos, finalPos, 0.25f, visibility);
            yield return StartCoroutine(changeVisibilityCoroutine);
        }
        else
        {
            changeVisibilityCoroutine = MoveActionBarCoroutine(initialPos, finalPos, 0.25f, visibility);
            StartCoroutine(changeVisibilityCoroutine);
        }

        if (currentVisibility == ActionBarVisibility.Unshown) CursorManager.instance.ResetCursors();
        else
        {
            ActionVerbBarElement verbElement = showingBasicVerbs ? BasicVerbBarElements[selectedVerb] : ImprovisationVerbBarElements[selectedVerb];

            CursorManager.instance.SetCursors(verbElement.normalCursor, verbElement.disableCursor, verbElement.hlCursor, 
                verbElement.normalPOV, verbElement.disablePOV, verbElement.hlPOV);
        }
    }

    IEnumerator MoveActionBarCoroutine(Vector3 initialPos, Vector3 finalPos, float time, ActionBarVisibility newVisibility, bool hiding = false)
    {
        if (!hiding && (newVisibility == ActionBarVisibility.HalfShown || newVisibility == ActionBarVisibility.FullShown)) actionContainer.SetActive(true);

        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            ActionContainerRectTransform.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        ActionContainerRectTransform.position = finalPos;

        if(!hiding)
        {
            if (newVisibility == ActionBarVisibility.Unshown)
            {
                actionContainer.SetActive(false);
                GeneralUIController.CurrentUI &= ~DisplayedUI.Gameplay;
            }
            else
            {
                GeneralUIController.CurrentUI |= DisplayedUI.Gameplay;
            }
        }

        changeVisibilityCoroutine = null;
    }

    public void ShowUnshowEscapeIcon(bool show)
    {
        StartCoroutine(ShowEscapeIconCoroutine(show));
    }

    IEnumerator ShowEscapeIconCoroutine(bool show)
    {
        Vector3 initialPosition = currentVisibility == ActionBarVisibility.FullShown ? fullShownPosition.position : halfShownPosition.position;
        yield return MoveActionBarCoroutine(initialPosition, unshownPosition.position, 0.25f, ActionBarVisibility.Unshown, true);

        escapeIconContainer.SetActive(show);

        yield return MoveActionBarCoroutine(unshownPosition.position, halfShownPosition.position, 0.25f, ActionBarVisibility.HalfShown, true);
    }

    void OnNewVerbSelected()
    {
        ActionVerbBarElement verbElement = showingBasicVerbs ? BasicVerbBarElements[selectedVerb] : ImprovisationVerbBarElements[selectedVerb];
        verbElement.SetSelected(true);

        UpdatePostItClipPointer();
        GeneralUIController.PlayUISound(postItClips[postItClipPointer]);

        CursorManager.instance.SetCursors(verbElement.normalCursor, verbElement.disableCursor, verbElement.hlCursor,
            verbElement.normalPOV, verbElement.disablePOV, verbElement.hlPOV);
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

    public void SetActionTextHighlighted(bool value)
    {
        if(value)
        {
            actionText.color = actionTextHighlightedColor;
        }
        else
        {
            actionText.color = actionTextNormalColor;
        }
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
    void UpdatePostItClipPointer()
    {
        int randNum = Random.Range(0, postItClips.Length);
        if (randNum == postItClipPointer)
        {
            postItClipPointer += (int)Mathf.Pow(-1, Random.Range(0, 1));

            if (postItClipPointer < 0) postItClipPointer = postItClips.Length - 1;
            else if (postItClipPointer >= postItClips.Length) postItClipPointer = 0;
        }
        else postItClipPointer = randNum;
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

    public Sprite normalPOV;
    public Sprite hlPOV;
    public Sprite disablePOV;

    public void SetSelected(bool value)
    {
        image.sprite = value ? selectedSprite : unselectedSprite;
        icon.sprite = value ? selectedIcon : unselectedIcon;
        mask.sprite = value ? selectedMask : unselectedMask;
    }
}
