using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkManagerObject : NetworkBehaviour
{
    public List<PrefabEntry> entries;
    //public RequestQueue requestQueue;
    private Dictionary<string, RequestQueue> requestQueue = new();

    public void RegisterQueue(string playerName, RequestQueue queue)
    {
        if (!requestQueue.ContainsKey(playerName))
        {
            requestQueue[playerName] = queue;
        }
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_SendRequest(string toPlayer, string fromPlayer, string prefabName)
    {
        PrefabEntry entry = entries.Find(n => n.prefabName == prefabName);
        if (entry.previewPrefab == null)
        {
            Debug.LogWarning($"{prefabName} does not exist.");
            return;
        }
        //if (requestQueue == null)
        //{
        //Debug.LogError("RequestQueue is not available");
        //return;
        //}
        Debug.Log($"TO PLAYER: {toPlayer}");
        if (!requestQueue.ContainsKey(toPlayer))
        {
            Debug.LogWarning($"RequestQueue not found");
        }
        requestQueue[toPlayer].AddRequest(toPlayer, fromPlayer, entry.previewPrefab);
        //requestQueue[toPlayer].AddRequest(fromPlayer, toPlayer, entry.previewPrefab);
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_SpawnItem(string toPlayer, string prefabName)
    {
        PrefabEntry entry = entries.Find(n => n.prefabName == prefabName);
        if (entry.previewPrefab == null)
        {
            Debug.LogWarning($"{prefabName} does not exist.");
            return;
        }
        Vector3 spawnPosition = new Vector3(-1.5753f, -0.0232f, -0.7475f);
        Runner.Spawn(entry.prefabRef, spawnPosition, Quaternion.identity, null);
    }
}
