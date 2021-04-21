using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetData
{
    public Dictionary<int, InteractableObjData> objDatas;
    public Dictionary<int, DoorData> doorDatas;

    public SetData()
    {
        objDatas = new Dictionary<int, InteractableObjData>();
        doorDatas = new Dictionary<int, DoorData>();
    }
}

public class InteractableObjData
{
    public ObjState state;

    public InteractableObjData(ObjState state)
    {
        this.state = state;
    }
}

public class DoorData
{
    public bool opened;
    public bool locked;

    public DoorData(bool opened, bool locked)
    {
        this.opened = opened;
        this.locked = locked;
    }
}
