using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueSendItem : MonoBehaviour
{
    public RequestQueue queue;
    public string toPlayer;
    public GameObject itemPrefab;

    public void TriggerSend()
    {
        if (queue != null && itemPrefab != null)
        {
            queue.HandleSend(toPlayer, itemPrefab);
        }
        else
        {
            Debug.LogWarning("Missing data in RequestSendAction.");
        }
    }
}
