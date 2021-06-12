using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
            if(cantGoThrough)
            {
                StartCoroutine(CantGoThroughCoroutine(pcController));
            }
            else
                SetTransitionSystem.instance.ExecuteSetTransition(door);
        }
    }

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
