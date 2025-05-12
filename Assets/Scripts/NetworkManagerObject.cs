using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkManagerObject : NetworkBehaviour
{
    public List<PrefabEntry> entries;
    public RequestQueue requestQueue;
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_SendRequest(string toPlayer, string fromPlayer, string prefabName)
    {
        PrefabEntry entry = entries.Find(n => n.prefabName == prefabName);
        if (entry.previewPrefab == null)
        {
            Debug.LogWarning($"{prefabName} does not exist.");
            return;
        }
        if (requestQueue == null)
        {
            Debug.LogError("RequestQueue is not available");
            return;
        }
        requestQueue.AddRequest(toPlayer, fromPlayer, entry.previewPrefab);
    }
}
