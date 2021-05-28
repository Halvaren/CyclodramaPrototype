using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VikingHelmetObjBehavior : PickableObjBehavior
{
    public VIDE_Assign pickConversation;

    public override IEnumerator _GetPicked()
    {
        yield return StartCoroutine(_StartConversation(pickConversation));
    }
}
