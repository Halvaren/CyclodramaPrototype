using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class MainMenuUIController : MonoBehaviour
{
    public GameObject menuContainer;
    private RectTransform menuContainerRectTransform;
    public RectTransform MenuContainerRectTransform
    {
        get
        {
            if (menuContainerRectTransform == null) menuContainerRectTransform = menuContainer.GetComponent<RectTransform>();
            return menuContainerRectTransform;
        }
    }

    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject visualSettings;
    public GameObject audioSettings;

    public RectTransform showingPosition;
    public RectTransform unshowingPosition;

    public AudioClip openClip;
    public AudioClip closeClip;

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
        }
    }

    private DataManager dataManager;
    public DataManager DataManager
    {
        get
        {
            if (dataManager == null) dataManager = DataManager.Instance;
            return dataManager;
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

    #region Main menu variables

    public Button loadGameButton;

    #endregion

    #region Visual settings variables

    Resolution[] resolutions;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullScreenToggle;
    public TMP_Dropdown qualityDropdown;

    public Button applyButton;

    bool isFullScreen;
    int qualityIndex;
    Resolution chosenResolution;

    #endregion

    #region Audio settings variables

    public AudioMixerGroup mainAudioMixer;
    public AudioMixerGroup musicAudioMixer;
    public AudioMixerGroup SFXAudioMixer;
    public AudioMixerGroup ambienceAudioMixer;

    #endregion

    private void Start()
    {
        menuContainer.SetActive(false);
        MenuContainerRectTransform.position = unshowingPosition.position;

        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> resolutionsStrings = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];
            if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
                currentResolutionIndex = i;

            resolutionsStrings.Add(resolution.width + " x " + resolution.height);
        }

        resolutionDropdown.AddOptions(resolutionsStrings);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void ShowUnshow(bool show)
    {
        if(show && !GeneralUIController.displayingMainMenuUI)
        {
            GeneralUIController.PlayUISound(openClip);
            StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.5f, show));
        }
        else if(!show && GeneralUIController.displayingMainMenuUI)
        {
            GeneralUIController.PlayUISound(closeClip);
            StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.5f, show));
        }
    }

    IEnumerator ShowUnshowCoroutine(Vector3 initalPos, Vector3 finalPos, float time, bool show)
    {
        if (show)
        {
            menuContainer.SetActive(true);
            loadGameButton.interactable = GeneralUIController.dataUIController.AreThereFiles();
        }

        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            MenuContainerRectTransform.position = Vector3.Lerp(initalPos, finalPos, elapsedTime / time);

            yield return null;
        }
        MenuContainerRectTransform.position = finalPos;

        if (!show)
        {
            ShowMainMenu();
            menuContainer.SetActive(false);

            GeneralUIController.CurrentUI &= ~DisplayedUI.MainMenu;
        }
        else
        {
            GeneralUIController.CurrentUI |= DisplayedUI.MainMenu;
        }
    }

    #region Callback methods

    public void NewGame()
    {
        StartCoroutine(NewGameCoroutine());
    }

    public void LoadGame()
    {
        GeneralUIController.ShowDataUI(false);
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        visualSettings.SetActive(false);
        audioSettings.SetActive(false);
    }

    public void ShowSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        visualSettings.SetActive(false);
        audioSettings.SetActive(false);
    }

    public void ShowVisualSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        visualSettings.SetActive(true);
        audioSettings.SetActive(false);

        qualityIndex = QualitySettings.GetQualityLevel();
        chosenResolution = Screen.currentResolution;
        isFullScreen = Screen.fullScreen;

        qualityDropdown.SetValueWithoutNotify(qualityIndex);
        qualityDropdown.RefreshShownValue();

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];
            if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
            {
                resolutionDropdown.SetValueWithoutNotify(i);
                break;
            }
        }
        resolutionDropdown.RefreshShownValue();

        fullScreenToggle.SetIsOnWithoutNotify(isFullScreen);

        applyButton.interactable = false;
    }

    public void ShowAudioSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        visualSettings.SetActive(false);
        audioSettings.SetActive(true);
    }
    public void Exit()
    {
        Application.Quit();
    }

    public void SetMainVolume(float volume)
    {
        //mainAudioMixer.SetFloat("mainVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        //musicAudioMixer.SetFloat("mainVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        //SFXAudioMixer.SetFloat("mainVolume", volume);
    }

    public void OnChangeQuality(int index)
    {
        qualityIndex = index;
        applyButton.interactable = true;
    }

    public void SetFullscreen(bool isFullScreen)
    {
        this.isFullScreen = isFullScreen;
        applyButton.interactable = true;
    }

    public void SetResolution(int index)
    {
        chosenResolution = resolutions[index];
        applyButton.interactable = true;
    }

    public void ApplyVisualSettings()
    {
        if (QualitySettings.GetQualityLevel() != qualityIndex)
            QualitySettings.SetQualityLevel(qualityIndex);
        if (Screen.fullScreen != isFullScreen)
            Screen.fullScreen = isFullScreen;
        if (Screen.currentResolution.width != chosenResolution.width || Screen.currentResolution.height != chosenResolution.height)
            Screen.SetResolution(chosenResolution.width, chosenResolution.height, Screen.fullScreen);

        applyButton.interactable = false;
    }

    #endregion

    IEnumerator NewGameCoroutine()
    {
        GeneralUIController.UnshowEverything();
        GeneralUIController.ShowLoadingUI(LoadingState.Loading);

        yield return StartCoroutine(DataManager.LoadNewGameData());

        GeneralUIController.UnshowLoadingUI();

        GameManager.StartNewGame();
    }
}
