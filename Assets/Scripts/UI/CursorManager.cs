using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorState
{
    Normal, Highlighted, Disable
}

public class CursorManager : MonoBehaviour
{
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

    public static CursorManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        detailCameraPOV.SetActive(false);
        cursorState = CursorState.Normal;
        SetCursors(defaultCursor, defaultCursor_Disable, defaultCursor_HL);
    }

    public void SetCursors(Texture2D normal, Texture2D disable, Texture2D hightlighted)
    {
        currentCursor = normal;
        currentCursor_Disable = disable;
        currentCursor_HL = hightlighted;

        SetCursor();
    }

    void SetCursor()
    {
        switch (cursorState)
        {
            case CursorState.Normal:
                Cursor.SetCursor(currentCursor, hotSpot, CursorMode.Auto);
                break;
            case CursorState.Highlighted:
                Cursor.SetCursor(currentCursor_HL, hotSpot, CursorMode.Auto);
                break;
            case CursorState.Disable:
                Cursor.SetCursor(currentCursor_Disable, hotSpot, CursorMode.Auto);
                break;
        }
    }

    public void ChangeCursorState(CursorState cursorState)
    {
        CursorState previousState = this.cursorState;
        this.cursorState = cursorState;

        if(previousState != this.cursorState)
            SetCursor();
    }

    public void ActivateDetailCameraStuff(bool value)
    {
        Cursor.visible = !value;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        detailCameraPOV.SetActive(value);
    }
}
