using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
