using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestQueue : MonoBehaviour
{
    public Transform requestList;
    public GameObject requestQueue;
    public GameObject placeholderQueue;
    public string playerInThisPosition;

    public void AddRequest(string toPlayer, string currPlayer, GameObject itemPrefab)
    {
        if (toPlayer != playerInThisPosition)
        {
            return;
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
    }

}
