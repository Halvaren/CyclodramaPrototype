﻿using System;
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

    public Coroutine StartCoroutine(string routineName, object value = null)
    {
        return m_PCController.StartCoroutine(routineName, value);
    }

    public void StopCoroutine(Coroutine routine)
    {
        m_PCController.StopCoroutine(routine);
    }

    protected void AddVerbExecutionCoroutine(IEnumerator coroutine)
    {
        m_PCController.verbExecutionCoroutines.Push(coroutine);
    }

    protected void RemoveVerbExecutionCoroutine()
    {
        m_PCController.verbExecutionCoroutines.Pop();
    }

    public T GetComponent<T>() where T : Component
    {
        return m_PCController.GetComponent<T>();
    }

    public T[] GetComponents<T>() where T : Component
    {
        return m_PCController.GetComponents<T>();
    }

    public T GetComponentInChildren<T>(bool includeInactive = false) where T : Component
    {
        return m_PCController.GetComponentInChildren<T>(includeInactive);
    }

    public T[] GetComponentsInChildren<T>(bool includeInactive = false) where T : Component
    {
        return m_PCController.GetComponentsInChildren<T>(includeInactive);
    }
}
