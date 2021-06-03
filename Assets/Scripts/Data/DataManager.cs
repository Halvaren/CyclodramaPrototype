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

public class DataManager : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<int, SetData> setDatas;
    [HideInInspector]
    public InventoryData inventoryData;
    public Dictionary<int, NPCData> npcDatas;
    public PCData pcData;
    public SaveStateData loadedSaveStateData;

    public string pathToSave = "/saves";
    public string completePathToSave
    {
        get { return Application.persistentDataPath + pathToSave; }
    }

    public string defaultSaveCompletePath
    {
        get { return Application.streamingAssetsPath + "/" + defaultSaveFile; }
    }

    public string defaultSaveFile = "default.xml";
    public string autoSaveFileName = "autosave.xml";
    public List<string> saveFileNames;

    public SaveStateData defaultStateData;
    public SaveStateData autoSaveStateData;
    public List<SaveStateData> saveStateDatas;

    public VerbDictionary verbsDictionary;
    public ObjDictionary pickableObjsDictionary;
    public SetPrefabDictionary setPrefabDictionary;

    public delegate void SaveDataEvent();
    public static event SaveDataEvent OnSaveData;

    public bool countingPlayedTime = false;

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
        }
    }

    private DataUIController dataUIController;
    public DataUIController DataUIController
    {
        get
        {
            if (dataUIController == null) dataUIController = GeneralUIController.dataUIController;
            return dataUIController;
        }
    }

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

        countingPlayedTime = false;
        LoadFileNames();
        StartCoroutine(LoadSaveStateData());
    }

    private void Update()
    {
        if(countingPlayedTime && loadedSaveStateData != null)
        {
            loadedSaveStateData.playedTime += Time.deltaTime;
        }
    }

    void LoadFileNames()
    {
        saveFileNames = new List<string>();

        if(!Directory.Exists(completePathToSave))
        {
            Directory.CreateDirectory(completePathToSave);
        }

        string[] fileNames = Directory.GetFiles(completePathToSave, "save*.xml");

        foreach (string fileName in fileNames)
        {
            saveFileNames.Add(Path.GetFileName(fileName));
        }
    }

    public IEnumerator LoadAutoSaveGameData()
    {
        loadedSaveStateData = autoSaveStateData;
        yield return StartCoroutine(LoadGameData(completePathToSave + "/" + autoSaveFileName));
    }

    public IEnumerator LoadNewGameData()
    {
        loadedSaveStateData = new SaveStateData(defaultStateData);
        yield return StartCoroutine(LoadGameData(defaultSaveCompletePath));
        yield return StartCoroutine(SaveAutoSaveGameData());
    }

    public IEnumerator LoadGameData(int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= saveFileNames.Count) yield break;

        loadedSaveStateData = saveStateDatas[fileIndex];
        yield return StartCoroutine(LoadGameData(completePathToSave + "/" + saveFileNames[fileIndex]));
    }

    IEnumerator LoadGameData(string path)
    {
        setDatas = new Dictionary<int, SetData>();
        npcDatas = new Dictionary<int, NPCData>();
        yield return StartCoroutine(ReadDataFromPath(path));
        yield return StartCoroutine(LoadDialogues());
    }

    IEnumerator LoadSaveStateData()
    {
        yield return StartCoroutine(ReadSaveStateDatas());

        DataUIController.InitializeDataUI(autoSaveStateData, saveStateDatas);
        GeneralUIController.ShowMainMenuUI();
    }

    public IEnumerator SaveAutoSaveGameData()
    {
        yield return StartCoroutine(SaveGameData(completePathToSave + "/" + autoSaveFileName));

        DataUIController.UpdateSaveState(-1, loadedSaveStateData);
        autoSaveStateData = loadedSaveStateData;
    }

    public IEnumerator SaveGameData(int fileIndex)
    {
        bool newFile = false;
        if (fileIndex < 0) yield break;
        if (fileIndex >= saveFileNames.Count)
        {
            saveFileNames.Add("save" + (saveFileNames.Count) + ".xml");
            newFile = true;
        }

        yield return StartCoroutine(SaveGameData(completePathToSave + "/" + saveFileNames[fileIndex]));

        if(newFile)
        {
            SaveStateData newSaveState = new SaveStateData(loadedSaveStateData);
            saveStateDatas.Add(newSaveState);
            loadedSaveStateData = newSaveState;
        }

        DataUIController.UpdateSaveState(fileIndex, loadedSaveStateData);
    }

    IEnumerator SaveGameData(string path)
    {
        if(OnSaveData != null)
            OnSaveData();
        loadedSaveStateData.oliverLocation = pcData.location;

        yield return StartCoroutine(WriteDataToPath(path));
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
        if (setDatas.ContainsKey(setID))
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

    IEnumerator ReadSaveStateDatas()
    {
        Debug.Log("Reading save state datas");

        List<string> saveFilePaths = new List<string>();
        string autosaveFilePath = completePathToSave + "/" + autoSaveFileName;

        foreach(string fileName in saveFileNames)
        {
            saveFilePaths.Add(completePathToSave + "/" + fileName);
        }

        saveStateDatas = new List<SaveStateData>();

        foreach(string path in saveFilePaths)
        {
            SaveStateData saveStateData = ReadSaveStateData(path);

            saveStateDatas.Add(saveStateData);

            yield return null;
        }

        autoSaveStateData = ReadSaveStateData(autosaveFilePath);
        defaultStateData = ReadSaveStateData(defaultSaveCompletePath);
    }

    SaveStateData ReadSaveStateData(string path)
    {
        if (File.Exists(path))
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

                if (data.FirstChild != null)
                {
                    XmlElement gameDataElement = (XmlElement)data.SelectSingleNode("//GameData");

                    SaveStateData saveStateData = new SaveStateData();

                    saveStateData.playedTime = gameDataElement.GetAttribute("playedTime") != "" ? GetFloatAttribute(gameDataElement, "playedTime") : 0;

                    if (gameDataElement.GetAttribute("location") != "")
                    {
                        Enum.TryParse(gameDataElement.GetAttribute("location"), out saveStateData.oliverLocation);
                    }
                    else
                        saveStateData.oliverLocation = CharacterLocation.Corridor2;

                    saveStateData.scene = gameDataElement.GetAttribute("scene") != "" ? GetIntegerAttribute(gameDataElement, "scene") : 1;
                    saveStateData.act = gameDataElement.GetAttribute("act") != "" ? GetIntegerAttribute(gameDataElement, "act") : 2;

                    return saveStateData;
                }
            }
        }
        else
        {
            Debug.LogWarning("Not found file in " + path);
        }

        return null;
    }


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
                    else if (interactableObjElement.Name == typeof(SeatableObjData).Name) type = typeof(SeatableObjData);

                    if(type != null)
                    {
                        InteractableObjData objData = (InteractableObjData)DeserializeObjectFromXML(interactableObjElement, type, false);
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
                    else if (pickableObjElement.Name == typeof(TeddyBearObjData).Name) type = typeof(TeddyBearObjData);
                    else if (pickableObjElement.Name == typeof(RopeObjData).Name) type = typeof(RopeObjData);
                    else if (pickableObjElement.Name == typeof(FabricObjData).Name) type = typeof(FabricObjData);

                    if (type != null)
                    {
                        PickableObjData pickableObjData = (PickableObjData)DeserializeObjectFromXML(pickableObjElement, type, false);
                        setData.pickableObjDatas.Add(pickableObjData.id, pickableObjData);
                    }

                    yield return null;
                }
            }

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
                    else if (emitterObjElement.Name == typeof(OpenableEmmitterObjData).Name) type = typeof(OpenableEmmitterObjData);

                    if (type != null)
                    {
                        EmitterObjData emitterObjData = (EmitterObjData)DeserializeObjectFromXML(emitterObjElement, type, false);
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
                    else if (detailedObjElement.Name == typeof(DetailedEmitterObjData).Name) type = typeof(DetailedEmitterObjData);
                    else if (detailedObjElement.Name == typeof(ZodiacBoxObjData).Name) type = typeof(ZodiacBoxObjData);

                    if (type != null)
                    {
                        DetailedObjData detailedObjData = (DetailedObjData)DeserializeObjectFromXML(detailedObjElement, type, false);
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
                else if (pickableObjInInventoryElement.Name == typeof(TeddyBearObjData).Name) type = typeof(TeddyBearObjData);
                else if (pickableObjInInventoryElement.Name == typeof(RopeObjData).Name) type = typeof(RopeObjData);
                else if (pickableObjInInventoryElement.Name == typeof(FabricObjData).Name) type = typeof(FabricObjData);

                if (type != null)
                {
                    PickableObjData pickableObjData = (PickableObjData)DeserializeObjectFromXML(pickableObjInInventoryElement, type, false);
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

    #endregion

    #region Write methods

    IEnumerator WriteDataToPath(string path)
    {
        XmlDocument saveDoc = new XmlDocument();
        XmlDeclaration decl = saveDoc.CreateXmlDeclaration("1.0", "UFT-8", null);
        saveDoc.InsertBefore(decl, saveDoc.DocumentElement);

        XmlElement gameDataElement = saveDoc.CreateElement("GameData");
        saveDoc.AppendChild(gameDataElement);

        WriteSaveStateData(gameDataElement);
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

    void WriteSaveStateData(XmlElement gameDataElement)
    {
        gameDataElement.SetAttribute("playedTime", loadedSaveStateData.playedTime.ToString());
        gameDataElement.SetAttribute("location", loadedSaveStateData.oliverLocation.ToString());
        gameDataElement.SetAttribute("scene", loadedSaveStateData.scene.ToString());
        gameDataElement.SetAttribute("act", loadedSaveStateData.act.ToString());
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

                    yield return null;
                }
            }

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

    private float GetFloatAttribute(XmlElement element, string attribute, bool throwIfInvalid = true)
    {
        float result = 0;
        if (!float.TryParse(element.GetAttribute(attribute), out result) && throwIfInvalid)
            throw new XmlException("Invalid Float " + attribute + " for element " + element.Name + "!");
        return result;
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

[Serializable]
public class SetPrefabDictionary : SerializableDictionaryBase<int, GameObject>
{

}
