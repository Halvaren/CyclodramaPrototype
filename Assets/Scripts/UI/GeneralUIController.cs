using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Flags]
public enum DisplayedUI
{
    Gameplay = 1, Dialogue = 2, Inventory = 4, Detailed = 8, Pause = 16, Data = 32, MainMenu = 64, Loading = 128
}

public class GeneralUIController : MonoBehaviour
{
    public static GeneralUIController instance;

    public MainMenuUIController mainMenuUIController;
    public DialogueUIController dialogueUIController;
    public ActionVerbsUIController actionVerbsUIController;
    public InventoryUIController inventoryUIController;
    public DetailedUIController detailedUIController;
    public PauseUIController pauseUIController;
    public DataUIController dataUIController;
    public LoadingUIController loadingUIController;

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
            if (currentUI == 0 && !displayNothing)
            {
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

    void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        pauseUIController.PauseUpdate();
        dataUIController.DataUIUpdate();
        if(!displayingPauseUI)
        {
            actionVerbsUIController.ActionVerbsUpdate();
            dialogueUIController.DialogueUpdate();
        }
    }

    public void ShowGameplayUI()
    {
        actionVerbsUIController.ChangeVisbility(ActionBarVisibility.HalfShown, true);
    }

    public void UnshowGameplayUI()
    {
        actionVerbsUIController.ChangeVisbility(ActionBarVisibility.Unshown, true);
    }

    public void ShowInventoryUI(bool showActionVerbs = true)
    {
        if(showActionVerbs) actionVerbsUIController.ChangeVisbility(ActionBarVisibility.FullShown, true);
        inventoryUIController.ShowUnshow(true);
    }

    public void UnshowInventoryUI(bool showActionVerbs = true)
    {
        if (showActionVerbs) actionVerbsUIController.ChangeVisbility(ActionBarVisibility.HalfShown, true);
        inventoryUIController.ShowUnshow(false);
    }

    public void ShowDialogueUI()
    {
        dialogueUIController.ShowUnshow(true);
    }

    public void UnshowDialogueUI()
    {
        dialogueUIController.ShowUnshow(false);
    }

    public DetailedUIBase ShowDetailedUI(DetailedObjBehavior behavior = null)
    {
        DetailedUIBase detailedUI = detailedUIController.ShowUnshow(true, behavior);
        CurrentUI |= DisplayedUI.Detailed;
        return detailedUI;
    }

    public void UnshowDetailedUI()
    {
        detailedUIController.ShowUnshow(false);
        CurrentUI &= ~DisplayedUI.Detailed;
    }

    public void ShowPauseUI()
    {
        pauseUIController.ShowUnshow(true);
    }

    public void UnshowPauseUI()
    {
        pauseUIController.ShowUnshow(false);
    }

    public void ShowDataUI(bool saving)
    {
        dataUIController.ShowUnshow(true, saving);
    }

    public void UnshowDataUI()
    {
        dataUIController.ShowUnshow(false);
    }

    public void ShowMainMenuUI()
    {
        mainMenuUIController.ShowUnshow(true);
    }

    public void UnshowMainMenuUI()
    {
        mainMenuUIController.ShowUnshow(false);
    }

    public void ShowLoadingUI(LoadingState state)
    {
        loadingUIController.ShowUnshow(true, state);
    }

    public void UnshowLoadingUI()
    {
        loadingUIController.ShowUnshow(false, LoadingState.Loading);
    }

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
    }

    public AudioSource PlayUISound(AudioClip audioClip, bool loop = false)
    {
        return AudioManager.PlaySound(audioClip, SoundType.UI, loop);
    }

    public void StopUISound(AudioSource source)
    {
        source.Stop();
    }

    public void StopUISound(AudioSource source, float fadeTime, float finalVolume = 0)
    {
        AudioManager.FadeOutSound(source, fadeTime, finalVolume);
    }
}
