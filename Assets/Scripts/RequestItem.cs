using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestItem : MonoBehaviour
{
    public string reqName;
    public string currPlayer;
    public string itemName;
    public GameObject itemPrefab;
    public GameObject reqItemUIPrefab;
    //public RequestItemUI requestitemUI;

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("IndexFingerCollider"))
        {
            Debug.Log("other");
            return;
        }
        Debug.Log("Entered!!!");
        GameObject popup = Instantiate(reqItemUIPrefab);
        RequestItemUI requi = popup.GetComponent<RequestItemUI>();
        requi.Show(reqName, currPlayer, itemPrefab);
    }

}
