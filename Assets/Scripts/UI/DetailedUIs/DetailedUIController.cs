using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the detailed UI
/// </summary>
public class DetailedUIController : MonoBehaviour
{
    public NumLockUIController numLockUIController;
    public DefaultDetailedUIController defaultDetailedUIController;

    /// <summary>
    /// Shows or unshows the detailed UI corresponding to the behavior passed as a parameter
    /// </summary>
    /// <param name="show"></param>
    /// <param name="behavior"></param>
    /// <returns></returns>
    public DetailedUIBase ShowUnshow(bool show, DetailedObjBehavior behavior = null)
    {
        DetailedUIBase detailedUI = null;

        if(show)
        {
            if(behavior is NumLockObjBehavior)
            {
                detailedUI = numLockUIController;
                numLockUIController.InitializeUI((NumLockObjBehavior)behavior);
            }
            else
            {
                detailedUI = defaultDetailedUIController;
                defaultDetailedUIController.InitializeUI(behavior, behavior.obj.GetName());
            }

            if (detailedUI != null)
            {
                detailedUI.ShowUnshow(true);
            }
        }
        else
        {
            if(numLockUIController.showing)
            {
                detailedUI = numLockUIController;
                numLockUIController.behavior = null;
            }
            else if(defaultDetailedUIController.showing)
            {
                detailedUI = defaultDetailedUIController;
                defaultDetailedUIController.behavior = null;
            }

            if(detailedUI != null)
                detailedUI.ShowUnshow(false);
        }

        return detailedUI;
    }
}

/// <summary>
/// Basic detailed UI
/// </summary>
public class DetailedUIBase : MonoBehaviour
{
    public GameObject container;
    private RectTransform containerRectTransform;
    public RectTransform ContainerRectTransform
    {
        get
        {
            if (containerRectTransform == null) containerRectTransform = container.GetComponent<RectTransform>();
            return containerRectTransform;
        }
    }

    [HideInInspector]
    public DetailedObjBehavior behavior;

    public RectTransform shownPosition;
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

    [HideInInspector]
    public bool showing = false;

    /// <summary>
    /// Shows or unshows the detailed UI
    /// </summary>
    /// <param name="show"></param>
    public void ShowUnshow(bool show)
    {
        if(show)
        {
            GeneralUIController.PlayUISound(openClip);
            StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, shownPosition.position, 0.5f, show));
        }
        else
        {
            GeneralUIController.PlayUISound(closeClip);
            StartCoroutine(ShowUnshowCoroutine(shownPosition.position, unshowingPosition.position, 0.5f, show));
        }
    }

    /// <summary>
    /// Coroutine that shows or unshows the detailed UI
    /// </summary>
    /// <param name="initialPos"></param>
    /// <param name="finalPos"></param>
    /// <param name="time"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public virtual IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show)
    {
        if (show)
        {
            container.SetActive(true);
        }

        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            ContainerRectTransform.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        ContainerRectTransform.position = finalPos;

        if (!show)
        {
            container.SetActive(false);
            showing = false;
        }
        else
        {
            showing = true;
        }
    }

    /// <summary>
    /// Method to override, useful for blocking the input of the interactable elements of the UI
    /// </summary>
    /// <param name="value"></param>
    public virtual void BlockInput(bool value)
    {

    }
}
