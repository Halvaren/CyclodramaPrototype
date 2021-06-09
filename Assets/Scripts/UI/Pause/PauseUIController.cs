using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class PauseUIController : MonoBehaviour
{
    #region General variables

    public Image pauseBackground;
    public GameObject pauseContainer;
    private RectTransform pauseContainerRectTransform;
    public RectTransform PauseContainerRectTransform
    {
        get
        {
            if (pauseContainerRectTransform == null) pauseContainerRectTransform = pauseContainer.GetComponent<RectTransform>();
            return pauseContainerRectTransform;
        }
    }

    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject visualSettings;
    public GameObject audioSettings;

    public Button loadButton;
    public Button saveButton;

    public RectTransform showingPosition;
    public RectTransform unshowingPosition;

    public Image charactersShowOffImage;
    public Sprite[] characterSprites;
    int currentCharacterSprite = -1;

    public float timeBetweenSprites = 10f;
    public float fadeSpriteTime = 1f;
    float elapsedTimeBetweenSprites = 0.0f;

    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip[] writingClips;

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

    bool wasInventoryInputEnabled = false;
    Coroutine showingCoroutine;

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

    #region General methods

    private void Start()
    {
        AddSliderListeners();
        LoadVolumesFromPlayerPrefs();

        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> resolutionsStrings = new List<string>();

        int currentResolutionIndex = 0;
        for(int i = 0; i < resolutions.Length; i++)
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

    public void PauseUpdate()
    {
        if(GeneralUIController.displayingPauseUI && !GeneralUIController.displayingDataUI && !GeneralUIController.displayControlsUI)
        {
            if (InputManager.pressedEscape)
            {
                GeneralUIController.UnshowPauseUI();
            }

            elapsedTimeBetweenSprites += Time.unscaledDeltaTime;

            if(elapsedTimeBetweenSprites > timeBetweenSprites)
            {
                elapsedTimeBetweenSprites = 0;
                ChangeCharacterSprite(true);
            }
        }
    }

    public void ShowUnshow(bool show)
    {
        if (showingCoroutine != null) return;

        if (show && !GeneralUIController.displayingPauseUI)
        {
            GeneralUIController.PlayUISound(openClip);
            showingCoroutine = StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.0f, 0.9f, 0.2f, 0.3f, true));
        }
        else if (!show && GeneralUIController.displayingPauseUI)
        {
            GeneralUIController.PlayUISound(closeClip);
            showingCoroutine = StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.9f, 0.0f, 0.2f, 0.3f, false));
        }
    }

    IEnumerator ShowUnshowCoroutine(Vector3 initalPos, Vector3 finalPos, float initialAlpha, float finalAlpha, float fadingTime, float displacementTime, bool show)
    {
        if (show)
        {
            pauseContainer.SetActive(true);
            pauseBackground.raycastTarget = true;

            wasInventoryInputEnabled = PCController.instance.IsEnableInventoryInput;

            loadButton.interactable = GeneralUIController.dataUIController.AreThereFiles();
            saveButton.interactable = PCController.instance.IsEnableGameplayInput;

            ChangeCharacterSprite(false);
            elapsedTimeBetweenSprites = 0.0f;

            PCController.instance.EnableGameplayInput(false);
            PCController.instance.EnableInventoryInput(false);
        }
        else 
        { 
            Time.timeScale = 1f;
        }

        float elapsedTime = 0.0f;
        float time = fadingTime > displacementTime ? fadingTime : displacementTime;

        float alpha = initialAlpha;
        Color pauseBackgroundColor = new Color(pauseBackground.color.r, pauseBackground.color.g, pauseBackground.color.b, alpha);
        pauseBackground.color = pauseBackgroundColor;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime < fadingTime)
            {
                alpha = Mathf.Lerp(initialAlpha, finalAlpha, elapsedTime / fadingTime);
                pauseBackgroundColor.a = alpha;
                pauseBackground.color = pauseBackgroundColor;
            }

            if (elapsedTime < displacementTime)
            {
                PauseContainerRectTransform.position = Vector3.Lerp(initalPos, finalPos, elapsedTime / displacementTime);
            }

            yield return null;
        }
        pauseBackgroundColor.a = finalAlpha;
        pauseBackground.color = pauseBackgroundColor;
        PauseContainerRectTransform.position = finalPos;

        if (!show)
        {
            ShowMainMenu();

            pauseContainer.SetActive(false);
            pauseBackground.raycastTarget = false;

            PCController.instance.EnableGameplayInput(true);
            PCController.instance.EnableInventoryInput(wasInventoryInputEnabled);

            GeneralUIController.CurrentUI &= ~DisplayedUI.Pause;
        }
        else
        {
            Time.timeScale = 0f;

            GeneralUIController.CurrentUI |= DisplayedUI.Pause;
        }

        showingCoroutine = null;
    }

    #endregion

    #region Callback methods

    public void ResumeGame()
    {
        GeneralUIController.UnshowPauseUI();
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

        LoadVolumesFromMixers();
    }

    public void SaveGame()
    {
        GeneralUIController.ShowDataUI(true);
    }

    public void LoadGame()
    {
        GeneralUIController.ShowDataUI(false);
    }

    public void BackToMenu()
    {
        GameManager.BackToMainMenu();
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
        if (PlayerPrefs.HasKey("mainVolume"))
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
        if(QualitySettings.GetQualityLevel() != qualityIndex)
            QualitySettings.SetQualityLevel(qualityIndex);
        if (Screen.fullScreen != isFullScreen)
            Screen.fullScreen = isFullScreen;
        if(Screen.currentResolution.width != chosenResolution.width || Screen.currentResolution.height != chosenResolution.height)
            Screen.SetResolution(chosenResolution.width, chosenResolution.height, Screen.fullScreen);

        applyButton.interactable = false;
    }

    public void PlayWritingSound()
    {
        if (GeneralUIController.displayingPauseUI)
        {
            int randNum = Random.Range(0, writingClips.Length);
            GeneralUIController.PlayUISound(writingClips[randNum]);
        }
    }

    #endregion

    void ChangeCharacterSprite(bool fade)
    {
        int randNum = Random.Range(0, characterSprites.Length);
        if(randNum == currentCharacterSprite)
        {
            randNum += (int) Mathf.Pow(-1, Random.Range(0, 1));
            if (randNum >= characterSprites.Length) randNum = 0;
            else if (randNum < 0) randNum = characterSprites.Length - 1;
        }

        if(fade)
        {
            StartCoroutine(FadeCharacterImage(randNum, fadeSpriteTime));
        }
        else
        {
            charactersShowOffImage.sprite = characterSprites[randNum];
            currentCharacterSprite = randNum;
        }
    }

    IEnumerator FadeCharacterImage(int randNum, float time)
    {
        float initialAlpha = 1f;
        float finalAlpha = 0f;

        float elapsedTime = 0.0f;
        Color c = charactersShowOffImage.color;

        while(elapsedTime < time)
        {
            elapsedTime += Time.unscaledDeltaTime;

            c.a = Mathf.Lerp(initialAlpha, finalAlpha, elapsedTime / time);
            charactersShowOffImage.color = c;

            yield return null;
        }
        c.a = finalAlpha;
        charactersShowOffImage.color = c;

        charactersShowOffImage.sprite = characterSprites[randNum];
        currentCharacterSprite = randNum;

        elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.unscaledDeltaTime;

            c.a = Mathf.Lerp(finalAlpha, initialAlpha, elapsedTime / time);
            charactersShowOffImage.color = c;

            yield return null;
        }
        c.a = initialAlpha;
        charactersShowOffImage.color = c;
    }
}
