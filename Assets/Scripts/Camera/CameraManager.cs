using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Camera detailCamera;

    [Header("Main virtual cameras")]
    public CinemachineVirtualCameraBase introCamera;
    public CinemachineVirtualCamera setCamera;
    public CinemachineVirtualCamera projectionCamera;

    public Animator Animator;

    public AudioClip projectionScreenOpenClip;
    public AudioClip projectionScreenCloseClip;

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

    private AudioManager audioManager;
    public AudioManager AudioManager
    {
        get
        {
            if (audioManager == null) audioManager = AudioManager.instance;
            return audioManager;
        }
    }

    public static CameraManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void FromIntroToMainCamera()
    {
        Animator.SetTrigger("setCamera");
    }

    public void FromMainToIntroCamera()
    {
        Animator.SetTrigger("introCamera");
    }

    public void FromProjectionToMainCamera()
    {
        currentDetailCamera.DeactivateCamera();
        currentDetailCamera = null;

        CursorManager.ActivateDetailCameraStuff(false);

        Animator.SetTrigger("setCamera");
        if (!screenHidden) 
        {
            AudioManager.PlaySound(projectionScreenCloseClip, SoundType.MetaTheater);
            StartCoroutine(ShowHideProjectorScreen(shownScreenPosition.position, hiddenScreenPosition.position, 0.5f)); 
        }
        usingMainCamera = true;

        if(PCController.IsEnableGameplayInput) PCController.EnableMovementInput(true);
        PCController.EnableInventoryInput(true);
    }

    public void FromMainToProjectCamera(DetailCameraBehavior detailCamera, bool freeCamera = true)
    {
        currentDetailCamera = detailCamera;
        currentDetailCamera.ActivateCamera();

        if(freeCamera)
            CursorManager.ActivateDetailCameraStuff(true);

        Animator.SetTrigger("projectorCamera");
        if (screenHidden)
        {
            AudioManager.PlaySound(projectionScreenOpenClip, SoundType.MetaTheater);
            StartCoroutine(ShowHideProjectorScreen(hiddenScreenPosition.position, shownScreenPosition.position, 0.5f, freeCamera));
        }
        usingMainCamera = false;
    }

    public void LockUnlockCurrentDetailCamera(bool unlock)
    {
        if (currentDetailCamera != null)
        {
            if(currentDetailCamera.LockUnlockCamera(unlock))
                CursorManager.ActivateDetailCameraStuff(unlock);
        }
    }

    IEnumerator ShowHideProjectorScreen(Vector3 initialPos, Vector3 finalPos, float time, bool freeCamera = true)
    {
        if(freeCamera && currentDetailCamera != null) currentDetailCamera.LockUnlockCamera(false);

        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            projectorScreen.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        projectorScreen.position = finalPos;

        screenHidden = !screenHidden;

        if (freeCamera && currentDetailCamera != null) currentDetailCamera.LockUnlockCamera(true);
    }
}
