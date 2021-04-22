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
        actionVerbsUIController.ShowUnshow(false);
        dialogueUIController.ShowUnshow(true);
        inventoryUIController.ShowUnshow(false);
    }

    public void DisplayGameplayUI()
    {
        actionVerbsUIController.ShowUnshow(true);
        dialogueUIController.ShowUnshow(false);
        inventoryUIController.ShowUnshow(false);
    }

    public void DisplayInventoryUI()
    {
        dialogueUIController.ShowUnshow(false);
        actionVerbsUIController.ShowUnshow(true);
        inventoryUIController.ShowUnshow(true);
    }
}
