using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Interactable Objects/Fabric Obj", fileName = "New object")]
public class FabricObj : InteractableObj
{
    public string realName;

    public Sprite redSprite;
    public Sprite pinkSprite;
    public Sprite purpleSprite;
    public Sprite navyBlueSprite;
    public Sprite lightBlueSprite;
    public Sprite greenSprite;
    public Sprite greenishYellowSprite;
    public Sprite yellowSprite;
    public Sprite orangeSprite;
    public Sprite whiteSprite;
    public Sprite blackSprite;
    public Sprite greySprite;

    public Sprite GetInventorySprite(FabricColor color)
    {
        switch(color)
        {
            case FabricColor.Red:
                return redSprite;
            case FabricColor.Pink:
                return pinkSprite;
            case FabricColor.Purple:
                return purpleSprite;
            case FabricColor.NavyBlue:
                return navyBlueSprite;
            case FabricColor.LightBlue:
                return lightBlueSprite;
            case FabricColor.Green:
                return greenSprite;
            case FabricColor.GreenishYellow:
                return greenishYellowSprite;
            case FabricColor.Yellow:
                return yellowSprite;
            case FabricColor.Orange:
                return orangeSprite;
            case FabricColor.White:
                return whiteSprite;
            case FabricColor.Black:
                return blackSprite;
            case FabricColor.Grey:
                return greySprite;
        }

        return inventorySprite;
    }

    public string GetName(bool realName = false)
    {
        if (realName)
            return this.realName;
        return GetName();
    }
}
