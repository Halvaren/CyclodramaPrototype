using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralUIController : MonoBehaviour
{
    public static GeneralUIController Instance;

    public DialogueUIController dialogueUIController;
    public ActionVerbsUIController actionVerbsUIController;
    public InventoryUIController inventoryUIController;
    public DetailedUIController detailedUIController;

    void Awake()
    {
        Instance = this;
    }

    public void DisplayGameplayUI()
    {
        actionVerbsUIController.SetActionBarVisibility(ActionBarVisibility.HalfShown);
        inventoryUIController.ShowUnshow(false);
        detailedUIController.ShowUnshow(false);
    }

    public void DisplayInventoryUI()
    {
        actionVerbsUIController.SetActionBarVisibility(ActionBarVisibility.FullShown);
        inventoryUIController.ShowUnshow(true);
        detailedUIController.ShowUnshow(false);
    }

    public DetailedUIBase DisplayDetailedUI(DetailedObjBehavior behavior = null)
    {
        actionVerbsUIController.SetActionBarVisibility(ActionBarVisibility.Unshown);
        inventoryUIController.ShowUnshow(false);
        return detailedUIController.ShowUnshow(true, behavior);
    }
}
