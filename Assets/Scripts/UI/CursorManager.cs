using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Appearance state of the cursor
/// </summary>
public enum CursorState
{
    Normal, Highlighted, Disable
}

/// <summary>
/// Manages the changes in the cursor (locking it, appearance)
/// </summary>
public class CursorManager : MonoBehaviour
{
    #region Variables

    public Vector2 hotSpot = new Vector2(0.81f, 0.85f);

    public Texture2D defaultCursor;
    public Texture2D defaultCursor_Disable;
    public Texture2D defaultCursor_HL;

    [HideInInspector]
    public Texture2D currentCursor;
    [HideInInspector]
    public Texture2D currentCursor_Disable;
    [HideInInspector]
    public Texture2D currentCursor_HL;

    CursorState cursorState;

    public GameObject detailCameraPOV;
    private Image povImage;
    public Image POVImage
    {
        get
        {
            if (povImage == null) povImage = detailCameraPOV.GetComponent<Image>();
            return povImage;
        }
    }

    public Sprite defaultPOV;
    public Sprite defaultPOV_Disable;
    public Sprite defaultPOV_HL;

    [HideInInspector]
    public Sprite currentPOV;
    [HideInInspector]
    public Sprite currentPOV_Disable;
    [HideInInspector]
    public Sprite currentPOV_HL;

    public static CursorManager instance;

    #endregion

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        detailCameraPOV.SetActive(false);
        ResetCursors();
    }

    /// <summary>
    /// Resets the appearance of the cursors
    /// </summary>
    public void ResetCursors()
    {
        cursorState = CursorState.Normal;
        SetCursors(defaultCursor, defaultCursor_Disable, defaultCursor_HL, defaultPOV, defaultPOV_Disable, defaultPOV_HL);
    }

    /// <summary>
    /// Sets the appearance of the cursors for each state
    /// </summary>
    /// <param name="normal"></param>
    /// <param name="disable"></param>
    /// <param name="hightlighted"></param>
    /// <param name="povNormal"></param>
    /// <param name="povDisable"></param>
    /// <param name="povHL"></param>
    public void SetCursors(Texture2D normal, Texture2D disable, Texture2D hightlighted, Sprite povNormal, Sprite povDisable, Sprite povHL)
    {
        currentCursor = normal;
        currentCursor_Disable = disable;
        currentCursor_HL = hightlighted;

        currentPOV = povNormal;
        currentPOV_Disable = povDisable;
        currentPOV_HL = povHL;

        SetCursor();
    }

    /// <summary>
    /// Set the appearance of the cursor acconding to the state
    /// </summary>
    void SetCursor()
    {
        switch (cursorState)
        {
            case CursorState.Normal:
                Cursor.SetCursor(currentCursor, hotSpot, CursorMode.Auto);
                if (detailCameraPOV.activeSelf)
                    POVImage.sprite = currentPOV;
                break;
            case CursorState.Highlighted:
                Cursor.SetCursor(currentCursor_HL, hotSpot, CursorMode.Auto);
                if (detailCameraPOV.activeSelf)
                    POVImage.sprite = currentPOV_HL;
                break;
            case CursorState.Disable:
                Cursor.SetCursor(currentCursor_Disable, hotSpot, CursorMode.Auto);
                if (detailCameraPOV.activeSelf)
                    POVImage.sprite = currentPOV_Disable;
                break;
        }
    }

    /// <summary>
    /// Changes cursors state
    /// </summary>
    /// <param name="cursorState"></param>
    public void ChangeCursorState(CursorState cursorState)
    {
        CursorState previousState = this.cursorState;
        this.cursorState = cursorState;

        if(previousState != this.cursorState)
            SetCursor();
    }

    /// <summary>
    /// Locks or unlocks and makes visible or invisible the mouse cursor, and makes visible or invisible the POV cursor
    /// </summary>
    /// <param name="value"></param>
    public void ActivateDetailCameraStuff(bool value)
    {
        Cursor.visible = !value;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        detailCameraPOV.SetActive(value);
    }
}
