using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the UI that shows the controls of the game
/// </summary>
public class ControlsUIController : MonoBehaviour
{
    #region Variables

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

    public RectTransform showingPosition;
    public RectTransform unshowingPosition;

    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip changePanelClip;

    public RectTransform[] panels;
    public RectTransform showingPanelPosition;
    public RectTransform unshowingPanelLeftPosition;
    public RectTransform unshowingPanelRightPosition;
    int currentPanel = 0;

    bool busy = false;

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

    #endregion

    /// <summary>
    /// It is executed each frame
    /// </summary>
    public void ControlsUIUpdate()
    {
        if(GeneralUIController.displayControlsUI)
        {
            if(InputManager.pressedEscape && !busy)
            {
                GeneralUIController.UnshowControlsUI();
            }
        }
    }

    /// <summary>
    /// Shows or unshows the UI
    /// </summary>
    /// <param name="show"></param>
    public void ShowUnshow(bool show)
    {
        if(show && !GeneralUIController.displayControlsUI)
        {
            GeneralUIController.PlayUISound(openClip);
            StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.5f, true));
        }
        else if(!show && GeneralUIController.displayControlsUI)
        {
            GeneralUIController.PlayUISound(closeClip);
            StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.5f, false));
        }
    }

    /// <summary>
    /// Coroutine that shows or unshows the UI
    /// </summary>
    /// <param name="initialPos"></param>
    /// <param name="finalPos"></param>
    /// <param name="time"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show)
    {
        busy = true;
        if(show)
        {
            container.SetActive(true);
            panels[currentPanel].gameObject.SetActive(true);
        }

        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.unscaledDeltaTime;

            ContainerRectTransform.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        ContainerRectTransform.position = finalPos;

        if(!show)
        {
            container.SetActive(false);
            panels[currentPanel].gameObject.SetActive(false);
            GeneralUIController.CurrentUI &= ~DisplayedUI.Controls;
        }
        else
        {
            GeneralUIController.CurrentUI |= DisplayedUI.Controls;
        }

        busy = false;
    }

    /// <summary>
    /// It is executed when left arrow is clicked
    /// </summary>
    public void OnClickLeftArrow()
    {
        if(!busy)
        {
            int nextPanelIndex = currentPanel - 1;
            if (nextPanelIndex < 0) nextPanelIndex = panels.Length - 1;

            StartCoroutine(ChangePanel(nextPanelIndex, false));
        }
    }

    /// <summary>
    /// It is executed when right arrow is clicked
    /// </summary>
    public void OnClickRightArrow()
    {
        if(!busy)
        {
            int nextPanelIndex = currentPanel + 1;
            if (nextPanelIndex >= panels.Length) nextPanelIndex = 0;

            StartCoroutine(ChangePanel(nextPanelIndex, true));
        }
    }

    /// <summary>
    /// Coroutine that changes a control panel for the next one or the previous one
    /// </summary>
    /// <param name="nextPanelIndex"></param>
    /// <param name="moveRight"></param>
    /// <returns></returns>
    IEnumerator ChangePanel(int nextPanelIndex, bool moveRight)
    {
        busy = true;

        GeneralUIController.PlayUISound(changePanelClip);
        RectTransform nextPanel = panels[nextPanelIndex];
        Vector3 nextPanelInitialPosition = moveRight ? unshowingPanelLeftPosition.position : unshowingPanelRightPosition.position;
        Vector3 currentPanelFinalPosition = moveRight ? unshowingPanelRightPosition.position : unshowingPanelLeftPosition.position;

        nextPanel.position = nextPanelInitialPosition;
        nextPanel.gameObject.SetActive(true);

        yield return StartCoroutine(MovePanel(panels[currentPanel], showingPosition.position, currentPanelFinalPosition, 0.25f));
        StartCoroutine(MovePanel(nextPanel, nextPanelInitialPosition, showingPosition.position, 0.25f));

        panels[currentPanel].gameObject.SetActive(false);
        currentPanel = nextPanelIndex;

        busy = false;
    }

    /// <summary>
    /// Coroutine that moves a panel from a position to another during a time
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="initialPosition"></param>
    /// <param name="finalPosition"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator MovePanel(RectTransform panel, Vector3 initialPosition, Vector3 finalPosition, float time)
    {
        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.unscaledDeltaTime;

            panel.position = Vector3.Lerp(initialPosition, finalPosition, elapsedTime / time);

            yield return null;
        }
        panel.position = finalPosition;
    }
}
