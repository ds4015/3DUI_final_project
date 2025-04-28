using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class SnapOnRelease : MonoBehaviour
{
    XRGrabInteractable grab;
    bool waitForSnap;
    Rigidbody rb;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(OnReleased);
        rb = GetComponent<Rigidbody>();
    }

    void OnReleased(SelectExitEventArgs args)
    {
        waitForSnap = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!waitForSnap) return;

        if (!collision.gameObject.CompareTag("Table")) return;
        

        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        
        Vector3 newPosition = transform.position;
        newPosition.y = collision.transform.position.y + (transform.localScale.y / 2f);
        transform.position = newPosition;
        
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        waitForSnap = false;
    }

    void OnDestroy()
    {
        grab.selectExited.RemoveListener(OnReleased);
    }
}
