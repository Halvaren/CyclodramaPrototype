using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object of an action verb
/// </summary>
[CreateAssetMenu(menuName = "Action Verb", fileName = "New verb")]
public class ActionVerb : ScriptableObject
{
    //Name of the verb
    public new string name;

    //String prepared to be formatted with the text that must be displayed in ActionVerbBar when it is single-object verb
    public string singleObjActionInfo;
    //String prepared to be formatted with the text that must be displayed in ActionVerbBar when it is multi-object verb
    public string multiObjActionInfo;
}
