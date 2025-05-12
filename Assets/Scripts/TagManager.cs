using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class TagManager : MonoBehaviour
{
    // This script is only used in the editor to ensure required tags exist
    
    [MenuItem("Tools/Setup Required Tags")]
    public static void SetupRequiredTags()
    {
        // Add the ARVRToggleButton tag if it doesn't exist
        AddTag("ARVRToggleButton");
        
        Debug.Log("Required tags have been set up!");
    }
    
    private static void AddTag(string tag)
    {
        // Open tag manager
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        bool found = false;
        
        // Check if tag already exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag))
            {
                found = true;
                break;
            }
        }
        
        // Add the tag if it doesn't exist
        if (!found)
        {
            tagsProp.arraySize++;
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            newTag.stringValue = tag;
            
            Debug.Log("Added tag: " + tag);
        }
        
        // Apply changes
        tagManager.ApplyModifiedProperties();
    }
}
#endif