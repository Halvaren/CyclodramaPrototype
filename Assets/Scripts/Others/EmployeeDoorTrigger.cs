using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeDoorTrigger : MonoBehaviour
{
    public EmployeeDoorBehavior door;

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PCController>() != null)
        {
            GameManager.instance.EndGame(door);
        }
    }
}
