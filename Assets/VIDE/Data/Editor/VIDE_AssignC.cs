using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using MiniJSON_VIDE;
using System.Linq;
using Supyrb;

[CanEditMultipleObjects]
[CustomEditor(typeof(VIDE_Assign))]
public class VIDE_AssignC : Editor
{
    /*
     * Custom Inspector for the VIDE_Assign component
     */

    VIDE_Assign d;
    bool loadup = false;

    static string path = "";
    List<string> fullPaths = new List<string>();

    bool searching = false;
    string diagSearch = "";
    List<string> results = new List<string>();

    SerializedProperty diags;

    SerializedProperty assignedIndex;
    SerializedProperty assignedID;
    SerializedProperty assignedDialogue;

    SerializedProperty interactionCount;
    SerializedProperty alias;

    SerializedProperty overrideStartNode;

    SerializedProperty defaultNPCSprite;
    SerializedProperty defaultPlayerSprite;

    SerializedProperty targetManager;

    SerializedProperty preload;
    SerializedProperty notuptodate;
    SerializedProperty startp;
    SerializedProperty loadtag;

    SerializedProperty playerDiags;
    SerializedProperty actionNodes;
    SerializedProperty langs;

    private void openVIDE_Editor(string idx)
    {
        if (d != null)
            loadFiles();

        VIDE_Editor editor = EditorWindow.GetWindow<VIDE_Editor>();
        editor.Init(idx, true);
    }

    void OnEnable()
    {
        diags = serializedObject.FindProperty("diags");

        assignedIndex = serializedObject.FindProperty("assignedIndex");
        assignedID = serializedObject.FindProperty("assignedID");
        assignedDialogue = serializedObject.FindProperty("assignedDialogue");

        interactionCount = serializedObject.FindProperty("interactionCount");
        alias = serializedObject.FindProperty("alias");

        overrideStartNode = serializedObject.FindProperty("overrideStartNode");

        defaultNPCSprite = serializedObject.FindProperty("defaultNPCSprite");
        defaultPlayerSprite = serializedObject.FindProperty("defaultPlayerSprite");

        targetManager = serializedObject.FindProperty("targetManager");

        preload = serializedObject.FindProperty("preload");
        notuptodate = serializedObject.FindProperty("notuptodate");
        startp = serializedObject.FindProperty("startp");
        loadtag = serializedObject.FindProperty("loadtag");

        playerDiags = serializedObject.FindProperty("playerDiags");
        actionNodes = serializedObject.FindProperty("actionNodes");
        langs = serializedObject.FindProperty("langs");

        loadup = true;

        path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
        path = Directory.GetParent(path).ToString();
        path = Directory.GetParent(path).ToString();
        path = Directory.GetParent(path).ToString();

        loadFiles();
    }

    bool HasUniqueID(int id, string[] saveNames, int currentDiag)
    {
        //Retrieve all IDs
        foreach (string s in saveNames)
        {
            if (s == saveNames[currentDiag]) continue;

            if (File.Exists(Application.dataPath + "/../" + s))
            {
                Dictionary<string, object> dict = SerializeHelper.ReadFromFile(s) as Dictionary<string, object>;
                if (dict.ContainsKey("dID"))
                    if (id == ((int)((long)dict["dID"])))
                        return false;
            }
        }
        return true;
    }

    int AssignDialogueID(string[] saveNames)
    {
        List<int> ids = new List<int>();
        int newID = UnityEngine.Random.Range(0, 99999);

        //Retrieve all IDs
        foreach (string s in saveNames)
        {
            if (File.Exists(Application.dataPath + "/../" + s))
            {
                Dictionary<string, object> dict = SerializeHelper.ReadFromFile(s) as Dictionary<string, object>;
                if (dict.ContainsKey("dID"))
                    ids.Add((int)((long)dict["dID"]));
            }
        }

        //Make sure ID is unique
        while (ids.Contains(newID))
        {
            newID = UnityEngine.Random.Range(0, 99999);
        }

        return newID;
    }

    public class SerializeHelper
    {
        static string fileDataPath = Application.dataPath + "/../";

        public static void WriteToFile(object data, string filename)
        {
            string outString = DiagJson.Serialize(data);
            File.WriteAllText(fileDataPath + filename, outString);
        }
        public static object ReadFromFile(string filename)
        {
            string jsonString = File.ReadAllText(fileDataPath + filename);
            return DiagJson.Deserialize(jsonString);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        d = (VIDE_Assign)target;
        Color defColor = GUI.color;
        GUI.color = Color.yellow;

        if (loadup)
        {
            loadFiles();
            loadup = false;
        }

        if (searching)
        {
            ShowSearch();
            return;
        }

        //Create a button to open up the VIDE Editor and load the currently assigned dialogue
        if (GUILayout.Button("Open VIDE Editor"))
        {
            openVIDE_Editor(assignedDialogue.stringValue);
        }

        GUI.color = defColor;

        //Refresh dialogue list
        if (Event.current.type == EventType.MouseDown)
        {
            if (d != null)
                loadFiles();
        }

        GUILayout.BeginHorizontal();

        GUILayout.Label(new GUIContent("Assigned dialogue", "Which dialogue is this NPC going to own?"));
        if (diags.arraySize > 0)
        {
            //Debug.Log("Array size " + diags.arraySize + " assignedIndex " + assignedIndex.intValue);
            EditorGUI.BeginChangeCheck();
            //Undo.RecordObject(d, "Changed dialogue index");

            string[] diagsArray = new string[diags.arraySize];
            for(int i = 0; i < diags.arraySize; i++)
            {
                diagsArray[i] = diags.GetArrayElementAtIndex(i).stringValue;
            }

            assignedIndex.intValue = EditorGUILayout.Popup(assignedIndex.intValue, diagsArray);

            if (EditorGUI.EndChangeCheck())
            {
                PreloadDialogue(false);
                int theID = 0;
                int currentName = -1;

                /* Get file location based on name */
                for (int i = 0; i < diags.arraySize; i++)
                {
                    if (fullPaths[i].Contains(diags.GetArrayElementAtIndex(assignedIndex.intValue).stringValue + ".json"))
                    {
                        currentName = i;
                    }
                }

                if (currentName == -1)
                {
                    return;
                }

                if (File.Exists(Application.dataPath + "/../" + fullPaths[currentName]))
                {
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                    {
                        theID = ((int)((long)dict["dID"]));

                    }
                    else Debug.LogError("Could not read dialogue ID!");
                }

                if (!HasUniqueID(theID, fullPaths.ToArray(), currentName))
                {
                    theID = AssignDialogueID(fullPaths.ToArray());
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                    {
                        dict["dID"] = theID;
                    }
                    SerializeHelper.WriteToFile(dict as Dictionary<string, object>, fullPaths[currentName]);
                }

                assignedID.intValue = theID;
                assignedDialogue.stringValue = diags.GetArrayElementAtIndex(assignedIndex.intValue).stringValue;

                foreach (var transform in Selection.transforms)
                {
                    VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                    scr.assignedIndex = assignedIndex.intValue;
                    scr.assignedDialogue = assignedDialogue.stringValue;
                    scr.assignedID = assignedID.intValue;
                }

            }
        }
        else
        {
            GUILayout.Label("No saved Dialogues!");
        }

        if (GUILayout.Button("Search", EditorStyles.miniButton))
        {
            searching = true;
            diagSearch = "";
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        //Undo.RecordObject(d, "Changed custom name");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(alias, new GUIContent("Alias", "Custom alias for this dialogue"));
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var transform in Selection.transforms)
            {
                VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                scr.alias = alias.stringValue;
            }
        }

        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        //Undo.RecordObject(d, "Changed override start node");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(overrideStartNode, new GUIContent("Override Start node", "Dialogue will instead begin on the node with this ID"));
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var transform in Selection.transforms)
            {
                VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                scr.overrideStartNode = overrideStartNode.intValue;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        defaultPlayerSprite.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Default Player Sprite", "Default player sprite for this component"), defaultPlayerSprite.objectReferenceValue, typeof(Sprite), false);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var transform in Selection.transforms)
            {
                VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                scr.defaultPlayerSprite = (Sprite)defaultPlayerSprite.objectReferenceValue;
            }
        }

        EditorGUI.BeginChangeCheck();
        defaultNPCSprite.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Default NPC Sprite", "Default NPC sprite for this component"), defaultNPCSprite.objectReferenceValue, typeof(Sprite), false);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var transform in Selection.transforms)
            {
                VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                scr.defaultNPCSprite = (Sprite)defaultNPCSprite.objectReferenceValue;
            }
        }
        GUILayout.Label(new GUIContent("Interaction Count: " + interactionCount.intValue.ToString(), "How many times have we interacted with this NPC?"));

        GUILayout.BeginVertical(GUI.skin.box);

        if (!preload.boolValue)
        {
            if (assignedDialogue.stringValue == "" || assignedIndex.intValue == -1) GUI.enabled = false;
            if (GUILayout.Button("Preload dialogue"))
            {
                PreloadDialogue(true);
            }
            GUI.enabled = true;
            EditorGUILayout.HelpBox("The dialogue will be preloaded for all VAs and won't require loading from json, eliminating loading times.\nMake sure you preload again if you make changes to the dialogue!", MessageType.Info);
        } else
        {
            GUI.color = Color.green;
            if (GUILayout.Button("Unload"))
            {
                PreloadDialogue(false);
            }
            GUI.color = Color.white;

            string helptext = "Dialogue preloaded.";
            if (playerDiags != null /*&& playerDiags.arraySize > 0*/) helptext += "\nDialogue Nodes: " + playerDiags.arraySize.ToString(); else helptext += "\nDialogue Nodes: 0";
            if (actionNodes != null  /*&& actionNodes.arraySize > 0*/) helptext += "\nAction Nodes: " + actionNodes.arraySize.ToString(); else helptext += "\nAction Nodes: 0";
            if (langs != null /*&&langs.arraySize > 0*/) helptext += "\nLanguages: " + langs.arraySize.ToString(); else helptext += "\nLanguages: 0";
            EditorGUILayout.HelpBox(helptext, MessageType.Info);
            if (notuptodate.boolValue)
            {
                EditorGUILayout.HelpBox("You've made changes to the dialogue. Make sure you preload again.", MessageType.Warning);
            }
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();

        if (!Application.isPlaying || assignedIndex.intValue == -1 || targetManager.objectReferenceValue == null) GUI.enabled = false;
        if (GUILayout.Button(new GUIContent("Test Interact", "Select dialogue, target gameobject, and enter play mode.")))
        {
            ((GameObject) targetManager.objectReferenceValue).SendMessage("Interact", d, SendMessageOptions.RequireReceiver);
        }
        GUI.enabled = true;

        EditorGUILayout.PropertyField(targetManager);

        GUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("You can select a gameobject containing a UI Manager and press 'Test Interact' during PlayMode to test this dialogue without requiring a dialogue trigger." +
            "\nUI Manager must contain an 'Interact' method like in Template_UIManager.cs", MessageType.Info);
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    Vector2 scrollpos = new Vector2();
    public void ShowSearch()
    {
        GUI.color = Color.white;
        GUILayout.BeginHorizontal(GUI.skin.box);
        if (GUILayout.Button("Back"))
        {
            searching = false;
        }
        diagSearch = EditorGUILayout.TextField(diagSearch);
        GUILayout.EndHorizontal();
        results.Clear();
        for (int i = 0; i < diags.arraySize; i++)
        {
            if (diags.GetArrayElementAtIndex(i).stringValue.ToLower().Contains(diagSearch.ToLower()))
            {
                results.Add(diags.GetArrayElementAtIndex(i).stringValue);
            }
        }
        scrollpos = GUILayout.BeginScrollView(scrollpos, GUI.skin.box, GUILayout.Height(200));
        for (int i = 0; i < results.Count; i++)
        {
            if (GUILayout.Button(results[i], EditorStyles.miniButton))
            {
                PreloadDialogue(false);
                int theID = 0;
                int currentName = -1;

                int indexOf = -1;

                for(int j = 0; j < diags.arraySize; j++)
                {
                    if(diags.GetArrayElementAtIndex(j).stringValue == results[i])
                    {
                        indexOf = j;
                        break;
                    }
                }

                assignedIndex.intValue = indexOf;

                if(indexOf != -1)
                {
                    /* Get file location based on name */
                    for (int i2 = 0; i2 < diags.arraySize; i2++)
                    {
                        if (fullPaths[i2].Contains(diags.GetArrayElementAtIndex(assignedIndex.intValue).stringValue + ".json"))
                            currentName = i2;
                    }
                }

                if (currentName == -1)
                {
                    return;
                }

                if (File.Exists(Application.dataPath + "/../" + fullPaths[currentName]))
                {
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                    {
                        theID = ((int)((long)dict["dID"]));

                    }
                    else Debug.LogError("Could not read dialogue ID!");
                }

                if (!HasUniqueID(theID, fullPaths.ToArray(), currentName))
                {
                    theID = AssignDialogueID(fullPaths.ToArray());
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                    {
                        dict["dID"] = theID;
                    }
                    SerializeHelper.WriteToFile(dict as Dictionary<string, object>, fullPaths[currentName]);
                }

                assignedID.intValue = theID;
                assignedDialogue.stringValue = diags.GetArrayElementAtIndex(assignedIndex.intValue).stringValue;


                foreach (var transform in Selection.transforms)
                {
                    VIDE_Assign scr = transform.GetComponent<VIDE_Assign>();
                    scr.assignedIndex = assignedIndex.intValue;
                    scr.assignedDialogue = assignedDialogue.stringValue;
                    scr.assignedID = assignedID.intValue;
                }
                searching = false;
            }
        }

        GUILayout.EndScrollView();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    //Refresh dialogue list
    public void OnFocus()
    {
        if (d != null)
            loadFiles();
    }

    //Refresh dialogue list
    public void loadFiles()
    {
        AssetDatabase.Refresh();
        d = (VIDE_Assign)target;

        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        List<string> diags = new List<string>();
        fullPaths = new List<string>();

        if (files.Length < 1) return;

        foreach (TextAsset f in files)
        {
            diags.Add(f.name);
            fullPaths.Add(AssetDatabase.GetAssetPath(f));
        }

        diags.Sort();

        this.diags.arraySize = 0;
        foreach(string diag in diags)
        {
            this.diags.arraySize++;
            this.diags.GetArrayElementAtIndex(this.diags.arraySize - 1).stringValue = diag;
        }

        //Lets make sure we still have the right file
        IDCheck();
        Repaint();

    }

    void IDCheck()
    {
        int theID = 0;
        List<int> theIDs = new List<int>();
        if (assignedIndex.intValue == -1)
        {
            if (assignedDialogue.stringValue != "")
            {
                bool contains = false;

                for(int i = 0; i < diags.arraySize; i++)
                {
                    if(diags.GetArrayElementAtIndex(i).stringValue == assignedDialogue.stringValue)
                    {
                        contains = true;
                        break;
                    }
                }

                if (contains)
                {
                    int indexOf = -1;

                    for (int j = 0; j < diags.arraySize; j++)
                    {
                        if (diags.GetArrayElementAtIndex(j).stringValue == assignedDialogue.stringValue)
                        {
                            indexOf = j;
                            break;
                        }
                    }

                    assignedIndex.intValue = indexOf;
                }
                else return;
            }
            else
            {
                return;
            }
        }

        if (assignedIndex.intValue >= diags.arraySize)
        {
            for (int i = 0; i < diags.arraySize; i++)
            {
                    if (diags.GetArrayElementAtIndex(i).stringValue == assignedDialogue.stringValue)
                {
                    assignedIndex.intValue = i;
                }
            }
        }

        int currentName = -1;

        /* Get file location based on name */
        for (int i = 0; i < diags.arraySize; i++)
        {
            if (fullPaths[i].Contains(diags.GetArrayElementAtIndex(assignedIndex.intValue).stringValue + ".json"))
                currentName = i;
        }

        if (currentName == -1)
        {
            return;
        }

        if (File.Exists(Application.dataPath + "/../" + fullPaths[currentName]))
        {
            Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
            if (dict.ContainsKey("dID"))
            {
                theID = ((int)((long)dict["dID"]));
            }
            else { Debug.LogError("Could not read dialogue ID!"); return; }
        }

        if (theID != assignedID.intValue)
        {

            for(int i = 0; i < diags.arraySize; i++)
            {
                string s = diags.GetArrayElementAtIndex(i).stringValue;

                for(int j = 0; j < diags.arraySize; j++)
                {
                    if (fullPaths[j].Contains(s + ".json"))
                        currentName = j;
                }

                if (File.Exists(Application.dataPath + "/../" + fullPaths[currentName]))
                {
                    Dictionary<string, object> dict = SerializeHelper.ReadFromFile(fullPaths[currentName]) as Dictionary<string, object>;
                    if (dict.ContainsKey("dID"))
                        theIDs.Add((int)((long)dict["dID"]));
                }
            }
            var theRealID_Index = theIDs.IndexOf(assignedID.intValue);

            assignedIndex.intValue = theRealID_Index;

            if (assignedIndex.intValue != -1)
                assignedDialogue.stringValue = diags.GetArrayElementAtIndex(assignedIndex.intValue).stringValue;
        }
    }

    
    void PreloadDialogue(bool preload)
    {
        if (preload)
        {
            IDCheck();

            VIDE_Data.Diags diag = VIDE_Data.VD.PreloadLoad(assignedDialogue.stringValue);

            playerDiags.arraySize = 0;
            foreach(VIDE_Data.DialogueNode node in diag.playerNodes)
            {
                playerDiags.arraySize++;
                SerializedPropertyExtensions.SetValue(playerDiags.GetArrayElementAtIndex(playerDiags.arraySize - 1), node);
            }

            actionNodes.arraySize = 0;
            foreach(VIDE_Data.ActionNode node in diag.actionNodes)
            {
                actionNodes.arraySize++;
                SerializedPropertyExtensions.SetValue(actionNodes.GetArrayElementAtIndex(actionNodes.arraySize - 1), node);
            }

            /*d.playerDiags = diag.playerNodes;
            d.actionNodes = diag.actionNodes;*/

            loadtag.stringValue = diag.loadTag;
            startp.intValue = diag.start;
            this.preload.boolValue = true;

            VIDE_EditorDB.videRoot = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
            VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();
            VIDE_EditorDB.videRoot = Directory.GetParent(VIDE_EditorDB.videRoot).ToString();

            List<VIDE_Localization.VLanguage> langs = VIDE_Localization.PreloadLanguages(assignedDialogue.stringValue);
            this.langs.arraySize = 0;
            foreach(VIDE_Localization.VLanguage lang in langs)
            {
                this.langs.arraySize++;
                SerializedPropertyExtensions.SetValue(this.langs.GetArrayElementAtIndex(this.langs.arraySize - 1), lang);
            }

            notuptodate.boolValue = false;

        }
        else
        {
            playerDiags.arraySize = 0;
            actionNodes.arraySize = 0;
            langs.arraySize = 0;
            loadtag.stringValue = "";
            startp.intValue = 0;
            this.preload.boolValue = false;
            notuptodate.boolValue = false;
        }
       
    }




}
