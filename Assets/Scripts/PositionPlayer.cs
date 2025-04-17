using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PositionPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject startMarker;
    

    IEnumerator Start()
    {
         yield return null;

        transform.SetPositionAndRotation(startMarker.transform.position, startMarker.transform.rotation);
    } 

}
