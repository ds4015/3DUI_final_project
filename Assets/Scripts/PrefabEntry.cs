using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

[System.Serializable]
public struct PrefabEntry
{
    public string prefabName;
    public NetworkPrefabRef prefabRef;
    public GameObject previewPrefab;
}
