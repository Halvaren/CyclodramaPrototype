﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionVerbsUIController : MonoBehaviour
{
    public List<ActionVerbBarElement> ActionVerbBarElements;
    public TextMeshProUGUI actionText;

    int selectedVerb = 0;
    string selectedVerbText = "";
    string firstFocusedObj;
    string secondFocusedObj;

    public float scrollDeltaNeededToNext = 0.5f;
    public float timeToScroll = 0.5f;

    float elapsedTime = 0.0f;
    float scrolled = 0.0f;

    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    public bool showingActionVerbs
    {
        get { return gameObject.activeSelf; }
    }

    private void Start()
    {
        if (ActionVerbBarElements != null && ActionVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
            OnNewVerbSelected();
    }

    private void Update()
    {
        if(Mathf.Abs(Input.mouseScrollDelta.y) > 0)
        {
            scrolled -= Input.mouseScrollDelta.y;
        }

        if(Mathf.Abs(scrolled) > 0)
        {
            elapsedTime += Time.deltaTime;
        }

        if(elapsedTime > timeToScroll)
        {
            scrolled = 0;
            elapsedTime = 0;
        }

        if(Mathf.Abs(scrolled) > scrollDeltaNeededToNext)
        {
            if (ActionVerbBarElements.Count > selectedVerb && selectedVerb >= 0)
                ActionVerbBarElements[selectedVerb].SetSelected(false);

            if (scrolled > 0) selectedVerb++;
            else selectedVerb--;

            if (selectedVerb >= ActionVerbBarElements.Count) selectedVerb = 0;
            else if (selectedVerb < 0) selectedVerb = ActionVerbBarElements.Count - 1;

            OnNewVerbSelected();

            scrolled = 0;
            elapsedTime = 0;
        }
    }

    public void ShowUnshow(bool value)
    {
        gameObject.SetActive(value);
    }

    void OnNewVerbSelected()
    {
        ActionVerbBarElement verbElement = ActionVerbBarElements[selectedVerb];
        verbElement.SetSelected(true);

        selectedVerbText = verbElement.verb != null ? verbElement.verb.name : "------";
        UpdateActionText();

        CursorManager.instance.SetCursors(verbElement.normalCursor, verbElement.disableCursor, verbElement.hlCursor);
        PCController.instance.ActionController.SetSelectedVerb(verbElement.verb);
    }

    void UpdateActionText()
    {
        actionText.text = selectedVerbText;
        if (!string.IsNullOrEmpty(firstFocusedObj))
            actionText.text += " " + firstFocusedObj;
        if (!string.IsNullOrEmpty(secondFocusedObj))
            actionText.text += " " + secondFocusedObj;
    }

    public void SetFirstFocusedObj(string name)
    {
        string previousName = firstFocusedObj;
        firstFocusedObj = name;

        if(previousName != firstFocusedObj)
            UpdateActionText();
    }

    public void SetSecondFocusedObj(string name)
    {
        string previousName = secondFocusedObj;
        secondFocusedObj = name;

        if(previousName != secondFocusedObj)
            UpdateActionText();
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
