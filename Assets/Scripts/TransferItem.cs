using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TransferItem : MonoBehaviour
{

    [SerializeField]
    private GameObject portal1;
    [SerializeField]
    private GameObject portal2;

    [SerializeField]
    private GameObject portal3;
    [SerializeField]
    private GameObject portal4;
    private GrabPushRotate gpr;
    [SerializeField]
    private GameObject portal1DropPoint; 
    [SerializeField]
    private GameObject portal2DropPoint; 
    [SerializeField]
    private GameObject portal3DropPoint; 
    [SerializeField]
    private GameObject portal4DropPoint;
    [HideInInspector] public Transform activePortal;
    private Transform activeDropPoint;
    [HideInInspector] public bool p1Active = false;
    [HideInInspector] public bool p2Active = false;
    [HideInInspector] public bool p3Active = false;
    [HideInInspector] public bool p4Active = false;

	void OnTriggerEnter(Collider other)
    {
        if (other.tag == "BuildingObject")
        {
            gpr = other.gameObject.GetComponent<GrabPushRotate>();
            if (activePortal != null && activeDropPoint != null)
            {
                Rigidbody objRB = other.gameObject.GetComponent<Rigidbody>();
                other.transform.gameObject.SetActive(false);
                objRB.useGravity = false;
                objRB.isKinematic = true;
                other.transform.position = new Vector3(activePortal.position.x,
                activePortal.position.y + 0.5f, activePortal.position.z);
                other.transform.gameObject.SetActive(true);
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Play();
                }
                StartCoroutine(MoveFromHoverToTable(other.transform, other.transform.position));
            }
        }
    }

    void Update()
    {
        activePortal = null;
        activeDropPoint = null;

        if (p1Active)
        {
            activePortal = portal1.transform;
            activeDropPoint = portal1DropPoint.transform;
        }
        else if (p2Active)
        {
            activePortal = portal2.transform;
            activeDropPoint = portal2DropPoint.transform;
        }
        else if (p3Active)
        {
            activePortal = portal3.transform;
            activeDropPoint = portal3DropPoint.transform;
        }
        else if (p4Active)
        {
            activePortal = portal4.transform;
            activeDropPoint = portal4DropPoint.transform;
        }
    }


	IEnumerator MoveFromHoverToTable(Transform obj, Vector3 startPos)
    {
        gpr.isRightHandTouching = false;
        gpr.isLeftHandTouching = false;
        float waitTime = 3f;
        float moveDuration = 0.8f;

        yield return new WaitForSeconds(waitTime);

        if (activeDropPoint == null) {
            yield return null;
        }
        Vector3 endPos = activeDropPoint.position;
        Quaternion startRot = obj.rotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, 0f);
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            obj.position = Vector3.Slerp(startPos, endPos, t);
            obj.rotation = Quaternion.Slerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.position = endPos;
        obj.rotation = endRot;
        obj.GetComponent<Rigidbody>().useGravity = true;
        obj.GetComponent<Rigidbody>().isKinematic = false;
    }
}
