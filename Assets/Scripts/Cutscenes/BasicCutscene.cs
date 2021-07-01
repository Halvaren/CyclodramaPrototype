using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for cutscenes
/// </summary>
public abstract class BasicCutscene : MonoBehaviour
{
    /// <summary>
    /// Where cutscene runs
    /// </summary>
    public SetLocation location;

    /// <summary>
    /// Runs the cutscene
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator RunCutscene();

    /// <summary>
    /// Returns if cutscene can start
    /// </summary>
    /// <returns></returns>
    public abstract bool CheckStartConditions();
}
