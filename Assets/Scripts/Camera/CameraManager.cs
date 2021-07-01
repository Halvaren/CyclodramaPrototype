using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Manages all cameras (change between cameras and shows and unshows metatheater projection screen
/// </summary>
public class CameraManager : MonoBehaviour
{
    #region Variables

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

    #endregion

    #region Methods

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Changes from intro camera to game main camera
    /// </summary>
    public void FromIntroToMainCamera()
    {
        Animator.SetTrigger("setCamera");
    }

    /// <summary>
    /// Changes from game main camera to intro camera
    /// </summary>
    public void FromMainToIntroCamera()
    {
        Animator.SetTrigger("introCamera");
    }

    /// <summary>
    /// Changes from projection camera to game main camera
    /// </summary>
    public void FromProjectionToMainCamera()
    {
        //Deactivates the current DetailCameraBehavior
        currentDetailCamera.DeactivateCamera();
        currentDetailCamera = null;

        //Changes cursor settings
        CursorManager.ActivateDetailCameraStuff(false);

        Animator.SetTrigger("setCamera");
        if (!screenHidden) 
        {
            //Unshows projection screen
            AudioManager.PlaySound(projectionScreenCloseClip, SoundType.MetaTheater);
            StartCoroutine(ShowHideProjectorScreen(shownScreenPosition.position, hiddenScreenPosition.position, 0.5f)); 
        }
        usingMainCamera = true;

        //Enables PC inputs
        if(PCController.IsEnableGameplayInput) PCController.EnableMovementInput(true);
        PCController.EnableInventoryInput(true);
    }

    /// <summary>
    /// Changes from game main camera to projection camera
    /// </summary>
    /// <param name="detailCamera"></param>
    /// <param name="freeCamera"></param>
    public void FromMainToProjectCamera(DetailCameraBehavior detailCamera, bool freeCamera = true)
    {
        //Activates the new current DetailCameraBehavior
        currentDetailCamera = detailCamera;
        currentDetailCamera.ActivateCamera();

        //Changes cursor settings
        if(freeCamera)
            CursorManager.ActivateDetailCameraStuff(true);

        Animator.SetTrigger("projectorCamera");
        if (screenHidden)
        {
            //Shows projection screen
            AudioManager.PlaySound(projectionScreenOpenClip, SoundType.MetaTheater);
            StartCoroutine(ShowHideProjectorScreen(hiddenScreenPosition.position, shownScreenPosition.position, 0.5f, freeCamera));
        }
        usingMainCamera = false;
    }

    /// <summary>
    /// When inventory is opened, for example, and a detail camera is active, cursor settings must change
    /// </summary>
    /// <param name="unlock">True means cursor is free and camera lock, i.e, when a menu is opened with a detail camera active</param>
    public void LockUnlockCurrentDetailCamera(bool unlock)
    {
        if (currentDetailCamera != null)
        {
            if(currentDetailCamera.LockUnlockCamera(unlock))
                CursorManager.ActivateDetailCameraStuff(unlock);
        }
    }

    /// <summary>
    /// Opens or closes projection screen
    /// </summary>
    /// <param name="initialPos"></param>
    /// <param name="finalPos"></param>
    /// <param name="time"></param>
    /// <param name="freeCamera"></param>
    /// <returns></returns>
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

    #endregion
}
