using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DetailCameraBehavior : MonoBehaviour
{
    private CinemachineVirtualCamera m_VirtualCamera;
    public CinemachineVirtualCamera VirtualCamera
    {
        get
        {
            if (m_VirtualCamera == null) m_VirtualCamera = GetComponent<CinemachineVirtualCamera>();
            return m_VirtualCamera;
        }
    }

    private void Start()
    {
        VirtualCamera.enabled = false;
    }

    public void ActivateCamera()
    {
        VirtualCamera.enabled = true;
        CameraManager.instance.ChangeToProjectorCamera();
        PCController.Instance.MakeInvisible(true);
        CursorManager.instance.ActivateDetailCamera(true);
    }

    public void DeactivateCamera()
    {
        PCController.Instance.MakeInvisible(false);
        CursorManager.instance.ActivateDetailCamera(false);
        CameraManager.instance.ChangeToMainCamera();
        RestartCamera();
        VirtualCamera.enabled = false;
    }

    void RestartCamera()
    {
        AxisState horizontalAxis = VirtualCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis;
        AxisState verticalAxis = VirtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis;

        horizontalAxis.Value = ((horizontalAxis.m_MaxValue - horizontalAxis.m_MinValue) / 2) + horizontalAxis.m_MinValue;
        verticalAxis.Value = ((verticalAxis.m_MaxValue - verticalAxis.m_MinValue) / 2) + verticalAxis.m_MinValue;

        VirtualCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis = horizontalAxis;
        VirtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis = verticalAxis;
    }
}
