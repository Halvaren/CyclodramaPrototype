using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PointingResult
{
    Floor, Door, Object, Subject, Nothing
}

[CreateAssetMenu(menuName = "PCComponents/Input Controller")]
public class PCInputController : PCComponent
{
    [HideInInspector]
    public bool running;

    [HideInInspector]
    public float horizontal;
    [HideInInspector]
    public float vertical;

    [HideInInspector]
    public bool EscapeKey;

    [HideInInspector]
    public bool openCloseInventory;

    [HideInInspector]
    public Vector3 clickedPoint;
    [HideInInspector]
    public GameObject pointedGO;
    [HideInInspector]
    public PointingResult pointingResult;

    public LayerMask outlimitsLayerMask;
    public LayerMask floorLayerMask;
    public LayerMask doorLayerMask;
    public LayerMask interactableObjMask;
    public LayerMask interactableSubjMask;

    public LayerMask detailCameraProjectionMask;

    public float unitsBetweenRaysWhenOutlimitsClicked = 1;

    private Camera m_MainCamera;
    public Camera MainCamera
    {
        get
        {
            if (m_MainCamera == null)
                m_MainCamera = CameraManager.instance.mainCamera;

            return m_MainCamera;
        }
    }

    private Camera m_DetailCamera;
    public Camera DetailCamera
    {
        get
        {
            if (m_DetailCamera == null)
                m_DetailCamera = CameraManager.instance.detailCamera;

            return m_DetailCamera;
        }
    }

    private bool zoomingIn
    {
        get { return !CameraManager.instance.usingMainCamera; }
    }

    // Update is called once per frame
    public bool InputUpdate()
    {
        horizontal = -Input.GetAxisRaw("Horizontal");
        vertical = -Input.GetAxisRaw("Vertical");

        running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        EscapeKey = Input.GetKeyDown(KeyCode.Escape);

        openCloseInventory = Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I);

        return !zoomingIn ? ThrowPointerRaycastMainCamera() : ThrowPointerRaycastDetailCamera();
    }

    bool ThrowPointerRaycastMainCamera()
    {
        bool click = Input.GetMouseButtonDown(0);

        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        pointingResult = PointingResult.Nothing;

        ThrowRaycast(ray, click);

        return click;
    }

    bool ThrowPointerRaycastDetailCamera()
    {
        bool click = Input.GetMouseButtonDown(0);

        Ray ray = new Ray(MainCamera.transform.position, MainCamera.transform.forward);
        RaycastHit hitInfo;
        pointingResult = PointingResult.Nothing;

        if(Physics.Raycast(ray, out hitInfo, Mathf.Infinity, detailCameraProjectionMask))
        {
            Vector2 localPoint = hitInfo.textureCoord;
            Ray projectionRay = DetailCamera.ViewportPointToRay(localPoint);

            ThrowRaycast(projectionRay, click);

            return click;
        }

        return false;
    }

    void ThrowRaycast(Ray ray, bool click)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, float.MaxValue, interactableSubjMask))
        {
            pointedGO = hitInfo.collider.gameObject;
            pointingResult = PointingResult.Subject;
            if (click)
            {
                clickedPoint = hitInfo.point;
            }
        }
        else if (Physics.Raycast(ray, out hitInfo, float.MaxValue, interactableObjMask))
        {
            pointedGO = hitInfo.collider.gameObject;
            pointingResult = PointingResult.Object;
            if (click)
            {
                clickedPoint = hitInfo.point;
            }
        }
        else if (Physics.Raycast(ray, out hitInfo, float.MaxValue, doorLayerMask))
        {
            pointedGO = hitInfo.collider.gameObject;
            pointingResult = PointingResult.Door;
            if (click)
            {
                clickedPoint = hitInfo.point;
            }
        }
        else if (Physics.Raycast(ray, out hitInfo, float.MaxValue, floorLayerMask))
        {
            pointedGO = hitInfo.collider.gameObject;
            pointingResult = PointingResult.Floor;
            if (click)
            {
                clickedPoint = hitInfo.point;
            }
        }
        else if (Physics.Raycast(ray, out hitInfo, float.MaxValue, outlimitsLayerMask))
        {
            pointedGO = hitInfo.collider.gameObject;
            pointingResult = PointingResult.Floor;
            if (click)
            {
                Vector3 direction = (transform.position - hitInfo.point).normalized;
                float distance = (transform.position - hitInfo.point).magnitude;

                RaycastHit hitInfo2;

                for (float i = 0; i < distance; i += unitsBetweenRaysWhenOutlimitsClicked)
                {
                    if (Physics.Raycast(hitInfo.point + direction * i + Vector3.down * 50, Vector3.up, out hitInfo2, int.MaxValue, floorLayerMask))
                    {
                        clickedPoint = hitInfo2.point;
                        break;
                    }
                }
            }
        }
    }
}
