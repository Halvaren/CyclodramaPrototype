using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralUIController : MonoBehaviour
{
    public static GeneralUIController Instance;

    public DialogueUIController dialogueUIController;
    public ActionVerbsUIController actionVerbsUIController;
    public InventoryUIController inventoryUIController;

    void Awake()
    {
        Instance = this;
    }

    public void DisplayDialogueUI()
    {
        actionVerbsUIController.gameObject.SetActive(false);
        dialogueUIController.gameObject.SetActive(true);
    }

    public void DisplayActionVerbsUI()
    {
        actionVerbsUIController.gameObject.SetActive(true);
        dialogueUIController.gameObject.SetActive(false);
    }
}
