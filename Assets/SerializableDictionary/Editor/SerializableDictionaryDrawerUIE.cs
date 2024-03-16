using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SerializableDictionaryConverter<KeyType, ValueType> : UxmlAttributeConverter<SerializableDictionary<KeyType, ValueType>>
{
    static string ValueToString(object InValue) => System.Convert.ToString(InValue, CultureInfo.InvariantCulture);

    public override string ToString(SerializableDictionary<KeyType, ValueType> InSource)
    {
        var DataBuilder = new StringBuilder();

        foreach(var KVP in InSource)
        {
            DataBuilder.Append($"{ValueToString(KVP.Key)}|{ValueToString(KVP.Value)},");
        }

        return DataBuilder.ToString();
    }

    public override SerializableDictionary<KeyType, ValueType> FromString(string InSource)
    {
        var OutputDictionary = new SerializableDictionary<KeyType, ValueType>();

        var KeyValuePairs = InSource.Split(',');
        foreach(var KVP in KeyValuePairs)
        {
            var Fields = KVP.Split("|");
            KeyType Key = (KeyType)System.Convert.ChangeType(Fields[0], typeof(KeyType));
            ValueType Value = (ValueType)System.Convert.ChangeType(Fields[1], typeof(ValueType));

            OutputDictionary.EditorOnly_Add(Key, Value);
        }

        OutputDictionary.SynchroniseToSerializedData();

        return OutputDictionary;
    }
}

[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawerUIE : PropertyDrawer
{
    SerializedProperty LinkedProperty;
    SerializedProperty LinkedKeys;
    SerializedProperty LinkedValues;

    public override VisualElement CreatePropertyGUI(SerializedProperty InProperty)
    {
        LinkedProperty = InProperty;
        LinkedKeys = InProperty.FindPropertyRelative("SerializedKeys");
        LinkedValues = InProperty.FindPropertyRelative("SerializedValues");

        var ContainerUI = new Foldout()
        {
            text = InProperty.displayName,
            viewDataKey = $"{InProperty.serializedObject.targetObject.GetInstanceID()}.{InProperty.name}"
        };

        var ContentsUI = new ListView()
        {
            showAddRemoveFooter = true,
            showBorder = true,
            showAlternatingRowBackgrounds = AlternatingRowBackground.All,
            showFoldoutHeader = false,
            showBoundCollectionSize = false,
            reorderable = false,
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            headerTitle = InProperty.displayName,
            bindingPath = LinkedKeys.propertyPath,
            bindItem = BindListItem,
            overridingAddButtonBehavior = OnAddButton,
            onRemove = OnRemove
        };

        ContainerUI.Add(ContentsUI);

        var RemoveDuplicatesButton = new Button() { text = "Remove Duplicates" };
        RemoveDuplicatesButton.clicked += OnRemoveDuplicates;

        ContainerUI.Add(RemoveDuplicatesButton);

        return ContainerUI;
    }

    bool AreDuplicatesOfKeyPresent(SerializedProperty InKeyProperty, int InKeyIndex)
    {
        for (int Index = 0; Index < LinkedKeys.arraySize; Index++)
        {
            // skip if this is our key
            if (Index == InKeyIndex)
                continue;

            SerializedProperty OtherKey = LinkedKeys.GetArrayElementAtIndex(Index);

            if (OtherKey.boxedValue.Equals(InKeyProperty.boxedValue))
                return true;
        }

        return false;
    }

    void BindListItem(VisualElement InItemUI, int InItemIndex)
    {
        InItemUI.Clear();
        InItemUI.Unbind();

        var KeyProperty = LinkedKeys.GetArrayElementAtIndex(InItemIndex);
        var ValueProperty = LinkedValues.GetArrayElementAtIndex(InItemIndex);

        var KeyUI = new PropertyField(KeyProperty) { label = "Key" };
        var ValueUI = new PropertyField(ValueProperty) { label = "Value" };

        InItemUI.Add(KeyUI);
        InItemUI.Add(ValueUI);

        var WarningUI = new Label("<b>Error: Duplicate Key Detected</b>");
        InItemUI.Add(WarningUI);

        WarningUI.visible = AreDuplicatesOfKeyPresent(KeyProperty, InItemIndex);

        InItemUI.TrackPropertyValue(KeyProperty, (SerializedProperty InKeyProp) =>
        {
            WarningUI.visible = AreDuplicatesOfKeyPresent(KeyProperty, InItemIndex);
        });

        InItemUI.Bind(LinkedProperty.serializedObject);
    }

    void OnAddButton(BaseListView InListView, Button InButton)
    {
        LinkedKeys.InsertArrayElementAtIndex(LinkedKeys.arraySize);
        LinkedValues.InsertArrayElementAtIndex(LinkedValues.arraySize);
        LinkedProperty.serializedObject.ApplyModifiedProperties();
    }

    void OnRemoveDuplicates()
    {
        List<int> IndicesToRemove = new();

        // search for any duplicates
        for (int Index = 0; Index < LinkedKeys.arraySize; Index++)
        {
            SerializedProperty FirstKey = LinkedKeys.GetArrayElementAtIndex(Index);

            for (int OtherIndex = Index + 1; OtherIndex < LinkedKeys.arraySize; OtherIndex++) 
            { 
                SerializedProperty OtherKey = LinkedKeys.GetArrayElementAtIndex(OtherIndex);

                if (FirstKey.boxedValue.Equals(OtherKey.boxedValue) && 
                    !IndicesToRemove.Contains(OtherIndex))
                {
                    IndicesToRemove.Add(OtherIndex);
                }
            }
        }

        // Remove the duplicates
        for (int Index = IndicesToRemove.Count - 1; Index >= 0; Index--)
        { 
            int IndexToRemove = IndicesToRemove[Index];
            LinkedKeys.DeleteArrayElementAtIndex(IndexToRemove);
            LinkedValues.DeleteArrayElementAtIndex(IndexToRemove);
        }
        LinkedProperty.serializedObject.ApplyModifiedProperties();
    }

    void OnRemove(BaseListView InListView)
    {
        if ((LinkedKeys.arraySize > 0) && 
            (InListView.selectedIndex >= 0) &&
            (InListView.selectedIndex < LinkedKeys.arraySize)) 
        {
            int IndexToRemove = InListView.selectedIndex;

            LinkedKeys.DeleteArrayElementAtIndex(IndexToRemove);
            LinkedValues.DeleteArrayElementAtIndex(IndexToRemove);
            LinkedProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
