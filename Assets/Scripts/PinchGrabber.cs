using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRRayInteractor))]
public class PinchGrabber : MonoBehaviour
{
    public Transform indexTipTransform;
    public Transform thumbTipTransform;
    public float pinchThreshold = 0.05f;

    XRRayInteractor     rayInteractor;
    XRInteractionManager manager;
    XRGrabInteractable   grabbed; 
    bool                 wasPinching;

    void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
        manager      = rayInteractor.interactionManager;
    }

    void Update()
    {
        if (indexTipTransform == null || thumbTipTransform == null)
            return;

        bool isPinching = Vector3.Distance(indexTipTransform.position, thumbTipTransform.position)
                          < pinchThreshold;

        if (isPinching && !wasPinching) TrySelect();
        if (!isPinching &&  wasPinching) TryDeselect();
        wasPinching = isPinching;
    }

    void TrySelect()
    {
        var go = GrabPushRotate.currentlyManipulatedObject;
        if (go == null) return;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) &&
            hit.collider.transform.IsChildOf(go.transform))
        {
            var xi = go.GetComponent<XRGrabInteractable>();
            if (xi != null && xi.enabled)
            {
                manager.SelectEnter((IXRSelectInteractor)rayInteractor, (IXRSelectInteractable)xi);
                grabbed = xi;
            }
        }
    }

    void TryDeselect()
    {
        if (grabbed != null)
        {
            if (grabbed.isSelected)
            {
                manager.SelectExit((IXRSelectInteractor)rayInteractor, (IXRSelectInteractable)grabbed);
            }
            grabbed = null;
        }
    }
}
