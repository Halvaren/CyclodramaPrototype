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

    public List<ActionVerbBarElement> ActionVerbBarElements;
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

    float scrollElapsedTime = 0.0f;
    float scrolled = 0.0f;

    float fullToHalfVisibilityElapsedTime = 0.0f;
    bool fullShown = false;

    ActionBarVisibility currentVisibility;

    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    public bool showingActionVerbs
    {
        get { return actionContainer.activeSelf; }
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
        if (ActionVerbBarElements != null && ActionVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            OnNewVerbSelected();

        SetActionBarVisibility(ActionBarVisibility.HalfShown);
    }

    private void Update()
    {
        if(!InventoryUIController.showingInventory || (InventoryUIController.showingInventory && !InventoryUIController.pointerIn))
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

                if (ActionVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
                    ActionVerbBarElements[selectedVerb].SetSelected(false);

                if (scrolled > 0) selectedVerb++;
                else selectedVerb--;

                if (selectedVerb >= ActionVerbBarElements.Count) selectedVerb = 0;
                else if (selectedVerb < 0) selectedVerb = ActionVerbBarElements.Count - 1;

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
        }
    }

    public void SetActionBarVisibility(ActionBarVisibility visibility)
    {
        if (currentVisibility == visibility) return;

        Vector3 initialPos = Vector3.zero;
        Vector3 finalPos = Vector3.zero;

        switch(currentVisibility)
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

        switch(visibility)
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
        StartCoroutine(ChangeActionBarVisibilityCoroutine(initialPos, finalPos, 0.25f, visibility));
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
    }

    void OnNewVerbSelected()
    {
        ActionVerbBarElement verbElement = ActionVerbBarElements[selectedVerb];
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
            this.selectedVerbInfo = ActionVerbBarElements[selectedVerb].verb.singleObjActionInfo;
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

    public Sprite selectedSprite;
    public Sprite unselectedSprite;

    public ActionVerb verb;

    public Texture2D normalCursor;
    public Texture2D hlCursor;
    public Texture2D disableCursor;

    public void SetSelected(bool value)
    {
        image.sprite = value ? selectedSprite : unselectedSprite;
    }
}
