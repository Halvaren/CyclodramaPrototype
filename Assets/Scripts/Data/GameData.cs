using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameData
{
    public List<SetData> setDatas;
    public InventoryData inventoryData;
    public List<NPCData> npcDatas;
    public PCData pcData;
}

[Serializable]
public class SetData
{
    public int id;

    public Dictionary<int, InteractableObjData> objDatas;
    public Dictionary<int, PickableObjData> pickableObjDatas;
    public Dictionary<int, ContainerObjData> containerObjDatas;
    public Dictionary<int, DoorData> doorDatas;
    public Dictionary<int, EmitterObjData> emitterObjDatas;
    public Dictionary<int, DetailedObjData> detailedObjDatas;

    public SetData(int id)
    {
        this.id = id;

        objDatas = new Dictionary<int, InteractableObjData>();
        pickableObjDatas = new Dictionary<int, PickableObjData>();
        containerObjDatas = new Dictionary<int, ContainerObjData>();
        doorDatas = new Dictionary<int, DoorData>();
        emitterObjDatas = new Dictionary<int, EmitterObjData>();
        detailedObjDatas = new Dictionary<int, DetailedObjData>();
    }

    public SetData(SetData other)
    {
        id = other.id;

        objDatas = new Dictionary<int, InteractableObjData>();
        pickableObjDatas = new Dictionary<int, PickableObjData>();
        containerObjDatas = new Dictionary<int, ContainerObjData>();
        doorDatas = new Dictionary<int, DoorData>();
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
        id = other.id;
        inScene = other.inScene;
    }
}

#region InteractableObjData especializations

public class SeatableObjData : InteractableObjData
{
    public bool occupied;

    public SeatableObjData()
    {

    }

    public SeatableObjData(bool inScene, bool occupied) : base(inScene)
    {
        this.occupied = occupied;
    }

    public SeatableObjData(SeatableObjData other) : base(other)
    {
        occupied = other.occupied;
    }
}

#endregion

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

#region DoorObjData especializations

#endregion

[Serializable]
public class EmitterObjData : InteractableObjData
{
    public List<DropObjData> dropObjs;

    public EmitterObjData()
    {

    }

    public EmitterObjData(bool inScene, List<DropObject> dropObjs) : base(inScene)
    {
        this.dropObjs = new List<DropObjData>();

        foreach(DropObject dropObj in dropObjs)
        {
            this.dropObjs.Add(new DropObjData(dropObj));
        }
    }

    public EmitterObjData(EmitterObjData other) : base (other)
    {
        dropObjs = new List<DropObjData>();

        foreach(DropObjData dropObj in other.dropObjs)
        {
            dropObjs.Add(new DropObjData(dropObj));
        }
    }
}

#region EmitterObjData especializations

public class OpenableEmmitterObjData : EmitterObjData
{
    public bool locked;

    public OpenableEmmitterObjData()
    {

    }

    public OpenableEmmitterObjData(bool inScene, List<DropObject> dropObjs, bool locked) : base(inScene, dropObjs)
    {
        this.locked = locked;
    }

    public OpenableEmmitterObjData(OpenableEmmitterObjData other) : base (other)
    {
        locked = other.locked;
    }
}

#endregion

[Serializable]
public class DropObjData
{
    public int quantity;
    public int objID;
    public List<int> banObjsIDs;

    public DropObjData()
    {
        banObjsIDs = new List<int>();
    }

    public DropObjData(DropObject dropObject)
    {
        quantity = dropObject.quantity;
        objID = dropObject.obj.objID;

        banObjsIDs = new List<int>();
        foreach(InteractableObj obj in dropObject.banObjs)
        {
            banObjsIDs.Add(obj.objID);
        }
    }

    public DropObjData(DropObjData other)
    {
        quantity = other.quantity;
        objID = other.objID;

        banObjsIDs = new List<int>();
        foreach(int objID in other.banObjsIDs)
        {
            banObjsIDs.Add(objID);
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

#region PickableObjData especializations

public class TeddyBearObjData : PickableObjData
{
    public bool fallen;

    public TeddyBearObjData()
    {

    }

    public TeddyBearObjData(bool inScene, bool inventoryObj, bool fallen) : base(inScene, inventoryObj)
    {
        this.fallen = fallen;
    }

    public TeddyBearObjData(TeddyBearObjData other) : base(other)
    {
        fallen = other.fallen;
    }
}

public class RopeObjData : PickableObjData
{
    public bool cut;

    public RopeObjData()
    {

    }

    public RopeObjData(bool inScene, bool inventoryObj, bool cut) : base(inScene, inventoryObj)
    {
        this.cut = cut;
    }

    public RopeObjData(RopeObjData other) : base(other)
    {
        cut = other.cut;
    }
}

public class FabricObjData : PickableObjData
{
    public FabricColor color;
    public bool inspected;

    public FabricObjData()
    {

    }

    public FabricObjData(bool inScene, bool inventoryObj, FabricColor color, bool inspected) : base(inScene, inventoryObj)
    {
        this.color = color;
        this.inspected = inspected;
    }

    public FabricObjData(FabricObjData other) : base(other)
    {
        color = other.color;
        inspected = other.inspected;
    }
}

#endregion

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

#region ContainerObjData especializations

#endregion

[Serializable]
public class NPCData : InteractableObjData
{
    public bool firstTimeTalk;

    public NPCData()
    {

    }

    public NPCData(bool inScene, bool firstTimeTalk) : base(inScene)
    {
        this.firstTimeTalk = firstTimeTalk;
    }

    public NPCData(NPCData other) : base(other)
    {
        firstTimeTalk = other.firstTimeTalk;
    }
}

#region NPCData especializations

[Serializable]
public class NotanData : NPCData
{
    public bool goneToBeMeasured;
    public bool convinced;
    public bool incidentOccurred;

    public NotanData()
    {

    }

    public NotanData(bool inScene, bool firstTimeTalk, bool goneToBeMeasured, bool convinced, bool incidentOccurred) : base(inScene, firstTimeTalk)
    {
        this.goneToBeMeasured = goneToBeMeasured;
        this.convinced = convinced;
        this.incidentOccurred = incidentOccurred;
    }

    public NotanData(NotanData other) : base(other)
    {
        goneToBeMeasured = other.goneToBeMeasured;
        convinced = other.convinced;
        incidentOccurred = other.incidentOccurred;
    }
}

#endregion

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

#region DetailedObjData especializations

public class DetailedEmitterObjData : DetailedObjData
{
    public List<DropObjData> dropObjs;

    public DetailedEmitterObjData()
    {

    }

    public DetailedEmitterObjData(bool inScene, List<DropObject> dropObjs) : base(inScene)
    {
        this.dropObjs = new List<DropObjData>();

        foreach (DropObject dropObj in dropObjs)
        {
            this.dropObjs.Add(new DropObjData(dropObj));
        }
    }

    public DetailedEmitterObjData(DetailedEmitterObjData other) : base(other)
    {
        dropObjs = new List<DropObjData>();

        foreach (DropObjData dropObj in other.dropObjs)
        {
            dropObjs.Add(new DropObjData(dropObj));
        }
    }
}

public class ZodiacBoxObjData : DetailedEmitterObjData
{
    public List<DropObjData> droppedObjs;

    public ZodiacBoxObjData()
    {

    }

    public ZodiacBoxObjData(bool inScene, List<DropObject> dropObjs, List<DropObject> droppedObjs) : base(inScene, dropObjs)
    {
        this.droppedObjs = new List<DropObjData>();

        foreach(DropObject droppedObj in droppedObjs)
        {
            this.droppedObjs.Add(new DropObjData(droppedObj));
        }
    }

    public ZodiacBoxObjData(ZodiacBoxObjData other) : base (other)
    {
        droppedObjs = new List<DropObjData>();

        foreach(DropObjData droppedObj in other.droppedObjs)
        {
            droppedObjs.Add(new DropObjData(droppedObj));
        }
    }
}

#endregion

[Serializable]
public class PCData
{
    public bool newScene = true;
    public CharacterLocation location;
    public float[] position;

    //Knowledge
    public bool needBelindaInspiration = false;
    public bool NotanDontWantToGetMeasured = false;

    //Quests
    public bool givenBelindaInspiration = false;
    public bool gotNotanMeasurements = false;
    public bool givenBelindaFabrics = false;

    public PCData()
    {

    }

    public PCData(PCData other)
    {
        newScene = other.newScene;
        location = other.location;
        position = other.position;

        needBelindaInspiration = other.needBelindaInspiration;
        NotanDontWantToGetMeasured = other.NotanDontWantToGetMeasured;

        givenBelindaInspiration = other.givenBelindaInspiration;
        gotNotanMeasurements = other.gotNotanMeasurements;
        givenBelindaFabrics = other.givenBelindaFabrics;
    }

    public bool CanDrawAnything()
    {
        return needBelindaInspiration /* || more things*/;
    }
}

[Serializable]
public class SaveStateData
{
    public CharacterLocation oliverLocation;
    public float playedTime;
    public int scene;
    public int act;

    public SaveStateData()
    {

    }

    public SaveStateData(SaveStateData other)
    {
        oliverLocation = other.oliverLocation;
        playedTime = other.playedTime;
        scene = other.scene;
        act = other.act;
    }
}
