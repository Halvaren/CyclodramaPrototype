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
    public Dictionary<int, DetailedObjData> detailedObjDatas;

    public SetData()
    {
        objDatas = new Dictionary<int, InteractableObjData>();
        pickableObjDatas = new Dictionary<int, PickableObjData>();
        containerObjDatas = new Dictionary<int, ContainerObjData>();
        doorDatas = new Dictionary<int, DoorData>();
        npcDatas = new Dictionary<int, NPCData>();
        emitterObjDatas = new Dictionary<int, EmitterObjData>();
        detailedObjDatas = new Dictionary<int, DetailedObjData>();
    }

    public SetData(SetData other)
    {
        objDatas = new Dictionary<int, InteractableObjData>();
        pickableObjDatas = new Dictionary<int, PickableObjData>();
        containerObjDatas = new Dictionary<int, ContainerObjData>();
        doorDatas = new Dictionary<int, DoorData>();
        npcDatas = new Dictionary<int, NPCData>();
        emitterObjDatas = new Dictionary<int, EmitterObjData>();
        detailedObjDatas = new Dictionary<int, DetailedObjData>();

        foreach (int objID in other.objDatas.Keys)
        {
            objDatas.Add(objID, new InteractableObjData(other.objDatas[objID]));
        }

        foreach (int objID in other.pickableObjDatas.Keys)
        {
            pickableObjDatas.Add(objID, new PickableObjData(other.pickableObjDatas[objID]));
        }

        foreach (int objID in other.containerObjDatas.Keys)
        {
            containerObjDatas.Add(objID, new ContainerObjData(other.containerObjDatas[objID]));
        }

        foreach (int objID in other.doorDatas.Keys)
        {
            doorDatas.Add(objID, new DoorData(other.doorDatas[objID]));
        }

        foreach (int objID in other.npcDatas.Keys)
        {
            npcDatas.Add(objID, new NPCData(other.npcDatas[objID]));
        }

        foreach (int objID in other.emitterObjDatas.Keys)
        {
            emitterObjDatas.Add(objID, new EmitterObjData(other.emitterObjDatas[objID]));
        }

        foreach(int objID in other.detailedObjDatas.Keys)
        {
            detailedObjDatas.Add(objID, new DetailedObjData(other.detailedObjDatas[objID]));
        }
    }

    public override string ToString()
    {
        string result = "";

        if (objDatas.Count > 0)
        {
            result += "Interactable objs: \n";
            foreach (int objID in objDatas.Keys)
            {
                result += "\tObject: ID: " + objID + "\n";
            }
            result += "\n";
        }

        if (pickableObjDatas.Count > 0)
        {
            result += "Pickable objs: \n";
            foreach (int objID in pickableObjDatas.Keys)
            {
                result += "\tObject: ID: " + objID + "\n";
            }
            result += "\n";
        }

        if (containerObjDatas.Count > 0)
        {
            result += "Container objs: \n";
            foreach (int objID in containerObjDatas.Keys)
            {
                result += "\tObject: ID: " + objID + "\n";
            }
            result += "\n";
        }

        if (doorDatas.Count > 0)
        {
            result += "Doors: \n";
            foreach (int objID in doorDatas.Keys)
            {
                result += "\tObject: ID: " + objID + "\n";
            }
            result += "\n";
        }

        if (npcDatas.Count > 0)
        {
            result += "NPCs: \n";
            foreach (int objID in npcDatas.Keys)
            {
                result += "\tObject: ID: " + objID + "\n";
            }
            result += "\n";
        }

        if (emitterObjDatas.Count > 0)
        {
            result += "Emitter objs: \n";
            foreach (int objID in emitterObjDatas.Keys)
            {
                result += "\tObject: ID: " + objID + "\n";
            }
        }

        return result;
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

    public InventoryData(InventoryData other)
    {
        pickableObjInInventoryDatas = new Dictionary<int, PickableObjData>();

        foreach (int objID in other.pickableObjInInventoryDatas.Keys)
        {
            pickableObjInInventoryDatas.Add(objID, new PickableObjData(other.pickableObjInInventoryDatas[objID]));
        }
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

    public InteractableObjData(InteractableObjData other)
    {
        inScene = other.inScene;
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

    public DoorData(DoorData other) : base(other)
    {
        opened = other.opened;
        locked = other.locked;
    }
}

[Serializable]
public class EmitterObjData : InteractableObjData
{
    public List<DropObject> dropObjs;

    public EmitterObjData()
    {

    }

    public EmitterObjData(bool inScene, List<DropObject> dropObjs) : base(inScene)
    {
        this.dropObjs = new List<DropObject>();

        foreach(DropObject dropObj in dropObjs)
        {
            this.dropObjs.Add(dropObj);
        }
    }

    public EmitterObjData(EmitterObjData other) : base (other)
    {
        dropObjs = new List<DropObject>();

        foreach(DropObject dropObj in other.dropObjs)
        {
            dropObjs.Add(new DropObject(dropObj));
        }
    }
}

[Serializable]
public class PickableObjData : InteractableObjData
{
    public bool inventoryObj;

    public PickableObjData()
    {

    }

    public PickableObjData(bool inScene, bool inventoryObj) : base(inScene)
    {
        this.inventoryObj = inventoryObj;
    }

    public PickableObjData(PickableObjData other) : base(other)
    {
        inventoryObj = other.inventoryObj;
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

    public ContainerObjData(ContainerObjData other) : base(other)
    {
        accessible = other.accessible;
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

    public NPCData(NPCData other) : base(other)
    {

    }
}

[Serializable]
public class DetailedObjData : InteractableObjData
{
    public DetailedObjData()
    {

    }

    public DetailedObjData(bool inScene) : base(inScene)
    {

    }

    public DetailedObjData(DetailedObjData other) : base(other)
    {

    }
}
