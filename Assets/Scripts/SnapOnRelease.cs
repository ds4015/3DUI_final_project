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
        float y = transform.eulerAngles.y;     
        waitForSnap = false;

    }

    void OnDestroy()
    {
        grab.selectExited.RemoveListener(OnReleased);
    }
}
