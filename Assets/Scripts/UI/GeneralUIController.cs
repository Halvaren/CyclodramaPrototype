using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Accumulative enum useful for knowing which UIs are displayed
/// </summary>
[Flags]
public enum DisplayedUI
{
    Gameplay = 1, Dialogue = 2, Inventory = 4, Detailed = 8, Pause = 16, Data = 32, MainMenu = 64, Loading = 128, Controls = 256
}

public class GeneralUIController : MonoBehaviour
{
    #region Variables

    public static GeneralUIController instance;

    public MainMenuUIController mainMenuUIController;
    public DialogueUIController dialogueUIController;
    public ActionVerbsUIController actionVerbsUIController;
    public InventoryUIController inventoryUIController;
    public DetailedUIController detailedUIController;
    public PauseUIController pauseUIController;
    public DataUIController dataUIController;
    public LoadingUIController loadingUIController;
    public ControlsUIController controlsUIController;

    [HideInInspector]
    public bool displayNothing = false;

    private AudioManager audioManager;
    public AudioManager AudioManager
    {
        get
        {
            if (audioManager == null) audioManager = AudioManager.instance;
            return audioManager;
        }
    }

    private DisplayedUI currentUI;
    public DisplayedUI CurrentUI
    {
        get { return currentUI; }
        set 
        { 
            currentUI = value;
            //If there's no UI display and it is not specifically said that no UI must be displayed
            if (currentUI == 0 && !displayNothing)
            {
                //The action verb bar is displayed
                ShowGameplayUI();
            }
        }
    }

    public bool displayingGameplayUI
    {
        get { return (CurrentUI & DisplayedUI.Gameplay) > 0; }
    }

    public bool displayingInventoryUI
    {
        get { return (CurrentUI & DisplayedUI.Inventory) > 0; }
    }

    public bool displayingDialogueUI
    {
        get { return (CurrentUI & DisplayedUI.Dialogue) > 0; }
    }

    public bool displayingDetailedUI
    {
        get { return (CurrentUI & DisplayedUI.Detailed) > 0; }
    }

    public bool displayingPauseUI
    {
        get { return (CurrentUI & DisplayedUI.Pause) > 0; }
    }

    public bool displayingDataUI
    {
        get { return (CurrentUI & DisplayedUI.Data) > 0; }
    }

    public bool displayingMainMenuUI
    {
        get { return (CurrentUI & DisplayedUI.MainMenu) > 0; }
    }

    public bool displayingLoadingUI
    {
        get { return (CurrentUI & DisplayedUI.Loading) > 0; }
    }

    public bool displayControlsUI
    {
        get { return (CurrentUI & DisplayedUI.Controls) > 0; }
    }

    #endregion

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Executes update methods of many of the UIs
    /// </summary>
    private void Update()
    {
        pauseUIController.PauseUpdate();
        dataUIController.DataUIUpdate();
        controlsUIController.ControlsUIUpdate();
        if(!displayingPauseUI)
        {
            actionVerbsUIController.ActionVerbsUpdate();
            dialogueUIController.DialogueUpdate();
        }
    }

    /// <summary>
    /// Shows Action bar UI
    /// </summary>
    public void ShowGameplayUI()
    {
        actionVerbsUIController.ChangeVisbility(ActionBarVisibility.HalfShown, true);
    }

    /// <summary>
    /// Unshows Action bar UI
    /// </summary>
    public void UnshowGameplayUI()
    {
        actionVerbsUIController.ChangeVisbility(ActionBarVisibility.Unshown, true);
    }
    
    /// <summary>
    /// Coroutine that unshows the Action bar and lasts until it is finished
    /// </summary>
    /// <returns></returns>
    public IEnumerator UnshowGameplayUICoroutine()
    {
        yield return StartCoroutine(actionVerbsUIController.ChangeVisbilityCoroutine(ActionBarVisibility.Unshown, true, true));
    }

    /// <summary>
    /// Shows the inventory UI
    /// </summary>
    /// <param name="showActionVerbs"></param>
    public void ShowInventoryUI(bool showActionVerbs = true)
    {
        if(showActionVerbs) actionVerbsUIController.ChangeVisbility(ActionBarVisibility.FullShown, true);
        inventoryUIController.ShowUnshow(true);
    }

    /// <summary>
    /// Unshows the inventory UI
    /// </summary>
    /// <param name="showActionVerbs"></param>
    public void UnshowInventoryUI(bool showActionVerbs = true)
    {
        if (showActionVerbs) actionVerbsUIController.ChangeVisbility(ActionBarVisibility.HalfShown, true);
        inventoryUIController.ShowUnshow(false);
    }

    /// <summary>
    /// Shows the dialogue UI
    /// </summary>
    public void ShowDialogueUI()
    {
        dialogueUIController.ShowUnshow(true);
    }

    /// <summary>
    /// Unshows the dialogue UI
    /// </summary>
    public void UnshowDialogueUI()
    {
        dialogueUIController.ShowUnshow(false);
    }

    /// <summary>
    /// Shows a detailed UI, depending on the behavior passed as a parameter, and returns it
    /// </summary>
    /// <param name="behavior"></param>
    /// <returns></returns>
    public DetailedUIBase ShowDetailedUI(DetailedObjBehavior behavior = null)
    {
        DetailedUIBase detailedUI = detailedUIController.ShowUnshow(true, behavior);
        CurrentUI |= DisplayedUI.Detailed;
        return detailedUI;
    }

    /// <summary>
    /// Unshows the displayed detailed UI
    /// </summary>
    public void UnshowDetailedUI()
    {
        detailedUIController.ShowUnshow(false);
        CurrentUI &= ~DisplayedUI.Detailed;
    }

    /// <summary>
    /// Shows the pause menu UI
    /// </summary>
    public void ShowPauseUI()
    {
        pauseUIController.ShowUnshow(true);
    }

    /// <summary>
    /// Unshows the pause menu UI
    /// </summary>
    public void UnshowPauseUI()
    {
        pauseUIController.ShowUnshow(false);
    }

    /// <summary>
    /// Shows the data menu UI
    /// </summary>
    /// <param name="saving"></param>
    public void ShowDataUI(bool saving)
    {
        dataUIController.ShowUnshow(true, saving);
    }

    /// <summary>
    /// Unshows the data menu UI
    /// </summary>
    public void UnshowDataUI()
    {
        dataUIController.ShowUnshow(false);
    }

    /// <summary>
    /// Shows the main menu UI
    /// </summary>
    public void ShowMainMenuUI()
    {
        mainMenuUIController.ShowUnshow(true);
    }

    /// <summary>
    /// Unshows the main menu UI
    /// </summary>
    public void UnshowMainMenuUI()
    {
        mainMenuUIController.ShowUnshow(false);
    }

    /// <summary>
    /// Shows the loading UI
    /// </summary>
    /// <param name="state"></param>
    public void ShowLoadingUI(LoadingState state)
    {
        loadingUIController.ShowUnshow(true, state);
    }

    /// <summary>
    /// Unshows the loading UI
    /// </summary>
    public void UnshowLoadingUI()
    {
        loadingUIController.ShowUnshow(false, LoadingState.Loading);
    }

    /// <summary>
    /// Shows the controls UI
    /// </summary>
    public void ShowControlsUI()
    {
        controlsUIController.ShowUnshow(true);
    }

    /// <summary>
    /// Unshows the controls UI
    /// </summary>
    public void UnshowControlsUI()
    {
        controlsUIController.ShowUnshow(false);
    }

    /// <summary>
    /// Unshows all the UIs
    /// </summary>
    public void UnshowEverything()
    {
        displayNothing = true;
        UnshowDetailedUI();
        UnshowDataUI();
        UnshowDialogueUI();
        UnshowGameplayUI();
        UnshowInventoryUI(false);
        UnshowMainMenuUI();
        UnshowPauseUI();
        UnshowLoadingUI();
        UnshowControlsUI();
    }

    /// <summary>
    /// Plays any UI sound passed as a parameter
    /// </summary>
    /// <param name="audioClip"></param>
    /// <param name="loop"></param>
    /// <returns></returns>
    public AudioSource PlayUISound(AudioClip audioClip, bool loop = false)
    {
        return AudioManager.PlaySound(audioClip, SoundType.UI, loop);
    }

    /// <summary>
    /// Stops the source passed as a parameter
    /// </summary>
    /// <param name="source"></param>
    public void StopUISound(AudioSource source)
    {
        source.Stop();
    }

    /// <summary>
    /// Fades out the source passed as a parameter
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fadeTime"></param>
    /// <param name="finalVolume"></param>
    public void StopUISound(AudioSource source, float fadeTime, float finalVolume = 0)
    {
        AudioManager.FadeOutSound(source, fadeTime, finalVolume);
    }
}
