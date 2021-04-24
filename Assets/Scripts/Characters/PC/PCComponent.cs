using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCComponent : ScriptableObject
{
    [NonSerialized]
    public PCController m_PCController;

    public Transform transform { get { return m_PCController.transform; } }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return m_PCController.StartCoroutine(routine);
    }

    public void StopCoroutine(Coroutine routine)
    {
        m_PCController.StopCoroutine(routine);
    }

    public T GetComponent<T>() where T : Component
    {
        return m_PCController.GetComponent<T>();
    }

    public T[] GetComponents<T>() where T : Component
    {
        return m_PCController.GetComponents<T>();
    }

    public T GetComponentInChildren<T>() where T : Component
    {
        return m_PCController.GetComponentInChildren<T>();
    }

    public T[] GetComponentsInChildren<T>() where T : Component
    {
        return m_PCController.GetComponentsInChildren<T>();
    }
}
