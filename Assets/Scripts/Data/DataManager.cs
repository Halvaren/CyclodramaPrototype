using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using System;
using RotaryHeart.Lib.SerializableDictionary;
using VIDE_Data;
using System.Xml.Serialization;
using System.Xml.XPath;

public enum SceneEnum
{
    LoadingScene, MainScene
}

public class DataManager : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<int, SetData> setDatas;
    [HideInInspector]
    public InventoryData inventoryData;
    public Dictionary<int, NPCData> npcDatas;
    public PCData pcData;

    public string pathToSave = "/saves";

    public VerbDictionary verbsDictionary;
    public ObjDictionary pickableObjsDictionary;
    //Aqu� podr�an ir m�s diccionarios de objetos si hicieran falta

    public delegate void SaveDataEvent();
    public static event SaveDataEvent OnSaveData;

    private static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            if (Application.isEditor && instance == null) return FindObjectOfType<DataManager>();
            return instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (SceneManager.GetActiveScene().buildIndex == (int)SceneEnum.LoadingScene)
        {
            StartCoroutine(LoadGameData((int)SceneEnum.MainScene));
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0) && SceneManager.GetActiveScene().buildIndex == (int)SceneEnum.MainScene)
        {
            SaveGameData();
        }
    }

    public IEnumerator LoadGameData(int loadSceneIndex = -1)
    {
        setDatas = new Dictionary<int, SetData>();
        npcDatas = new Dictionary<int, NPCData>();
        yield return StartCoroutine(ReadDataFromPath(Application.persistentDataPath + pathToSave + "/default.xml"));
        yield return StartCoroutine(LoadDialogues());

        if (loadSceneIndex != -1)
        {
            SceneManager.LoadSceneAsync(loadSceneIndex);
        }
    }

    public void SaveGameData()
    {
        OnSaveData();

        DebugSetDatas();

        StartCoroutine(WriteDataToPath(Application.persistentDataPath + pathToSave + "/default.xml"));
    }

    public InventoryData GetInvenetoryData()
    {
        return inventoryData;
    }

    public void SetInventoryData(InventoryData inventoryData)
    {
        this.inventoryData = inventoryData;
    }

    public SetData GetSetData(int setID)
    {
        if (/*setDatas != null &&*/ /*Debug*/ setDatas.ContainsKey(setID))
        {
            SetData setData = setDatas[setID];
            return setData;
        }
        else 
        {
            return null;
        }
    }

    public void SetSetData(int setID, SetData setData)
    {
        //if(setDatas != null) //Debug
        if(setDatas.ContainsKey(setID))
        {
            setDatas[setID] = setData;
        }
        else
        {
            setDatas.Add(setID, setData);
        }
    }

    public void DebugSetDatas()
    {
        string result = "";
        foreach(int setID in setDatas.Keys)
        {
            result += "Set ID " + setID + "\n" + setDatas[setID].ToString() + "\n";
        }
        Debug.Log(result);
    }

    IEnumerator LoadDialogues()
    {
        Debug.Log("Loading dialogues");
        yield return StartCoroutine(VD.LoadDialoguesCoroutine());
    }

    #region Read methods

    IEnumerator ReadDataFromPath(string path)
    {
        Debug.Log("Reading game data");
        if(File.Exists(path))
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                XmlDocument data = new XmlDocument();
                 
                try
                {
                    data.Load(fs);
                }
                catch (XmlException e)
                {
                    Debug.LogError(e);
                }

                if(data.FirstChild != null)
                {
                    XmlElement gameDataElement = (XmlElement)data.SelectSingleNode("//GameData");

                    yield return StartCoroutine(ReadSetData(gameDataElement));
                    yield return StartCoroutine(ReadInventoryData(gameDataElement));
                    yield return StartCoroutine(ReadNPCsData(gameDataElement));
                    yield return StartCoroutine(ReadPCData(gameDataElement));
                }
            }
        }
        else
        {
            Debug.LogWarning("Not found file in " + path);
        }
    }

    IEnumerator ReadSetData(XmlElement gameDataElement)
    {
        setDatas.Clear();

        XmlElement setsElement = (XmlElement)gameDataElement.SelectSingleNode("Sets");

        IEnumerable<XmlElement> setElements = setsElement.SelectNodes("Set").OfType<XmlElement>();

        foreach(XmlElement setElement in setElements)
        {
            SetData setData = new SetData();

            setDatas.Add(GetIntegerAttribute(setElement, "id"), setData);

            XmlElement interactableObjsElement = (XmlElement)setElement.SelectSingleNode("InteractableObjs");

            if(interactableObjsElement != null)
            {
                IEnumerable<XmlElement> interactableObjElements = interactableObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement interactableObjElement in interactableObjElements)
                {
                    Type type = null;
                    if (interactableObjElement.Name == typeof(InteractableObjData).Name) type = typeof(InteractableObjData);

                    if(type != null)
                    {
                        InteractableObjData objData = (InteractableObjData)DeserializeObjectFromXML(interactableObjElement, type, false);
                        //InteractableObjData objData = ReadInteractableObjData(interactableObjElement);
                        setData.objDatas.Add(objData.id, objData);
                    }

                    yield return null;
                }
            }

            XmlElement pickableObjsElement = (XmlElement)setElement.SelectSingleNode("PickableObjs");

            if(pickableObjsElement != null)
            {
                IEnumerable<XmlElement> pickableObjElements = pickableObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach (XmlElement pickableObjElement in pickableObjElements)
                {
                    Type type = null;
                    if (pickableObjElement.Name == typeof(PickableObjData).Name) type = typeof(PickableObjData);

                    if (type != null)
                    {
                        PickableObjData pickableObjData = (PickableObjData)DeserializeObjectFromXML(pickableObjElement, type, false);
                        //PickableObjData pickableObjData = ReadPickableObjData(pickableObjElement);
                        setData.pickableObjDatas.Add(pickableObjData.id, pickableObjData);
                    }

                    yield return null;
                }
            }

            /*XmlElement npcsElement = (XmlElement)setElement.SelectSingleNode("NPCs");

            if(npcsElement != null)
            {
                IEnumerable<XmlElement> npcElements = npcsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement npcElement in npcElements)
                {
                    if (npcElement.Name == typeof(NPCData).Name)
                    {
                        NPCData npcData = ReadNPCData(npcElement);
                        setData.npcDatas.Add(npcData.id, npcData);
                    }

                    yield return null;
                }
            }*/

            XmlElement doorsElement = (XmlElement)setElement.SelectSingleNode("Doors");

            if(doorsElement != null)
            {
                IEnumerable<XmlElement> doorElements = doorsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement doorElement in doorElements)
                {
                    Type type = null;
                    if (doorElement.Name == typeof(DoorData).Name) type = typeof(DoorData);

                    if (type != null)
                    {
                        DoorData doorData = (DoorData)DeserializeObjectFromXML(doorElement, type, false);
                        //DoorData doorData = ReadDoorData(doorElement);
                        setData.doorDatas.Add(doorData.id, doorData);
                    }

                    yield return null;
                }
            }

            XmlElement containerObjsElement = (XmlElement)setElement.SelectSingleNode("ContainerObjs");

            if(containerObjsElement != null)
            {
                IEnumerable<XmlElement> containerObjElements = containerObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement containerObjElement in containerObjElements)
                {
                    Type type = null;
                    if (containerObjElement.Name == typeof(ContainerObjData).Name) type = typeof(ContainerObjData);

                    if (type != null)
                    {
                        ContainerObjData containerObjData = (ContainerObjData)DeserializeObjectFromXML(containerObjElement, type, false);
                        //ContainerObjData containerObjData = ReadContainerObjData(containerObjElement);
                        setData.containerObjDatas.Add(containerObjData.id, containerObjData);
                    }

                    yield return null;
                }
            }

            XmlElement emitterObjsElement = (XmlElement)setElement.SelectSingleNode("EmitterObjs");

            if(emitterObjsElement != null)
            {
                IEnumerable<XmlElement> emitterObjElements = emitterObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement emitterObjElement in emitterObjElements)
                {
                    Type type = null;
                    if (emitterObjElement.Name == typeof(EmitterObjData).Name) type = typeof(EmitterObjData);

                    if (type != null)
                    {
                        EmitterObjData emitterObjData = (EmitterObjData)DeserializeObjectFromXML(emitterObjElement, type, false);
                        //EmitterObjData emitterObjData = ReadEmitterObjData(emitterObjElement);
                        setData.emitterObjDatas.Add(emitterObjData.id, emitterObjData);
                    }

                    yield return null;
                }
            }

            XmlElement detailedObjsElement = (XmlElement)setElement.SelectSingleNode("DetailedObjs");

            if (detailedObjsElement != null)
            {
                IEnumerable<XmlElement> detailedObjElements = detailedObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach (XmlElement detailedObjElement in detailedObjElements)
                {
                    Type type = null;
                    if (detailedObjElement.Name == typeof(DetailedObjData).Name) type = typeof(DetailedObjData);

                    if (type != null)
                    {
                        DetailedObjData detailedObjData = (DetailedObjData)DeserializeObjectFromXML(detailedObjElement, type, false);
                        //DetailedObjData detailedObjData = ReadDetailedObjData(detailedObjElement);
                        setData.detailedObjDatas.Add(detailedObjData.id, detailedObjData);
                    }

                    yield return null;
                }
            }

            yield return null;
        }
    }

    IEnumerator ReadInventoryData(XmlElement gameDataElement)
    {
        inventoryData = new InventoryData();

        XmlElement inventoryElement = (XmlElement)gameDataElement.SelectSingleNode("Inventory");

        if(inventoryElement != null)
        {
            IEnumerable<XmlElement> pickableObjInInventoryElements = inventoryElement.SelectNodes("*").OfType<XmlElement>();

            foreach (XmlElement pickableObjInInventoryElement in pickableObjInInventoryElements)
            {
                Type type = null;
                if (pickableObjInInventoryElement.Name == typeof(PickableObjData).Name) type = typeof(PickableObjData);

                if (type != null)
                {
                    PickableObjData pickableObjData = (PickableObjData)DeserializeObjectFromXML(pickableObjInInventoryElement, type, false);
                    //PickableObjData pickableObjData = ReadPickableObjData(pickableObjInInventoryElement);
                    inventoryData.pickableObjInInventoryDatas.Add(pickableObjData.id, pickableObjData);
                }

                yield return null;
            }
        }
    }

    IEnumerator ReadNPCsData(XmlElement gameDataElement)
    {
        XmlElement npcsElement = (XmlElement)gameDataElement.SelectSingleNode("NPCs");

        if(npcsElement != null)
        {
            IEnumerable<XmlElement> npcElements = npcsElement.SelectNodes("*").OfType<XmlElement>();

            foreach (XmlElement npcElement in npcElements)
            {
                Type type = null;
                if (npcElement.Name == typeof(NPCData).Name) type = typeof(NPCData);
                else if (npcElement.Name == typeof(NotanData).Name) type = typeof(NotanData);

                if (type != null)
                {
                    NPCData npcData = (NPCData)DeserializeObjectFromXML(npcElement, type, false);
                    //NPCData npcData = ReadNPCData(npcElement);
                    npcDatas.Add(npcData.id, npcData);
                }

                yield return null;
            }
        }
    }

    IEnumerator ReadPCData(XmlElement gameDataElement)
    {
        XmlElement pcElement = (XmlElement)gameDataElement.SelectSingleNode("PC");

        if(pcElement != null)
        {
            pcData = new PCData((PCData)DeserializeObjectFromXML(pcElement, typeof(PCData)));

            yield return null;
        }
    }

    /*
    InteractableObjData ReadInteractableObjData(XmlElement interactableObjElement, InteractableObjData initialData = null)
    {
        InteractableObjData interactableObjData = initialData;
        if(interactableObjData == null) interactableObjData = new InteractableObjData();

        interactableObjData.id = GetIntegerAttribute(interactableObjElement, "id");
        interactableObjData.inScene = GetBooleanAttribute(interactableObjElement, "inScene");

        return interactableObjData;
    }

    PickableObjData ReadPickableObjData(XmlElement pickableObjElement, PickableObjData initialData = null)
    {
        PickableObjData pickableObjData = initialData;
        if (pickableObjData == null) pickableObjData = new PickableObjData();
        pickableObjData = (PickableObjData)ReadInteractableObjData(pickableObjElement, pickableObjData);

        pickableObjData.inventoryObj = GetBooleanAttribute(pickableObjElement, "inventoryObj");

        return pickableObjData;
    }

    NPCData ReadNPCData(XmlElement npcElement, NPCData initialData = null)
    {
        NPCData npcData = initialData;
        if (npcData == null) npcData = new NPCData();
        npcData =  (NPCData)ReadInteractableObjData(npcElement, npcData);

        return npcData;
    }

    DoorData ReadDoorData(XmlElement doorElement, DoorData initialData = null)
    {
        DoorData doorData = initialData;
        if (doorData == null) doorData = new DoorData();
        doorData = (DoorData)ReadInteractableObjData(doorElement, doorData);

        doorData.opened = GetBooleanAttribute(doorElement, "opened");
        doorData.locked = GetBooleanAttribute(doorElement, "locked");

        return doorData;
    }

    ContainerObjData ReadContainerObjData(XmlElement containerObjElement, ContainerObjData initialData = null)
    {
        ContainerObjData containerObjData = initialData;
        if (containerObjData == null) containerObjData = new ContainerObjData();
        containerObjData = (ContainerObjData)ReadInteractableObjData(containerObjElement, containerObjData);

        containerObjData.accessible = GetBooleanAttribute(containerObjElement, "accessible");

        return containerObjData;
    }

    EmitterObjData ReadEmitterObjData(XmlElement emitterObjElement, EmitterObjData initialData = null)
    {
        EmitterObjData emitterObjData = initialData;
        if (emitterObjData == null) emitterObjData = new EmitterObjData();
        emitterObjData = (EmitterObjData)ReadInteractableObjData(emitterObjElement, emitterObjData);

        emitterObjData.dropObjs = new List<DropObject>();

        IEnumerable<XmlElement> objToDropElements = emitterObjElement.SelectNodes("DropObj").OfType<XmlElement>();

        foreach(XmlElement objToDropElement in objToDropElements)
        {
            DropObject dropObj = new DropObject();

            dropObj.quantity = GetIntegerAttribute(objToDropElement, "quantity");
            dropObj.obj = pickableObjsDictionary[GetIntegerAttribute(objToDropElement, "id")];
            dropObj.banObjs = new List<InteractableObj>();

            IEnumerable<XmlElement> banObjElements = objToDropElement.SelectNodes("BanObj").OfType<XmlElement>();

            foreach(XmlElement banObjElement in banObjElements)
            {
                dropObj.banObjs.Add(pickableObjsDictionary[GetIntegerAttribute(banObjElement, "id")]);
            }

            emitterObjData.dropObjs.Add(dropObj);
        }

        return emitterObjData;
    }*/

    #endregion

    #region Write methods

    IEnumerator WriteDataToPath(string path)
    {
        XmlDocument saveDoc = new XmlDocument();
        XmlDeclaration decl = saveDoc.CreateXmlDeclaration("1.0", "UFT-8", null);
        saveDoc.InsertBefore(decl, saveDoc.DocumentElement);

        XmlElement gameDataElement = saveDoc.CreateElement("GameData");
        saveDoc.AppendChild(gameDataElement);

        yield return StartCoroutine(WriteSetData(saveDoc, gameDataElement));
        yield return StartCoroutine(WriteInventoryData(saveDoc, gameDataElement));
        yield return StartCoroutine(WriteNPCsData(saveDoc, gameDataElement));
        yield return StartCoroutine(WritePCData(saveDoc, gameDataElement));

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8))
        {
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 1;
            writer.IndentChar = '\t';
            saveDoc.Save(writer);
        }
    }

    IEnumerator WriteSetData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement setsElement = saveDoc.CreateElement("Sets");
        gameDataElement.AppendChild(setsElement);

        foreach (int setID in setDatas.Keys)
        {
            XmlElement setElement = saveDoc.CreateElement("Set");
            setElement.SetAttribute("id", setID.ToString());

            setsElement.AppendChild(setElement);

            SetData setData = setDatas[setID];
            if (setData.objDatas.Count > 0)
            {
                XmlElement interactableObjsElement = saveDoc.CreateElement("InteractableObjs");

                setElement.AppendChild(interactableObjsElement);

                foreach (int objID in setData.objDatas.Keys)
                {
                    InteractableObjData objData = setData.objDatas[objID];
                    objData.id = objID;

                    SerializeObjectToXML(interactableObjsElement, objData);

                    //interactableObjsElement.AppendChild(WriteInteractableObjElement(saveDoc, objID, objData));

                    yield return null;
                }
            }

            if (setData.pickableObjDatas.Count > 0)
            {
                XmlElement pickableObjsElement = saveDoc.CreateElement("PickableObjs");

                setElement.AppendChild(pickableObjsElement);

                foreach (int objID in setData.pickableObjDatas.Keys)
                {
                    PickableObjData pickableObjData = setData.pickableObjDatas[objID];
                    pickableObjData.id = objID;

                    SerializeObjectToXML(pickableObjsElement, pickableObjData);

                    //pickableObjsElement.AppendChild(WritePickableObjElement(saveDoc, objID, pickableObjData));

                    yield return null;
                }
            }

            /*if(setData.npcDatas.Count > 0)
            {
                XmlElement npcsElement = saveDoc.CreateElement("NPCs");

                setElement.AppendChild(npcsElement);

                foreach(int objID in setData.npcDatas.Keys)
                {
                    NPCData npcData = setData.npcDatas[objID];

                    npcsElement.AppendChild(WriteNPCElement(saveDoc, objID, npcData));

                    yield return null;
                }
            }*/

            if(setData.doorDatas.Count > 0)
            {
                XmlElement doorsElement = saveDoc.CreateElement("Doors");

                setElement.AppendChild(doorsElement);

                foreach(int objID in setData.doorDatas.Keys)
                {
                    DoorData doorData = setData.doorDatas[objID];
                    doorData.id = objID;

                    SerializeObjectToXML(doorsElement, doorData);

                    yield return null;
                }
            }

            if (setData.containerObjDatas.Count > 0)
            {
                XmlElement containerObjsElement = saveDoc.CreateElement("ContainerObjs");

                setElement.AppendChild(containerObjsElement);

                foreach (int objID in setData.containerObjDatas.Keys)
                {
                    ContainerObjData containerObjData = setData.containerObjDatas[objID];
                    containerObjData.id = objID;

                    SerializeObjectToXML(containerObjsElement, containerObjData);

                    //containerObjsElement.AppendChild(WriteContainerObjElement(saveDoc, objID, containerObjData));

                    yield return null;
                }
            }

            if (setData.emitterObjDatas.Count > 0)
            {
                XmlElement emitterObjsElement = saveDoc.CreateElement("EmitterObjs");

                setElement.AppendChild(emitterObjsElement);

                foreach (int objID in setData.emitterObjDatas.Keys)
                {
                    EmitterObjData emitterObjData = setData.emitterObjDatas[objID];
                    emitterObjData.id = objID;

                    SerializeObjectToXML(emitterObjsElement, emitterObjData);

                    //emitterObjsElement.AppendChild(WriteEmitterObjElement(saveDoc, objID, emitterObjData));

                    yield return null;
                }
            }

            if(setData.detailedObjDatas.Count > 0)
            {
                XmlElement detailedObjsElement = saveDoc.CreateElement("DetailedObjs");

                setElement.AppendChild(detailedObjsElement);

                foreach(int objID in setData.detailedObjDatas.Keys)
                {
                    DetailedObjData detailedObjData = setData.detailedObjDatas[objID];
                    detailedObjData.id = objID;

                    SerializeObjectToXML(detailedObjsElement, detailedObjData);

                    yield return null;
                }
            }

            yield return null;
        }
    }

    IEnumerator WriteInventoryData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement inventoryElement = saveDoc.CreateElement("Inventory");
        gameDataElement.AppendChild(inventoryElement);

        foreach(int objID in inventoryData.pickableObjInInventoryDatas.Keys)
        {
            PickableObjData pickableObjData = inventoryData.pickableObjInInventoryDatas[objID];
            pickableObjData.id = objID;

            SerializeObjectToXML(inventoryElement, pickableObjData);

            yield return null;
        }
    }

    IEnumerator WriteNPCsData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement npcsElement = saveDoc.CreateElement("NPCs");
        gameDataElement.AppendChild(npcsElement);

        foreach(int objID in npcDatas.Keys)
        {
            NPCData npcData = npcDatas[objID];
            npcData.id = objID;

            SerializeObjectToXML(npcsElement, npcData);

            yield return null;
        }
    }

    IEnumerator WritePCData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement pcElement = saveDoc.CreateElement("PC");
        gameDataElement.AppendChild(pcElement);

        PCData pcData = new PCData(this.pcData);

        SerializeObjectToXML(pcElement, pcData);

        yield return null;
    }

    /*XmlElement WriteInteractableObjElement(XmlDocument saveDoc, int objID, InteractableObjData objData)
    {
        XmlElement interactableObjElement = saveDoc.CreateElement(objData.GetType().Name);

        if (objData is InteractableObjData)
        {
            interactableObjElement.SetAttribute("id", objID.ToString());
            interactableObjElement.SetAttribute("inScene", objData.inScene.ToString());
        }

        return interactableObjElement;
    }

    XmlElement WritePickableObjElement(XmlDocument saveDoc, int objID, PickableObjData pickableObjData)
    {
        XmlElement pickableObjElement = WriteInteractableObjElement(saveDoc, objID, pickableObjData);

        if(pickableObjData is PickableObjData)
        {
            pickableObjElement.SetAttribute("inventoryObj", pickableObjData.inventoryObj.ToString());
        }

        return pickableObjElement;
    }

    XmlElement WriteNPCElement(XmlDocument saveDoc, int objID, NPCData npcData)
    {
        XmlElement npcElement = WriteInteractableObjElement(saveDoc, objID, npcData);

        if (npcData is NPCData)
        {
            
        }

        return npcElement;
    }

    XmlElement WriteContainerObjElement(XmlDocument saveDoc, int objID, ContainerObjData containerObjData)
    {
        XmlElement containerObjElement = WriteInteractableObjElement(saveDoc, objID, containerObjData);

        if(containerObjData is ContainerObjData)
        {
            containerObjElement.SetAttribute("accessible", containerObjData.accessible.ToString());
        }

        return containerObjElement;
    }

    XmlElement WriteEmitterObjElement(XmlDocument saveDoc, int objID, EmitterObjData emitterObjData)
    {
        XmlElement emitterObjElement = WriteInteractableObjElement(saveDoc, objID, emitterObjData);

        if(emitterObjData is EmitterObjData)
        {
            foreach(DropObject dropObj in emitterObjData.dropObjs)
            {
                XmlElement dropObjElement = saveDoc.CreateElement("DropObj");
                dropObjElement.SetAttribute("id", dropObj.obj.objID.ToString());
                dropObjElement.SetAttribute("quantity", dropObj.quantity.ToString());

                foreach(InteractableObj obj in dropObj.banObjs)
                {
                    XmlElement banObjElement = saveDoc.CreateElement("BanObj");
                    banObjElement.SetAttribute("id", obj.objID.ToString());

                    dropObjElement.AppendChild(banObjElement);
                }

                emitterObjElement.AppendChild(dropObjElement);
            }
        }

        return emitterObjElement;
    }*/

    #endregion

    #region Xml attributes methods

    private XmlElement SerializeObjectToXML(XmlElement parent, object obj)
    {
        try
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", ""); 
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            XPathNavigator navigator = parent.CreateNavigator();
            using (XmlWriter writer = navigator.AppendChild())
            {
                writer.WriteWhitespace("");
                serializer.Serialize(writer, obj, ns);
            }
            return (XmlElement)parent.LastChild;
        }
        catch(Exception)
        {
            Debug.Log("Could not serialize " + obj.ToString());
            return null;
        }
    }

    private object DeserializeObjectFromXML(XmlElement xmlElement, Type type, bool isParent = true)
    {
        if (isParent && !xmlElement.HasChildNodes)
            return null;
        XmlSerializer serializer = new XmlSerializer(type);
        XPathNavigator navigator = (isParent ? xmlElement.FirstChild : xmlElement).CreateNavigator();
        using (XmlReader reader = navigator.ReadSubtree())
            return serializer.Deserialize(reader);
    }

    private int GetIntegerAttribute(XmlElement element, string attribute, bool throwIfInvalid = true)
    {
        int result = 0;
        if (!int.TryParse(element.GetAttribute(attribute), out result) && throwIfInvalid)
            throw new XmlException("Invalid Int " + attribute + " for element " + element.Name + "!");
        return result;
    }

    private bool GetBooleanAttribute(XmlElement element, string attribute, bool throwIfInvalid = true)
    {
        bool result = false;
        if (!bool.TryParse(element.GetAttribute(attribute), out result) && throwIfInvalid)
            throw new XmlException("Invalid Bool " + attribute + " for element " + element.Name + "!");
        return result;
    }

    #endregion
}

[Serializable]
public class ObjDictionary : SerializableDictionaryBase<int, InteractableObj>
{
    
}

[Serializable]
public class VerbDictionary : SerializableDictionaryBase<string, ActionVerb>
{

}