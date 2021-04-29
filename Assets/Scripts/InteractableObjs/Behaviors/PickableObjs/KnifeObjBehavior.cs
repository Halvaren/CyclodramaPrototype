using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeObjBehavior : PickableObjBehavior
{
    /*
     * Aquí deberían ir los diferentes campos necesarios para ejecutar el UseMethod
     */

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

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

        yield return null;
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            Debug.Log("Se da el cuchillo");
        }
        else if (index == 1)
        {
            Debug.Log("No funciona");
        }

        yield return null;
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, hitObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            Debug.Log("Se golpea con el cuchillo");
        }
        else if (index == 1)
        {
            Debug.Log("No funciona");
        }

        yield return null;
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, drawObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            Debug.Log("Se dibuja con el cuchillo");
        }
        else if (index == 1)
        {
            Debug.Log("No funciona");
        }

        yield return null;
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, throwObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            Debug.Log("Se lanza el cuchillo");
        }
        else if (index == 1)
        {
            Debug.Log("No funciona");
        }

        yield return null;
    }
}
