#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// This script is used to ensure that the correct Projection Eye Rig layers are created and assigned to the objects in the sample scene.
/// </summary>
/// <remarks>
/// When creating a new Projection Eye Rig, a layer is created and is also removed from the main camera culling mask.
/// As the sample scene uses a Projection Eye Rig, this script ensures that the layers are created and assigned correctly.
/// </remarks>
[InitializeOnLoad]
public static class SampleLayers
{
    const string k_ScenePath = "Sample Composition Layers Scene/Scenes/SampleCompositionLayersScene.unity";

    static (string, int)[] s_CreatedLayers =
    {
        ("Midground Objects Layer", 8)
    };

    static SerializedObject s_TagManager;
    static SerializedProperty s_Layers;

    static SampleLayers()
    {
        EditorSceneManager.sceneOpened += (scene, _) =>
        {
            s_TagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            s_Layers = s_TagManager.FindProperty("layers");

            if (scene.path.Contains(k_ScenePath))
                AssignIncorrectLayers();
        };
    }

    static void AssignIncorrectLayers()
    {
        foreach (var (currentLayer, currentIndex) in s_CreatedLayers)
        {
            GameObject[] objectsWithLayer = FindObjectsWithLayer(currentIndex);

            if (objectsWithLayer.Length == 0)
                continue;

            string layerName = GetLayerNameFromIndex(currentIndex);
            if (layerName == string.Empty || layerName != currentLayer) // Layer is empty or Layer is not empty and has a different name
            {
                int newLayer = CreateLayerAndAssignObjects(currentLayer, objectsWithLayer);
                RemoveLayerFromCamera(newLayer);
            }
        }
    }

    static void RemoveLayerFromCamera(int layer)
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        camera.cullingMask &= ~(1 << layer);
    }

    static int CreateLayerAndAssignObjects(string layerName, GameObject[] objects)
    {
        int layer = CreateLayer(layerName);
        foreach (var go in objects)
        {
            // Special case for cameras
            if (go.TryGetComponent<Camera>(out var camera))
                camera.cullingMask = 1 << layer;

            go.layer = layer;
        }

        return layer;
    }

    static int CreateLayer(string layerName)
    {
        int index = LayerExists(layerName);
        if (index != -1) return index;

        for (int i = 8; i < s_Layers.arraySize; i++)
        {
            SerializedProperty layer = s_Layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                index = i;
                break;
            }
        }

        SerializedProperty newLayerProperty = s_Layers.GetArrayElementAtIndex(index);
        newLayerProperty.stringValue = layerName;

        s_TagManager.ApplyModifiedProperties();

        return index;
    }

    static int LayerExists(string layerName)
    {
        for (int i = 8; i < s_Layers.arraySize; i++)
        {
            SerializedProperty layer = s_Layers.GetArrayElementAtIndex(i);
            if (layer.stringValue == layerName)
                return i;
        }

        return -1;
    }

    static GameObject[] FindObjectsWithLayer(int layer)
    {
        var goArray = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var objectsWithLayer = new List<GameObject>();

        foreach (var go in goArray)
            if (go.layer == layer)
                objectsWithLayer.Add(go);

        return objectsWithLayer.ToArray();
    }

    static string GetLayerNameFromIndex(int layer)
    {
        return s_Layers.GetArrayElementAtIndex(layer).stringValue;
    }
}
#endif // UNITY_EDITOR
