using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeObjBehavior : OpenableEmitterObjBehavior
{
    public Light fridgeLight;

    public override void InitializeObjBehavior(GameObject currentSet)
    {
        base.InitializeObjBehavior(currentSet);
        fridgeLight.enabled = false;
    }

    public void ActivateFridgeLight()
    {
        fridgeLight.enabled = true;
    }

    public void DeactivateFridgeLight()
    {
        fridgeLight.enabled = false;
    }
}
