using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PortToPosition : MonoBehaviour
{
    public GameObject portPosition;
    [SerializeField]
    private XROrigin xrRig;


	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("IndexFingerCollider"))
        {
            xrRig.transform.rotation = portPosition.transform.rotation;
            xrRig.MoveCameraToWorldLocation(portPosition.transform.position);
        }
	}

}
