using System.Collections;
using Unity;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

public class PositionPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject startMarker;
    [SerializeField]
    XROrigin origin;
    

    IEnumerator Start()
    {
        yield return null;
        origin.transform.rotation = startMarker.transform.rotation;
        origin.MoveCameraToWorldLocation(startMarker.transform.position);        
    } 

}
