using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public Dictionary<int, SetData> setDatas;

    public static SceneManager instance;

    private void Awake()
    {
        instance = this;
        setDatas = new Dictionary<int, SetData>();
    }

    public SetData GetSetData(int setID)
    {
        if (setDatas.ContainsKey(setID))
        {
            SetData setData = setDatas[setID];
            /*Debug.Log("Load");
            foreach (int id in setData.doorDatas.Keys)
            {
                Debug.Log(id + " " + setData.doorDatas[id].opened);
            }*/
            return setData;
        }
        else return null;
    }

    public void SetSetData(int setID, SetData setData)
    {
        /*Debug.Log("Save");
        foreach(int id in setData.doorDatas.Keys)
        {
            Debug.Log(id + " " + setData.doorDatas[id].opened);
        }*/

        if(setDatas.ContainsKey(setID))
        {
            setDatas[setID] = setData;
        }
        else
        {
            setDatas.Add(setID, setData);
        }
    }
}
