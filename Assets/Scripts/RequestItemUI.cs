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
        transform.position = new Vector3(-1.3578f, 1.538f, -0.2454f);
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
        RequestQueue queue = FindObjectOfType<RequestQueue>();
        if (queue != null)
        {
            queue.AddRequest(requestName, currPlayer, requestItemPrefab);
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
