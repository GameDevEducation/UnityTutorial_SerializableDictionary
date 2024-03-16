using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<KeyType, ValueType> : Dictionary<KeyType, ValueType>, ISerializationCallbackReceiver
{
    public List<KeyType> SerializedKeys = new();
    public List<ValueType> SerializedValues = new();

    public void OnAfterDeserialize()
    {
        SynchroniseToSerializedData();
    }

    public void OnBeforeSerialize() { }

#if UNITY_EDITOR
    public void EditorOnly_Add(KeyType InKey, ValueType InValue)
    {
        SerializedKeys.Add(InKey);
        SerializedValues.Add(InValue);
    }
#endif // UNITY_EDITOR

    public void SynchroniseToSerializedData()
    {
        this.Clear();

        // if we have valid data then build the dictionary
        if ((SerializedKeys != null) && (SerializedValues != null)) 
        { 
            int NumElements = Mathf.Min(SerializedKeys.Count, SerializedValues.Count);
            for (int Index = 0; Index < NumElements; ++Index)
            {
                this[SerializedKeys[Index]] = SerializedValues[Index];
            }
        }
        else
        {
            SerializedKeys = new();
            SerializedValues = new();
        }

        // if the lists are out of sync then rebuild
        if (SerializedKeys.Count != SerializedValues.Count) 
        {
            SerializedKeys = new(Keys);
            SerializedValues = new(Values);
        }
    }
}
