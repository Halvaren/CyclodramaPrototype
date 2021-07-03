using UnityEngine;

/// <summary>
/// ScriptableObject that stores interactable object data and identifies it
/// </summary>
[CreateAssetMenu(menuName = "Interactable Objects/Base Interactable Obj", fileName = "New object")]
public class InteractableObj : ScriptableObject
{

    public int objID;

    public new string name;

    public InteractableObjBehavior behavior;

    public Sprite inventorySprite;

    public virtual string GetName()
    {
        return name;
    }

    public virtual Sprite GetInventorySprite()
    {
        return inventorySprite;
    }
}
