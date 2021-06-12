using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicCutscene : MonoBehaviour
{
    public SetLocation location;

    public abstract IEnumerator RunCutscene();

    public abstract bool CheckStartConditions();
}
