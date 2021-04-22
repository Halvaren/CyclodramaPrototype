using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeObjBehavior : PickableObjBehavior
{
    /*
     * Aquí deberían ir los diferentes campos necesarios para ejecutar el UseMethod
     */

    public override int UseMethod(InteractableObjBehavior targetObj)
    {
        int index = base.UseMethod(targetObj);

        if(index == -1)
        {
            Debug.Log("Error");
        }
        else if(index == 0)
        {
            Debug.Log("Se usa cuchillo y corta la cuerda");
        }
        else if(index == 1)
        {
            Debug.Log("No funciona");
        }

        return index;
    }
}
