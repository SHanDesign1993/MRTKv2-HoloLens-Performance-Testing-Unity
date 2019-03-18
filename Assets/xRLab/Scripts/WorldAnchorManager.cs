﻿using Microsoft.MixedReality.Toolkit.Core.Services;
using Microsoft.MixedReality.Toolkit.Core.Providers.UnityInput;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Persistence;
using System.Collections.Generic;
using UnityEngine.XR.WSA;

public class WorldAnchorManager :MonoBehaviour
{
    public static WorldAnchorManager Instance;
    public WorldAnchorStore anchorStore;
    public Dictionary<string, GameObject> SceneObjects = new Dictionary<string, GameObject>();


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        WorldAnchorStore.GetAsync(WorldAnchorStoreLoaded);
    }

    private void WorldAnchorStoreLoaded(WorldAnchorStore store)
    {
        this.anchorStore = store;
    }

    public bool SaveSceneObject(string objectId, WorldAnchor anchor)
    {
        var result = this.anchorStore.Save(objectId, anchor);

        if (!SceneObjects.ContainsKey(objectId))
        {
            SceneObjects.Add(objectId, anchor.gameObject);
        }
        else
        {
            SceneObjects[objectId] = anchor.gameObject;
        }

        return result;
    }

    
    public WorldAnchor LoadSceneObject(string objectId)
    {
        if (SceneObjects.ContainsKey(objectId))
        {
            var target = SceneObjects[objectId];
            return anchorStore.Load(objectId, target);
        }
        return null;

    }

    public void ClearAllObject() {
        string[] ids = this.anchorStore.GetAllIds();
        for (int index = 0; index < ids.Length; index++)
        {
            anchorStore.Delete(ids[index]);
        }
        SceneObjects.Clear();
    }

    public void RestoreAllSceneObjects()
    {
        foreach (var key in SceneObjects.Keys)
        {
            var target = SceneObjects[key];
            this.anchorStore.Load(key, target);
        }
    }


}