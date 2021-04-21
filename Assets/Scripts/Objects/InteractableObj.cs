using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(menuName = "Interactable Objects/Base Interactable Obj", fileName = "New object")]
public class InteractableObj : ScriptableObject
{

    public int objID;

    public new string name;

    public InteractableObjBehavior behavior;
}
