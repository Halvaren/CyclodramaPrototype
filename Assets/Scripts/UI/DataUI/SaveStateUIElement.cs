using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// An object (a button in fact) that represent a save state of the game
/// </summary>
public class SaveStateUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Variables

    private DataUIController dataUIController;

    private SaveStateData currentSaveState;

    [HideInInspector]
    public int saveIndex;
    [HideInInspector]
    public bool autosave;

    public GameObject saveStateInfoContent;
    public TextMeshProUGUI emptyFileLabel;

    public TextMeshProUGUI saveLabel;
    public string autoSaveBaseText = "Autosave";
    public string saveBaseText = "Save ";
    public TextMeshProUGUI actSceneLabel;
    public string actSceneBaseText = "Act {0} Scene {1}";
    public TextMeshProUGUI playedTimeLabel;
    public string playedTimeBaseText = "Played time: ";
    public TextMeshProUGUI locationLabel;
    public string locationBaseText = "Location: ";

    public Sprite unhightlightedFrameSprite;
    public Sprite hightlightedFrameSprite;

    private Image frameImage;
    public Image FrameImage
    {
        get
        {
            if (frameImage == null) frameImage = GetComponent<Image>();
            return frameImage;
        }
    }

    private Button button;
    public Button Button
    {
        get
        {
            if (button == null) button = GetComponent<Button>();
            return button;
        }
    }

    #endregion

    /// <summary>
    /// Initializes the object
    /// </summary>
    /// <param name="dataUIController"></param>
    /// <param name="saveStateData"></param>
    /// <param name="parent"></param>
    /// <param name="autosave"></param>
    public void InitializeElement(DataUIController dataUIController, SaveStateData saveStateData, Transform parent, bool autosave = false)
    {
        this.dataUIController = dataUIController;
        transform.SetParent(parent, false);
        currentSaveState = saveStateData;
        this.autosave = autosave;

        if(saveStateData != null)
        {
            UpdateSaveState(saveStateData);
        }
        //If there's no save state data as parameter, that means that it is the empty save state used to save in a new file
        else
        {
            saveStateInfoContent.gameObject.SetActive(false);

            emptyFileLabel.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Updates the info displayed by the object given a new save state data
    /// </summary>
    /// <param name="data"></param>
    public void UpdateSaveState(SaveStateData data)
    {
        saveStateInfoContent.gameObject.SetActive(true);

        emptyFileLabel.gameObject.SetActive(false);

        currentSaveState = data;
        UpdateSaveState();
    }

    /// <summary>
    /// Updates the info displayed by the object using the current save state data
    /// </summary>
    public void UpdateSaveState()
    {
        if(currentSaveState != null)
        {
            saveLabel.text = saveIndex == -1 ? autoSaveBaseText : saveBaseText + saveIndex;
            actSceneLabel.text = string.Format(actSceneBaseText, currentSaveState.act, currentSaveState.scene);
            SetPlayedTime(currentSaveState.playedTime);
            SetLocation(currentSaveState.oliverLocation);
        }
    }

    /// <summary>
    /// Formats the played time and sets it to the corresponding text label
    /// </summary>
    /// <param name="time"></param>
    public void SetPlayedTime(float time)
    {
        int intTime = (int)time;
        int hours = intTime / 3600;
        int minutes = (intTime - (hours * 3600)) / 60;
        int seconds = intTime - (hours * 3600) - (minutes * 60);

        string hoursString = hours < 10 ? "0" + hours : hours.ToString();
        string minutesString = minutes < 10 ? "0" + minutes : minutes.ToString();
        string secondsString = seconds < 10 ? "0" + seconds : seconds.ToString();

        playedTimeLabel.text = playedTimeBaseText + hoursString + ":" + minutesString + ":" + secondsString;
    }

    /// <summary>
    /// Formats the current location and sets it to the corresponding text label
    /// </summary>
    /// <param name="location"></param>
    public void SetLocation(SetLocation location)
    {
        string locationString = location.ToString();
        string properLocationString = "";

        bool firstChar = true;
        foreach(char c in locationString)
        {
            if(!firstChar)
            {
                if ((c >= 65 && c <= 90) || (c >= 48 && c <= 57))
                    properLocationString += " ";
            }
            properLocationString += c;

            firstChar = false;
        }

        locationLabel.text = locationBaseText + properLocationString;
    }

    /// <summary>
    /// It is executed when the pointer enters the object
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(Button.interactable)
            FrameImage.sprite = hightlightedFrameSprite;
    }

    /// <summary>
    /// It is executed when the pointer exits the object
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        FrameImage.sprite = unhightlightedFrameSprite;
    }

    /// <summary>
    /// It is executed when object is clicked
    /// </summary>
    public void OnClick()
    {
        dataUIController.OnClickSaveState(saveIndex);
    }
}
