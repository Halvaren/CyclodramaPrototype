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

    bool screenHidden = true;

    public bool usingMainCamera = true;

    public DetailCameraBehavior currentDetailCamera;

    private CursorManager cursorManager;
    public CursorManager CursorManager
    {
        get
        {
            if (cursorManager == null) cursorManager = CursorManager.instance;
            return cursorManager;
        }
    }

    private PCController pcController;
    public PCController PCController
    {
        get
        {
            if (pcController == null) pcController = PCController.instance;
            return pcController;
        }
    }

    public static CameraManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeToMainCamera()
    {
        currentDetailCamera.DeactivateCamera();
        currentDetailCamera = null;

        PCController.MakeInvisible(false);
        CursorManager.ActivateDetailCameraStuff(false);

        Animator.SetTrigger("setCamera");
        if (!screenHidden) StartCoroutine(ShowHideProjectorScreen(shownScreenPosition.position, hiddenScreenPosition.position, 0.5f));
        usingMainCamera = true;
    }

    public void ChangeToProjectorCamera(DetailCameraBehavior detailCamera)
    {
        currentDetailCamera = detailCamera;
        currentDetailCamera.ActivateCamera();

        PCController.MakeInvisible(true);
        CursorManager.ActivateDetailCameraStuff(true);

        Animator.SetTrigger("projectorCamera");
        if (screenHidden) StartCoroutine(ShowHideProjectorScreen(hiddenScreenPosition.position, shownScreenPosition.position, 0.5f));
        usingMainCamera = false;
    }

    public void LockUnlockCurrentDetailCamera(bool unlock)
    {
        if (currentDetailCamera != null)
        {
            currentDetailCamera.LockUnlockCamera(unlock);
            CursorManager.ActivateDetailCameraStuff(unlock);
        }
    }

    IEnumerator ShowHideProjectorScreen(Vector3 initialPos, Vector3 finalPos, float time)
    {
        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            projectorScreen.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        projectorScreen.position = finalPos;

        screenHidden = !screenHidden;
    }
}
