using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SetData
{
    public Dictionary<int, InteractableObjData> objDatas;
    public Dictionary<int, PickableObjData> pickableObjDatas;
    public Dictionary<int, ContainerObjData> containerObjDatas;
    public Dictionary<int, DoorData> doorDatas;
    public Dictionary<int, NPCData> npcDatas;
    public Dictionary<int, EmitterObjData> emitterObjDatas;

    public SetData()
    {
        objDatas = new Dictionary<int, InteractableObjData>();
        pickableObjDatas = new Dictionary<int, PickableObjData>();
        containerObjDatas = new Dictionary<int, ContainerObjData>();
        doorDatas = new Dictionary<int, DoorData>();
        npcDatas = new Dictionary<int, NPCData>();
        emitterObjDatas = new Dictionary<int, EmitterObjData>();
    }
}

[Serializable]
public class InventoryData
{
    public Dictionary<int, PickableObjData> pickableObjInInventoryDatas;

    public InventoryData()
    {
        pickableObjInInventoryDatas = new Dictionary<int, PickableObjData>();
    }
}

[Serializable]
public class InteractableObjData
{
    public int id;
    public bool inScene;

    public InteractableObjData()
    {

    }

    public InteractableObjData(bool inScene)
    {
        this.inScene = inScene;
    }
}

[Serializable]
public class DoorData : InteractableObjData
{
    public bool opened;
    public bool locked;

    public DoorData()
    {

    }

    public DoorData(bool inScene, bool opened, bool locked) : base(inScene)
    {
        this.opened = opened;
        this.locked = locked;
    }
}

[Serializable]
public class EmitterObjData : InteractableObjData
{
    public List<int> quantityPerObj;
    public List<int> objToDropIDs;

    public EmitterObjData()
    {

    }

    public EmitterObjData(bool inScene, List<int> quantityPerObj, List<InteractableObj> objsToDrop) : base(inScene)
    {
        this.quantityPerObj = new List<int>();
        objToDropIDs = new List<int>();

        foreach(int quantity in quantityPerObj)
        {
            this.quantityPerObj.Add(quantity);
        }

        foreach(InteractableObj obj in objsToDrop)
        {
            objToDropIDs.Add(obj.objID);
        }
    }
}

[Serializable]
public class PickableObjData : InteractableObjData
{
    public bool inInventory;

    public PickableObjData()
    {

    }

    public PickableObjData(bool inScene, bool inInventory) : base(inScene)
    {
        this.inInventory = inInventory;
    }
}

[Serializable]
public class ContainerObjData : InteractableObjData
{
    public bool accessible;

    public ContainerObjData()
    {

    }

    public ContainerObjData(bool inScene, bool accessible) : base(inScene)
    {
        this.accessible = accessible;
    }
}

[Serializable]
public class NPCData : InteractableObjData
{
    public NPCData()
    {

    }

    public NPCData(bool inScene) : base(inScene)
    {

    }
}
