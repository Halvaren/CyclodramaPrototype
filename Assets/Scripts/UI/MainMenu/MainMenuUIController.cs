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
    public AudioClip[] tappingClips;

    public Sprite frontCover;
    public Sprite backCover;

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

    public Slider mainSlider;
    public Slider musicSlider;
    public Slider SFXSlider;
    public Slider ambienceSlider;

    public AudioMixer mainAudioMixer;

    #endregion

    private void Start()
    {
        LoadVolumesFromPlayerPrefs();
        AddSliderListeners();

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

    IEnumerator ChangeCover(bool toMainMenu)
    {
        GeneralUIController.PlayUISound(closeClip);
        yield return StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.5f, false, true));

        if(toMainMenu)
        {
            menuContainer.GetComponent<Image>().sprite = frontCover;

            ShowMainMenu(false);
        }
        else
        {
            menuContainer.GetComponent<Image>().sprite = backCover;

            ShowSettings(false);
        }

        GeneralUIController.PlayUISound(openClip);
        yield return StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.5f, false, true));
    }

    IEnumerator ShowUnshowCoroutine(Vector3 initalPos, Vector3 finalPos, float time, bool show, bool hiding = false)
    {
        if (show && !hiding)
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

        if(!hiding)
        {
            if (!show)
            {
                ShowMainMenu(false);
                menuContainer.SetActive(false);

                GeneralUIController.CurrentUI &= ~DisplayedUI.MainMenu;
            }
            else
            {
                GeneralUIController.CurrentUI |= DisplayedUI.MainMenu;
            }
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

    public void ShowMainMenu(bool changeCover)
    {
        if (changeCover)
        {
            StartCoroutine(ChangeCover(true));
        }
        else
        {
            mainMenu.SetActive(true);
            settingsMenu.SetActive(false);
            visualSettings.SetActive(false);
            audioSettings.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(mainMenu.GetComponent<RectTransform>());
        }
    }

    public void ShowSettings(bool changeCover)
    {
        if(changeCover)
        {
            StartCoroutine(ChangeCover(false));
        }
        else
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(true);
            visualSettings.SetActive(false);
            audioSettings.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(settingsMenu.GetComponent<RectTransform>());
        }
    }

    public void ShowControls()
    {
        GeneralUIController.ShowControlsUI();
    }

    public void ShowVisualSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        visualSettings.SetActive(true);
        audioSettings.SetActive(false);

        LayoutRebuilder.ForceRebuildLayoutImmediate(visualSettings.GetComponent<RectTransform>());

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

        LayoutRebuilder.ForceRebuildLayoutImmediate(audioSettings.GetComponent<RectTransform>());

        LoadVolumesFromMixers();
    }
    public void Exit()
    {
        Application.Quit();
    }

    void AddSliderListeners()
    {
        mainSlider.onValueChanged.AddListener(delegate { SetMainVolume(mainSlider.value); });
        musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicSlider.value); });
        SFXSlider.onValueChanged.AddListener(delegate { SetSFXVolume(SFXSlider.value); });
        ambienceSlider.onValueChanged.AddListener(delegate { SetAmbienceVolume(ambienceSlider.value); });
    }

    void LoadVolumesFromPlayerPrefs()
    {
        if(PlayerPrefs.HasKey("mainVolume"))
        {
            mainAudioMixer.SetFloat("mainVolume", PlayerPrefs.GetFloat("mainVolume"));
        }

        if (PlayerPrefs.HasKey("musicVolume"))
        {
            mainAudioMixer.SetFloat("musicVolume", PlayerPrefs.GetFloat("musicVolume"));
        }

        if (PlayerPrefs.HasKey("sfxVolume"))
        {
            mainAudioMixer.SetFloat("sfxVolume", PlayerPrefs.GetFloat("sfxVolume"));
        }

        if (PlayerPrefs.HasKey("ambienceVolume"))
        {
            mainAudioMixer.SetFloat("ambienceVolume", PlayerPrefs.GetFloat("ambienceVolume"));
        }

        LoadVolumesFromMixers();
    }

    void LoadVolumesFromMixers()
    {
        float mainVolume;
        mainAudioMixer.GetFloat("mainVolume", out mainVolume);
        mainSlider.value = mainVolume;

        float musicVolume;
        mainAudioMixer.GetFloat("musicVolume", out musicVolume);
        musicSlider.value = musicVolume;

        float sfxVolume;
        mainAudioMixer.GetFloat("sfxVolume", out sfxVolume);
        SFXSlider.value = sfxVolume;

        float ambienceVolume;
        mainAudioMixer.GetFloat("ambienceVolume", out ambienceVolume);
        ambienceSlider.value = ambienceVolume;
    }

    public void SetMainVolume(float volume)
    {
        mainAudioMixer.SetFloat("mainVolume", volume);
        PlayerPrefs.SetFloat("mainVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        mainAudioMixer.SetFloat("musicVolume", volume);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mainAudioMixer.SetFloat("sfxVolume", volume);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void SetAmbienceVolume(float volume)
    {
        mainAudioMixer.SetFloat("ambienceVolume", volume);
        PlayerPrefs.SetFloat("ambienceVolume", volume);
    }

    public void ResetVolumes()
    {
        mainSlider.value = 0;
        SetMainVolume(0);
        musicSlider.value = 0;
        SetMusicVolume(0);
        SFXSlider.value = 0;
        SetSFXVolume(0);
        ambienceSlider.value = 0;
        SetAmbienceVolume(0);
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

    public void PlayTappingSound()
    {
        if(GeneralUIController.displayingMainMenuUI)
        {
            int randNum = Random.Range(0, tappingClips.Length);
            GeneralUIController.PlayUISound(tappingClips[randNum]);
        }
    }

    #endregion

    IEnumerator NewGameCoroutine()
    {
        GeneralUIController.UnshowEverything();
        GeneralUIController.ShowLoadingUI(LoadingState.Loading);

        while(GeneralUIController.displayingMainMenuUI)
        {
            yield return null;
        }

        yield return StartCoroutine(DataManager.LoadNewGameData());

        GeneralUIController.UnshowLoadingUI();

        GameManager.StartNewGame();
    }
}
