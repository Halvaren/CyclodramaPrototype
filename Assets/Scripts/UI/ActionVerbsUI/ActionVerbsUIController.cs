using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Possible positions of the ActionVerbs bar
/// </summary>
public enum ActionBarVisibility
{
    FullShown, HalfShown, Unshown
}

/// <summary>
/// Manages the Action verbs bar UI
/// </summary>
public class ActionVerbsUIController : MonoBehaviour
{
    #region Variables

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
    ActionBarVisibility currentVisibility = ActionBarVisibility.Unshown;

    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    public Color actionTextHighlightedColor = Color.blue;
    public Color actionTextNormalColor = Color.black;

    KeyCode[] alphaNumerics = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };

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

    #endregion

    /// <summary>
    /// Initializes the UI
    /// </summary>
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

    /// <summary>
    /// It is executed each frame
    /// </summary>
    public void ActionVerbsUpdate()
    {
        if(GeneralUIController.displayingGameplayUI)
        {
            //Controls input from number keys from alphanumeric keyboard
            ManageAlphaNumerics();

            //Controls mouse scroll input
            if(!GeneralUIController.displayingInventoryUI || (GeneralUIController.displayingInventoryUI && !InventoryUIController.pointerIn))
                ManageScroll();

            //Autohides the Action bar after some time
            if(currentVisibility == ActionBarVisibility.FullShown && !GeneralUIController.displayingInventoryUI)
                AutoHide();

            //If it is pressed the change verbs key, change the group of verbs displayed
            if(InputManager.pressedChangeVerbsKey)
            {
                ChangeVerbs();
            }
        }
    }

    /// <summary>
    /// Manages mouse scroll input
    /// </summary>
    void ManageScroll()
    {
        //If player scrolls
        if (Mathf.Abs(InputManager.deltaScroll) > 0)
        {
            //The amount of scrolled is added to scrolled
            scrolled -= InputManager.deltaScroll;
        }

        //If player has scrolled any amount
        if (Mathf.Abs(scrolled) > 0)
        {
            //Time starts to be counted
            scrollElapsedTime += Time.deltaTime;
        }

        //If scrolled time is over the time limits
        if (scrollElapsedTime > timeToScroll)
        {
            //scrolled is reset
            scrolled = 0;
            scrollElapsedTime = 0;
        }

        //If scrolled is over the the scroll limit
        if (Mathf.Abs(scrolled) > scrollDeltaNeededToNext)
        {
            //Changes the action bar visibility
            ChangeVisbility(ActionBarVisibility.FullShown);
            //Resets the autoHide time counter
            fullToHalfVisibilityElapsedTime = 0.0f;

            //Unhighlight the current selected verb
            if (showingBasicVerbs && BasicVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
                BasicVerbBarElements[selectedVerb].SetSelected(false);
            else if (!showingBasicVerbs && ImprovisationVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
                ImprovisationVerbBarElements[selectedVerb].SetSelected(false);

            //Changes the selectedVerb index
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

            //Calls to select a new verb
            OnNewVerbSelected();

            //Resets scrolled and scroll timer
            scrolled = 0;
            scrollElapsedTime = 0;
        }
    }

    /// <summary>
    /// Manages the alphanumeric number keys
    /// </summary>
    void ManageAlphaNumerics()
    {
        //Checks if any key is pressed and changes the selectedVerb index if it is the case
        int previousSelectedVerb = selectedVerb;
        bool pressedAny = false;
        foreach(KeyCode alphaNumeric in alphaNumerics)
        {
            if(Input.GetKeyDown(alphaNumeric))
            {
                if (alphaNumeric == KeyCode.Alpha0) selectedVerb = 9;
                else selectedVerb = (int)alphaNumeric - (int)alphaNumerics[0];

                pressedAny = true;
            }
        }

        //If any key has been pressed
        if (pressedAny)
        {
            //Changes action verb bar visibility
            ChangeVisbility(ActionBarVisibility.FullShown);
            //Resets the autoHide time counter
            fullToHalfVisibilityElapsedTime = 0.0f;

            //Unhighlight the current selected verb
            if (showingBasicVerbs && BasicVerbBarElements.Count > previousSelectedVerb && previousSelectedVerb >= 0)
                BasicVerbBarElements[previousSelectedVerb].SetSelected(false);
            else if (!showingBasicVerbs && ImprovisationVerbBarElements.Count > previousSelectedVerb && previousSelectedVerb >= 0)
                ImprovisationVerbBarElements[previousSelectedVerb].SetSelected(false);

            //Calls to select a new verb
            OnNewVerbSelected();
        }
    }

    /// <summary>
    /// Autohides the action bar (changes its visibility) after some time
    /// </summary>
    void AutoHide()
    {
        fullToHalfVisibilityElapsedTime += Time.deltaTime;

        if (fullToHalfVisibilityElapsedTime > fullToHalfVisibilityTime)
        {
            ChangeVisbility(ActionBarVisibility.HalfShown);
        }
    }

    /// <summary>
    /// Changes between the two groups of verbs
    /// </summary>
    void ChangeVerbs()
    {
        if (changeVisibilityCoroutine != null) return;
        StartCoroutine(ChangeVerbCoroutine());
    }

    /// <summary>
    /// Coroutine that changes between the two groups of verbs
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeVerbCoroutine()
    {
        yield return StartCoroutine(ChangeVisbilityCoroutine(ActionBarVisibility.HalfShown, false, true));

        if (showingBasicVerbs && BasicVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            BasicVerbBarElements[selectedVerb].SetSelected(false);
        else if (!showingBasicVerbs && ImprovisationVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            ImprovisationVerbBarElements[selectedVerb].SetSelected(false);

        showingBasicVerbs = !showingBasicVerbs;

        basicVerbBar.SetActive(showingBasicVerbs);
        improvisationVerbBar.SetActive(!showingBasicVerbs);

        OnNewVerbSelected();

        yield return StartCoroutine(ChangeVisbilityCoroutine(ActionBarVisibility.FullShown, false, true));

        fullToHalfVisibilityElapsedTime = 0.0f;
    }

    /// <summary>
    /// Changes the visbility of the bar
    /// </summary>
    /// <param name="visibility"></param>
    /// <param name="ignoreOtherCoroutines"></param>
    public void ChangeVisbility(ActionBarVisibility visibility, bool ignoreOtherCoroutines = false)
    {
        StartCoroutine(ChangeVisbilityCoroutine(visibility, ignoreOtherCoroutines));
    }

    /// <summary>
    /// Coroutine that changes the visibility of the bar
    /// </summary>
    /// <param name="visibility"></param>
    /// <param name="ignoreOtherCoroutines"></param>
    /// <param name="waitFinishing"></param>
    /// <returns></returns>
    public IEnumerator ChangeVisbilityCoroutine(ActionBarVisibility visibility, bool ignoreOtherCoroutines = false, bool waitFinishing = false)
    {
        if (changeVisibilityCoroutine != null)
        {
            if (ignoreOtherCoroutines)
            {
                StopCoroutine(changeVisibilityCoroutine);
            }
            else
                yield break;
        }

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

    /// <summary>
    /// Coroutine that moves the coroutine between two positions during a specific time
    /// </summary>
    /// <param name="initialPos"></param>
    /// <param name="finalPos"></param>
    /// <param name="time"></param>
    /// <param name="newVisibility"></param>
    /// <param name="hiding"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Shows or unshows the Esc icon
    /// </summary>
    /// <param name="show"></param>
    public void ShowUnshowEscapeIcon(bool show)
    {
        StartCoroutine(ShowEscapeIconCoroutine(show));
    }

    /// <summary>
    /// Coroutine that shows or unshows the Esc icon
    /// </summary>
    /// <param name="show"></param>
    /// <returns></returns>
    IEnumerator ShowEscapeIconCoroutine(bool show)
    {
        Vector3 initialPosition = currentVisibility == ActionBarVisibility.FullShown ? fullShownPosition.position : halfShownPosition.position;
        yield return MoveActionBarCoroutine(initialPosition, unshownPosition.position, 0.25f, ActionBarVisibility.Unshown, true);

        escapeIconContainer.SetActive(show);

        yield return MoveActionBarCoroutine(unshownPosition.position, halfShownPosition.position, 0.25f, ActionBarVisibility.HalfShown, true);
    }

    /// <summary>
    /// Changes the current selected verb
    /// </summary>
    void OnNewVerbSelected()
    {
        ActionVerbBarElement verbElement = showingBasicVerbs ? BasicVerbBarElements[selectedVerb] : ImprovisationVerbBarElements[selectedVerb];
        verbElement.SetSelected(true);

        UpdatePostItClipPointer();
        GeneralUIController.PlayUISound(postItClips[postItClipPointer]);

        //Updates the cursor
        CursorManager.instance.SetCursors(verbElement.normalCursor, verbElement.disableCursor, verbElement.hlCursor,
            verbElement.normalPOV, verbElement.disablePOV, verbElement.hlPOV);
        //Updates the selected verb in the ActionController of the PC
        PCController.instance.ActionController.SetSelectedVerb(verbElement.verb);
    }

    /// <summary>
    /// Updates the text of the verb info
    /// </summary>
    void UpdateActionText()
    {
        actionText.text = SpecialTrim(string.Format(selectedVerbInfo, 
            string.IsNullOrEmpty(firstFocusedObj) ? "" : firstFocusedObj, 
            string.IsNullOrEmpty(secondFocusedObj) ? "" : secondFocusedObj));
    }

    /// <summary>
    /// Deletes initial, final and double spaces
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Sets the current selected verb info
    /// </summary>
    /// <param name="selectedVerbInfo"></param>
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

    /// <summary>
    /// Changes the color of the verb info
    /// </summary>
    /// <param name="value"></param>
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

    /// <summary>
    /// Sets the first object of the verb info
    /// </summary>
    /// <param name="name"></param>
    public void SetFirstFocusedObj(string name)
    {
        string previousName = firstFocusedObj;
        firstFocusedObj = name;

        if(previousName != firstFocusedObj)
            UpdateActionText();
    }

    /// <summary>
    /// Resets the first object of the verb info
    /// </summary>
    public void ResetFirstFocusedObj()
    {
        SetFirstFocusedObj("");
    }

    /// <summary>
    /// Sets the second object of the verb info
    /// </summary>
    /// <param name="name"></param>
    public void SetSecondFocusedObj(string name)
    {
        string previousName = secondFocusedObj;
        secondFocusedObj = name;

        if(previousName != secondFocusedObj)
            UpdateActionText();
    }

    /// <summary>
    /// Resets the second object of the verb info
    /// </summary>
    public void ResetSecondFocusedObj()
    {
        SetSecondFocusedObj("");
    }

    /// <summary>
    /// Updates the pointer that randomly indicates which post-it sound must be played next time
    /// </summary>
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

/// <summary>
/// Element of the ActionVerbBar, which corresponds to a specific verb
/// </summary>
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
