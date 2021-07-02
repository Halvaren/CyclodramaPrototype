using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Specialization of SetBehavior for the initial set (Corridor 2)
/// </summary>
public class InitialSetBehavior : SetBehavior
{
    public Transform newScenePosition;
    public EmployeeDoorBehavior employeeDoor;

    protected override void InitializeSet()
    {
        base.InitializeSet();

        employeeDoor.InitializeObjBehavior(gameObject);
    }
}
