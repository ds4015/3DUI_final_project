using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTabMenu : MonoBehaviour
{
    public List<GameObject> pages;
    public int currentPage = 0;
    public int firstPage;

    void Start()
    {
        ShowTab(firstPage);
    }

    void ShowTab(int page)
    {
        if(page < 0 && page >= pages.Count)
        {
            return;
        }
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == page);
            Debug.Log("Switched to page " + page);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("IndexFingerCollider"))
        {
            return;
        }
        ShowTab(currentPage);
    }

}
