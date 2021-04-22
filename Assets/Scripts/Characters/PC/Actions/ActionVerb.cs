using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action Verb", fileName = "New verb")]
public class ActionVerb : ScriptableObject
{
    public new string name;
    public Texture2D icon;
    public Texture2D cursor;
}
