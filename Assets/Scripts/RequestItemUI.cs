using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RequestItemUI : MonoBehaviour
{
    public TextMeshPro msg;
    public Transform itemPreview;
    private GameObject currentPreview;
    //public Button yesButton;
    //public Button noButton;

    private GameObject requestItemPrefab;
    private string requestName;
    private string currPlayer;

    public void Show(string reqName, string currPlayer, GameObject itemPrefab)
    {
        msg.text = $"Request: ";
        if (currPlayer == "Player 1")
        {
            transform.position = new Vector3(-1.3578f, 1.538f, -0.2454f);
        }
        if (currPlayer == "Player 2")
        {
            transform.position = new Vector3(-0.3221f, 1.5712f, 1.1840f);
        }
        //Vector3(-0.322100013,1.57120001,1.18400002)
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        this.requestItemPrefab = itemPrefab;
        this.requestName = reqName;
        this.currPlayer = currPlayer;
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
        Debug.Log(itemPreview.position);
        Debug.Log(itemPreview.rotation);
        currentPreview = Instantiate(itemPrefab, itemPreview.position, itemPreview.rotation);

        Debug.Log("itemPreview: " + itemPreview);
        Debug.Log("itemPrefab: " + itemPrefab);
        currentPreview.transform.localScale *= 0.05f;

        gameObject.SetActive(true);

        //yesButton.onClick.AddListener(YesPressed);
        //noButton.onClick.AddListener(NoPressed);
    }

    public void YesPressed()
    {
        Debug.Log($"Pressed Yes. Requesting {requestItemPrefab.name} from {requestName}");
        //RequestQueue queue = FindObjectOfType<RequestQueue>();
        var queueManager = FindObjectOfType<NetworkManagerObject>();
        //if (queue != null)
        //{
        //queue.AddRequest(requestName, currPlayer, requestItemPrefab);
        //}
        if (queueManager != null)
        {
            queueManager.RPC_SendRequest(requestName, currPlayer, requestItemPrefab.name);
        }
        Destroy(currentPreview);
        Destroy(gameObject);
    }
    public void NoPressed()
    {
        Destroy(currentPreview);
        Destroy(gameObject);
    }
}
