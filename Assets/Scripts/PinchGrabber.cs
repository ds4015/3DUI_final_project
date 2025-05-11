using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor))]
public class PinchGrabber : MonoBehaviour
{

    public Transform indexTipTransform;
    public Transform thumbTipTransform;

    public float pinchThreshold = 0.05f;

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    XRInteractionManager manager;
    UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable grabbed;
    bool wasPinching;

    void Awake()
    {
        rayInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        manager      = rayInteractor.interactionManager;
    }

    void Update()
    {
        if (indexTipTransform == null || thumbTipTransform == null)
            return;

        float dist = Vector3.Distance(indexTipTransform.position, thumbTipTransform.position);
        bool isPinching = dist < pinchThreshold;

        if (isPinching && !wasPinching)
            TrySelect();
        if (!isPinching && wasPinching)
            TryDeselect();

        wasPinching = isPinching;
    }

    void TrySelect()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) &&
            manager.TryGetInteractableForCollider(hit.collider, out UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable baseInt))
        {
            var interactor = (UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)rayInteractor;
            var target     = (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)   baseInt;
            manager.SelectEnter(interactor, target);
            grabbed = target;
        }
    }

    void TryDeselect()
    {
        if (grabbed != null)
        {
            var interactor = (UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)rayInteractor;
            manager.SelectExit(interactor, grabbed);
            grabbed = null;
        }
    }
}
