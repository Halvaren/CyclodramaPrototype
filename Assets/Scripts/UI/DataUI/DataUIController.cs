using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataUIController : MonoBehaviour
{
    public GameObject dataUIContainer;
    private RectTransform dataUIContainerRectTransform;
    public RectTransform DataUIContainerRectTransform
    {
        get
        {
            if (dataUIContainerRectTransform == null) dataUIContainerRectTransform = dataUIContainer.GetComponent<RectTransform>();
            return dataUIContainerRectTransform;
        }
    }

    public GameObject saveStatesPanel;
    public GameObject saveStateButtonPrefab;

    [HideInInspector]
    public GameObject autoSaveState;
    [HideInInspector]
    public List<GameObject> saveStates;
    [HideInInspector]
    public GameObject newSaveState;

    public RectTransform showingPosition;
    public RectTransform unshowingPosition;

    public ScrollRect scrollRect;

    public AudioClip openClip;
    public AudioClip closeClip;

    [HideInInspector]
    public bool saving;
    bool busy;

    Coroutine showingCoroutine;

    private DataManager dataManager;
    public DataManager DataManager
    {
        get
        {
            if (dataManager == null) dataManager = DataManager.Instance;
            return dataManager;
        }
    }

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
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

    private GameManager gameManager;
    public GameManager GameManager
    {
        get
        {
            if (gameManager == null) gameManager = GameManager.instance;
            return gameManager;
        }
    }

    public void InitializeDataUI(SaveStateData autosaveData, List<SaveStateData> fileDatas)
    {
        dataUIContainer.SetActive(false);
        DataUIContainerRectTransform.position = unshowingPosition.position;

        saveStates = new List<GameObject>();

        if (autosaveData != null)
        {
            autoSaveState = AddSaveState(autosaveData, true);
        }

        for (int i = 0; i < fileDatas.Count; i++)
        {
            AddSaveState(fileDatas[i]);            
        }

        newSaveState = AddSaveState(null, false, true);
    }

    public bool AreThereFiles()
    {
        return autoSaveState != null || saveStates.Count > 0;
    }

    public void DataUIUpdate()
    {
        if (GeneralUIController.displayingDataUI)
        {
            if (InputManager.pressedEscape && !busy)
            {
                GeneralUIController.UnshowDataUI();
            }
        }
    }

    public void ShowUnshow(bool show, bool saving = false)
    {
        if (showingCoroutine != null) return;

        this.saving = saving;
        if(show) newSaveState.SetActive(saving);

        if (show && !GeneralUIController.displayingDataUI)
        {
            GeneralUIController.PlayUISound(openClip);
            scrollRect.verticalNormalizedPosition = 1;
            showingCoroutine = StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.25f, show));
        }
        else if(!show && GeneralUIController.displayingDataUI)
        {
            GeneralUIController.PlayUISound(closeClip);
            showingCoroutine = StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.25f, show));
        }
    }

    IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show)
    {
        if(show)
        {
            dataUIContainer.SetActive(true);
        }

        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.unscaledDeltaTime;

            DataUIContainerRectTransform.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        DataUIContainerRectTransform.position = finalPos;

        if(!show)
        {
            dataUIContainer.SetActive(false);
            EnableButtons(true);

            GeneralUIController.CurrentUI &= ~DisplayedUI.Data;
        }
        else
        {
            GeneralUIController.CurrentUI |= DisplayedUI.Data;
        }

        showingCoroutine = null;
    }

    public GameObject AddSaveState(SaveStateData data, bool autosave = false, bool newSaveState = false)
    {
        GameObject saveState = Instantiate(saveStateButtonPrefab);

        saveState.GetComponent<SaveStateUIElement>().InitializeElement(this, data, saveStatesPanel.transform, autosave);

        if(!autosave)
        {
            if(newSaveState)
            {
                saveState.GetComponent<SaveStateUIElement>().saveIndex = saveStates.Count;
            }
            else
            {
                saveStates.Add(saveState);
                saveState.GetComponent<SaveStateUIElement>().saveIndex = saveStates.Count - 1;
            }
        }
        else
        {
            saveState.GetComponent<SaveStateUIElement>().saveIndex = -1;
        }
        saveState.GetComponent<SaveStateUIElement>().UpdateSaveState();

        return saveState;
    }

    public void UpdateSaveState(int index, SaveStateData data)
    {
        if(index >= saveStates.Count)
        {
            AddSaveState(data);
            newSaveState.GetComponent<SaveStateUIElement>().saveIndex++;

            newSaveState.transform.SetAsLastSibling();
        }
        else
        {
            if(index == -1)
            {
                if (autoSaveState == null)
                {
                    autoSaveState = AddSaveState(data, true);
                    autoSaveState.transform.SetAsFirstSibling();
                }
                autoSaveState.GetComponent<SaveStateUIElement>().UpdateSaveState(data);
            }
            else
            {
                saveStates[index].GetComponent<SaveStateUIElement>().UpdateSaveState(data);
            }
        }
    }

    public void OnClickSaveState(int saveIndex)
    {
        StartCoroutine(LoadSaveData(saveIndex));
    }

    IEnumerator LoadSaveData(int saveIndex)
    {
        GeneralUIController.ShowLoadingUI(saving ? LoadingState.Saving : LoadingState.Loading);
        EnableButtons(false);

        if(saving)
        {
            if (saveIndex == -1)
                yield return StartCoroutine(DataManager.SaveAutoSaveGameData());
            else
                yield return StartCoroutine(DataManager.SaveGameData(saveIndex));
        }
        else
        {
            if (saveIndex == -1)
                yield return StartCoroutine(DataManager.LoadAutoSaveGameData());
            else
                yield return StartCoroutine(DataManager.LoadGameData(saveIndex));
        }

        GeneralUIController.UnshowLoadingUI();

        if(!saving)
        {
            if (GeneralUIController.displayingMainMenuUI)
                GameManager.StartNewGame();
            else if(GeneralUIController.displayingPauseUI)
                GameManager.LoadOtherGame();
        }
        else
        {
            EnableButtons(true);
            GeneralUIController.UnshowDataUI();
        }
    }

    void EnableButtons(bool value)
    {
        busy = !value;
        autoSaveState.GetComponent<Button>().interactable = value;
        if(!value) autoSaveState.GetComponent<SaveStateUIElement>().OnPointerExit(null);

        newSaveState.GetComponent<Button>().interactable = value;
        if (!value) newSaveState.GetComponent<SaveStateUIElement>().OnPointerExit(null);

        foreach (GameObject saveState in saveStates)
        {
            saveState.GetComponent<Button>().interactable = value;
            if (!value) saveState.GetComponent<SaveStateUIElement>().OnPointerExit(null);
        }
    }
}
