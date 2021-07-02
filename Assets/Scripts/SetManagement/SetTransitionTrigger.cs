using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// It has a trigger collider and when PC enters in it, a set transition begins
/// </summary>
public class SetTransitionTrigger : MonoBehaviour
{
    public SetDoorBehavior door;
    [HideInInspector]
    public bool cantGoThrough;
    public VIDE_Assign cantGoThroughComment;

    private void OnTriggerEnter(Collider other)
    {
        PCController pcController = other.GetComponent<PCController>();
        if (pcController != null)
        {
            //If trigger is deactivated
            if(cantGoThrough)
            {
                //A different behavior starts
                StartCoroutine(CantGoThroughCoroutine(pcController));
            }
            //If not
            else
                //A set transition begins
                SetTransitionSystem.instance.ExecuteSetTransition(door);
        }
    }

    /// <summary>
    /// It is executed when trigger is inactive
    /// </summary>
    /// <param name="pcController"></param>
    /// <returns></returns>
    IEnumerator CantGoThroughCoroutine(PCController pcController)
    {
        pcController.EnableGameplayInput(false);
        pcController.EnableInventoryInput(false);

        yield return StartCoroutine(pcController.MovementController.MoveAndRotateToDirection(door.interactionPoint.position, Vector3.zero, true));

        yield return StartCoroutine(door._StartConversation(cantGoThroughComment));

        pcController.EnableGameplayInput(true);
        pcController.EnableInventoryInput(true);
    }
}
