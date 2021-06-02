using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PointingResult
{
    Floor, Object, DetailedObject, Nothing
}

public class InputManager : MonoBehaviour
{
    [HideInInspector]
    public bool holdingShift;
    [HideInInspector]
    public bool pressedSpace;
    [HideInInspector]
    public bool pressedEscape;
    [HideInInspector]
    public bool pressedReturn;

    [HideInInspector]
    public bool pressedUp;
    [HideInInspector]
    public bool pressedDown;
    [HideInInspector]
    public float horizontal;
    [HideInInspector]
    public float vertical;

    [HideInInspector]
    public bool pressedInventoryKey;
    [HideInInspector]
    public bool pressedChangeVerbsKey;

    [HideInInspector]
    public float deltaScroll;

    [HideInInspector]
    public bool clicked;
    [HideInInspector]
    public Vector3 clickedPoint;
    [HideInInspector]
    public GameObject pointedGO;
    [HideInInspector]
    public PointingResult pointingResult;

    public LayerMask outlimitsLayerMask;
    public LayerMask floorLayerMask;
    public LayerMask interactableObjMask;
    public LayerMask detailedObjMask;
    public LayerMask detailCameraProjectionMask;

    public float unitsBetweenRaysWhenOutlimitsClicked = 1;

    private CameraManager cameraManager;
    public CameraManager CameraManager
    {
        get
        {
            if (cameraManager == null) cameraManager = CameraManager.instance;
            return cameraManager;
        }
    }

    private Camera m_MainCamera;
    public Camera MainCamera
    {
        get
        {
            if (m_MainCamera == null)
                m_MainCamera = CameraManager.mainCamera;

            return m_MainCamera;
        }
    }

    private Camera m_DetailCamera;
    public Camera DetailCamera
    {
        get
        {
            if (m_DetailCamera == null)
                m_DetailCamera = CameraManager.detailCamera;

            return m_DetailCamera;
        }
    }

    public bool usingDetailCamera
    {
        get { return !CameraManager.usingMainCamera; }
    }

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
        }
    }

    public static InputManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void InitializeInput()
    {
        InventoryUIController.OnCursorEnter += PointedGO;
        InventoryUIController.OnClick += ClickedInventoryItem;
    }

    private void Update()
    {
        clicked = false;

        horizontal = -Input.GetAxisRaw("Horizontal");
        vertical = -Input.GetAxisRaw("Vertical");

        holdingShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        pressedSpace = Input.GetKeyDown(KeyCode.Space);
        pressedEscape = Input.GetKeyDown(KeyCode.Escape);
        pressedReturn = Input.GetKeyDown(KeyCode.Return);

        pressedUp = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        pressedDown = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);

        pressedInventoryKey = Input.GetKeyDown(KeyCode.I);
        pressedChangeVerbsKey = Input.GetKeyDown(KeyCode.C);

        deltaScroll = Input.mouseScrollDelta.y;

        if(usingDetailCamera)
        {
            clicked = ThrowPointerRaycastDetailCamera();
        }
        else
        {
            clicked = ThrowPointerRaycastMainCamera();
        }
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

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, detailCameraProjectionMask))
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
        if (Physics.Raycast(ray, out hitInfo, float.MaxValue, usingDetailCamera ? detailedObjMask : interactableObjMask))
        {
            PointedGO(hitInfo.collider.gameObject, PointingResult.Object);
            if (click)
            {
                clickedPoint = hitInfo.point;
            }
        }
        else if (Physics.Raycast(ray, out hitInfo, float.MaxValue, floorLayerMask))
        {
            PointedGO(hitInfo.collider.gameObject, PointingResult.Floor);
            if (click)
            {
                clickedPoint = hitInfo.point;
            }
        }
        else if (Physics.Raycast(ray, out hitInfo, float.MaxValue, outlimitsLayerMask))
        {
            PointedGO(hitInfo.collider.gameObject, PointingResult.Floor);
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
        else
        {
            pointedGO = null;
        }
    }

    void PointedGO(GameObject go, PointingResult pointingResult)
    {
        pointedGO = go;
        this.pointingResult = pointingResult;
    }

    void ClickedInventoryItem()
    {
        clicked = true;
    }
}
