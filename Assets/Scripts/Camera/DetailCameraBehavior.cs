using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Behavior of a detail camera, that is, a camera focused on a small section of a set
/// </summary>
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

    /// <summary>
    /// Activates the camera
    /// </summary>
    public void ActivateCamera()
    {
        RestartCamera();
        VirtualCamera.enabled = true;
    }

    /// <summary>
    /// Deactivates the camera
    /// </summary>
    public void DeactivateCamera()
    {
        VirtualCamera.enabled = false;
    }

    /// <summary>
    /// Locks or unlocks camera movement (only if it's a POV camera)
    /// </summary>
    /// <param name="unlock">True means that activates movement</param>
    /// <returns></returns>
    public bool LockUnlockCamera(bool unlock)
    {
        if(CinemachinePOV != null)
        {
            if (unlock)
            {
                CinemachinePOV.m_HorizontalAxis.m_MaxSpeed = axisSpeed;
                CinemachinePOV.m_VerticalAxis.m_MaxSpeed = axisSpeed;
            }
            else
            {
                CinemachinePOV.m_HorizontalAxis.m_MaxSpeed = 0f;
                CinemachinePOV.m_VerticalAxis.m_MaxSpeed = 0f;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Restarts camera point of view, centering it
    /// </summary>
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
