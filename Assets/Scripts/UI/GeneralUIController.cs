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
        actionVerbsUIController.SetActionBarVisibility(ActionBarVisibility.Unshown);
        dialogueUIController.ShowUnshow(true);
        inventoryUIController.ShowUnshow(false);
    }

    public void DisplayGameplayUI()
    {
        actionVerbsUIController.SetActionBarVisibility(ActionBarVisibility.HalfShown);
        dialogueUIController.ShowUnshow(false);
        inventoryUIController.ShowUnshow(false);
    }

    public void DisplayInventoryUI()
    {
        dialogueUIController.ShowUnshow(false);
        actionVerbsUIController.SetActionBarVisibility(ActionBarVisibility.FullShown);
        inventoryUIController.ShowUnshow(true);
    }
}
