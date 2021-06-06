using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum LoadingState
{
    Loading, Saving, Autosaving
}

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
