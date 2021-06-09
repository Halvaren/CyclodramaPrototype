using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsUIController : MonoBehaviour
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

    IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show)
    {
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
    }

    public void OnClickLeftArrow()
    {
        int nextPanelIndex = currentPanel - 1;
        if (nextPanelIndex < 0) nextPanelIndex = panels.Length - 1;

        StartCoroutine(ChangePanel(nextPanelIndex, false));
    }

    public void OnClickRightArrow()
    {
        int nextPanelIndex = currentPanel + 1;
        if (nextPanelIndex >= panels.Length) nextPanelIndex = 0;

        StartCoroutine(ChangePanel(nextPanelIndex, true));
    }

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
