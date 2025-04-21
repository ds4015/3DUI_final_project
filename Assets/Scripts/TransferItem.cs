using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferItem : MonoBehaviour
{

    [SerializeField]
    private GameObject portal1;
    [SerializeField]
    private GameObject portal2;
    [SerializeField]
    private GameObject tableDropPoint;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Cube entered portal");
        if (other.tag == "BuildingObject")
        {
            Rigidbody objRB = other.gameObject.GetComponent<Rigidbody>();
            other.transform.gameObject.SetActive(false);
            objRB.useGravity = false;
            objRB.isKinematic = true;
            other.transform.position = new Vector3(portal2.transform.position.x,
            portal2.transform.position.y + 0.5f, portal2.transform.position.z);
            other.transform.gameObject.SetActive(true);
            StartCoroutine(MoveFromHoverToTable(other.transform, other.transform.position));
        }
    }

    IEnumerator MoveFromHoverToTable(Transform obj, Vector3 startPos)
    {
        float waitTime = 3f;
        float moveDuration = 0.8f;

        yield return new WaitForSeconds(waitTime);

        Vector3 endPos = tableDropPoint.transform.position;
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
