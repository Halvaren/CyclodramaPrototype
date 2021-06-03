using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlembicSetBehavior : SetBehavior
{
    public AlembicPlayerDictionary alembicPlayerDictionary;

    Transform alembicPlayerContainer;
    List<Transform> alembicPlayersInSet;

    protected override void InitializeSet()
    {
        alembicPlayerContainer = GameObject.FindGameObjectWithTag("AlembicPlayerContainer").transform;
        alembicPlayersInSet = new List<Transform>();

        for (int i = 0; i < alembicPlayerContainer.childCount; i++)
        {
            Transform alembicPlayer = alembicPlayerContainer.GetChild(i);
            if(alembicPlayerDictionary.ContainsKey(alembicPlayer.gameObject.tag))
            {
                Transform alembicParent = alembicPlayerDictionary[alembicPlayer.gameObject.tag];
                alembicPlayer.position = alembicParent.position;
                alembicPlayer.rotation = alembicParent.rotation;
                alembicPlayer.localScale = alembicParent.localScale;
                alembicPlayer.parent = alembicParent;

                if(alembicPlayer.GetComponent<AlembicAnimatorListener>() != null && alembicParent.GetComponent<InteractableObjBehavior>() != null)
                {
                    alembicPlayer.GetComponent<AlembicAnimatorListener>().behavior = alembicParent.GetComponent<InteractableObjBehavior>();
                }

                alembicPlayersInSet.Add(alembicPlayer);

                alembicPlayer.gameObject.SetActive(true);
            }
        }

        base.InitializeSet();
    }

    private void OnDestroy()
    {
        foreach(Transform alembicPlayer in alembicPlayersInSet)
        {
            alembicPlayer.gameObject.SetActive(false);

            alembicPlayer.parent = alembicPlayerContainer;

            if (alembicPlayer.GetComponent<AlembicAnimatorListener>() != null) alembicPlayer.GetComponent<AlembicAnimatorListener>().behavior = null;
        }
    }
}

[System.Serializable]
public class AlembicPlayerDictionary : SerializableDictionaryBase<string, Transform>
{

}
