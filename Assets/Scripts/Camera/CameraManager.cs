using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Camera detailCamera;

    public Animator Animator;

    public Transform projectorScreen;
    public Transform hiddenScreenPosition;
    public Transform shownScreenPosition;

    bool screenMoving = false;
    bool screenHidden = true;

    public bool usingMainCamera = true;

    public static CameraManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeToMainCamera()
    {
        Animator.SetTrigger("setCamera");
        if (!screenHidden) StartCoroutine(ShowHideProjectorScreen(shownScreenPosition.position, hiddenScreenPosition.position, 0.5f));
        usingMainCamera = true;
    }

    public void ChangeToProjectorCamera()
    {
        Animator.SetTrigger("projectorCamera");
        if (screenHidden) StartCoroutine(ShowHideProjectorScreen(hiddenScreenPosition.position, shownScreenPosition.position, 0.5f));
        usingMainCamera = false;
    }

    IEnumerator ShowHideProjectorScreen(Vector3 initialPos, Vector3 finalPos, float time)
    {
        screenMoving = true;

        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            projectorScreen.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        projectorScreen.position = finalPos;

        screenMoving = false;
        screenHidden = !screenHidden;
    }
}
