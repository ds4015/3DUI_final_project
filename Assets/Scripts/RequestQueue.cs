using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RequestQueue : MonoBehaviour
{
    public Transform requestList;
    public GameObject requestQueue;
    public GameObject placeholderQueue;
    public string playerInThisPosition;

    private void Start()
    {
        var queueManager = FindObjectOfType<NetworkManagerObject>();
        if(queueManager != null )
        {
            queueManager.RegisterQueue(playerInThisPosition, this);
        }

    }
    public void HandleSend(string toPlayer, GameObject itemPrefab)
    {
        var queueNetManager = FindObjectOfType<NetworkManagerObject>();
        if (queueNetManager != null)
        {
            queueNetManager.RPC_SpawnItem(toPlayer, itemPrefab.name);
        }
    }
    public void AddRequest(string toPlayer, string currPlayer, GameObject itemPrefab)
    {
        if (toPlayer != playerInThisPosition)
        {
            Debug.Log($"toPlayer: {toPlayer}");
            Debug.Log($"playerinthisposition: {playerInThisPosition}");
            //return;
        }
        Debug.Log("Add Request called");
        Debug.Log("Item Prefab sent: " + itemPrefab);

        
        GameObject card = Instantiate(requestQueue, requestList);
        card.transform.SetSiblingIndex(0);
        TMPro.TextMeshProUGUI txt = card.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        txt.text = $"{currPlayer} is requesting";
        Transform previewPoint = card.transform.Find("Panel/PreviewPoint");
        if(previewPoint != null)
        {
            GameObject previewPrefab = Instantiate(itemPrefab, previewPoint);
            previewPrefab.transform.localPosition = Vector3.zero;
            previewPrefab.transform.localScale *= 30f;
            //previewPoint.transform.localScale *= 0.2f;
        }
        else
        {
            Debug.Log("No previewpoint");
        }
        Transform sendButtonQ = card.transform.Find("Panel/Canvas/SendButton");
        sendButtonQ.gameObject.SetActive(true);
        if(sendButtonQ  != null)
        {
            Debug.Log("SendButtonQ not null");
            SendQueueButton sendButton = sendButtonQ.GetComponent<SendQueueButton>();
            if(sendButton != null)
            {
                Debug.Log("sendbutton not null");
                sendButton.onPress.AddListener(() => { HandleSend(currPlayer, itemPrefab); 
                Destroy(card);
                });

            }
        }

    }
    

}
