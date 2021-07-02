using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Why is loading
/// </summary>
public enum LoadingState
{
    Loading, Saving, Autosaving
}

/// <summary>
/// Manages the UI of loading indication
/// </summary>
public class LoadingUIController : MonoBehaviour
{
    public GameObject loadingPair;

    public TextMeshProUGUI loadingText;
    public string loadingBaseText = "Loading...";
    public string savingBaseText = "Saving...";
    public string autosavingBaseText = "Autosaving...";

    public void Start()
    {
        loadingPair.SetActive(false);
    }

    /// <summary>
    /// Shows or unshows the UI
    /// </summary>
    /// <param name="show"></param>
    /// <param name="state"></param>
    public void ShowUnshow(bool show, LoadingState state)
    {
        if(show)
        {
            loadingText.text = GetLoadingText(state);
            loadingPair.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(loadingPair.GetComponent<RectTransform>());
        }
        else
        {
            loadingPair.SetActive(false);
        }
    }

    /// <summary>
    /// Returns the corresponding text to the LoadingState passed as a parameter
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    string GetLoadingText(LoadingState state)
    {
        switch(state)
        {
            case LoadingState.Loading:
                return loadingBaseText;
            case LoadingState.Saving:
                return savingBaseText;
            case LoadingState.Autosaving:
                return autosavingBaseText;
        }
        return "";
    }
}
