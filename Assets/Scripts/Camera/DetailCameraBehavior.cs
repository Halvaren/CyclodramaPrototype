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

    private CinemachinePOV cinemachinePOV;
    public CinemachinePOV CinemachinePOV
    {
        get
        {
            if (cinemachinePOV == null) cinemachinePOV = VirtualCamera.GetCinemachineComponent<CinemachinePOV>();
            return cinemachinePOV;
        }
    }

    public float axisSpeed = 300f;

    private void Start()
    {
        VirtualCamera.enabled = false;
    }

    public void ActivateCamera()
    {
        VirtualCamera.enabled = true;
    }

    public void DeactivateCamera()
    {
        RestartCamera();
        VirtualCamera.enabled = false;
    }

    public void LockUnlockCamera(bool unlock)
    {
        if(unlock)
        {
            CinemachinePOV.m_HorizontalAxis.m_MaxSpeed = axisSpeed;
            CinemachinePOV.m_VerticalAxis.m_MaxSpeed = axisSpeed;
        }
        else
        {
            CinemachinePOV.m_HorizontalAxis.m_MaxSpeed = 0f;
            CinemachinePOV.m_VerticalAxis.m_MaxSpeed = 0f;
        }
    }

    void RestartCamera()
    {
        if(CinemachinePOV != null)
        {
            AxisState horizontalAxis = CinemachinePOV.m_HorizontalAxis;
            AxisState verticalAxis = CinemachinePOV.m_VerticalAxis;

            horizontalAxis.Value = ((horizontalAxis.m_MaxValue - horizontalAxis.m_MinValue) / 2) + horizontalAxis.m_MinValue;
            verticalAxis.Value = ((verticalAxis.m_MaxValue - verticalAxis.m_MinValue) / 2) + verticalAxis.m_MinValue;

            CinemachinePOV.m_HorizontalAxis = horizontalAxis;
            CinemachinePOV.m_VerticalAxis = verticalAxis;
        }
    }
}
