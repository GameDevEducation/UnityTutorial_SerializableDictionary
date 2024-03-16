using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryTest : MonoBehaviour
{
    public SerializableDictionary<string, float> StringToFloatMap;
    public SerializableDictionary<string, Vector3> StringToVector3Map;
    public SerializableDictionary<string, GameObject> StringToGameObjectMap;
    public SerializableDictionary<GameObject, string> GameObjectToStringMap;
    public SerializableDictionary<string, Color> StringToColourMap;

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogWarning("StringToFloatMap");
        foreach(var KVP in StringToFloatMap)
            Debug.Log($"{KVP.Key} = {KVP.Value}");

        Debug.LogWarning("StringToVector3Map");
        foreach (var KVP in StringToVector3Map)
            Debug.Log($"{KVP.Key} = {KVP.Value}");

        Debug.LogWarning("StringToGameObjectMap");
        foreach (var KVP in StringToGameObjectMap)
            Debug.Log($"{KVP.Key} = {KVP.Value}");

        Debug.LogWarning("GameObjectToStringMap");
        foreach (var KVP in GameObjectToStringMap)
            Debug.Log($"{KVP.Key} = {KVP.Value}");

        Debug.LogWarning("StringToColourMap");
        foreach (var KVP in StringToColourMap)
            Debug.Log($"{KVP.Key} = {KVP.Value}");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
