using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Fusion;
using cakeslice;
using Unity.VisualScripting;

public class GrabPushRotate : MonoBehaviour
{
    [HideInInspector] public static GameObject currentlyManipulatedObject = null;
    private static bool isAnyObjectBeingManipulated = false;

    private Transform leftHand;
    private Transform rightHand;
    private Transform leftIndex;
    private Transform rightIndex;
    private Collider leftHandCollider;
    private Collider rightHandCollider;
    private Collider leftIndexCollider;
    private Collider rightIndexCollider;
    [HideInInspector] public bool isLeftHandTouching = false;
    [HideInInspector] public bool isRightHandTouching = false;
    [HideInInspector] public bool isLeftIndexTouching = false;
    [HideInInspector] public bool isRightIndexTouching = false;

    public AudioSource audioSource;

    public float rotationSpeed = 35f;
    public float moveSpeed = 10f;
    public float tableHeight = 1.125f;
    public float rotationSensitivity = 2f;
    private float fixedY;
    private Rigidbody rb;
    private Vector3 lastHandPosition;
    private Vector3 handOffset;
    private Vector3 lastRightHandPosition;
    private Vector3 lastLeftHandPosition;
    private float lastAngle;
    private float lastLeftIndexTapTime = -1f;
    private float lastRightIndexTapTime = -1f;
    private float doubleTapThreshold = 0.3f;
    private GameObject lastPokedObject = null;
    private bool leftIndexInside = false;
    private bool rightIndexInside = false;
    private float lastFingerAngle = 0f;
    private bool handOffsetSet = false;
    private XRGrabInteractable interactable;
    private Outline outline;
    private bool isBeingTranslated = false;
    private Vector3 translationOffset;

    void Awake()
    {
        /* add necessary components if not present */
        interactable = GetComponent<XRGrabInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<XRGrabInteractable>();

        //NetworkObject networkObject = GetComponent<NetworkObject>();
        //if (networkObject == null)
        //networkObject = gameObject.AddComponent<NetworkObject>();

        //NetworkTransform networkTransform = GetComponent<NetworkTransform>();
        // if (networkTransform == null)
        //networkTransform = gameObject.AddComponent<NetworkTransform>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            AudioClip turnClip = Resources.Load<AudioClip>("turn");
            if (turnClip != null)
            {
                audioSource.clip = turnClip;
            }
        }

        gameObject.layer = LayerMask.NameToLayer("Default");

        interactable.selectEntered.AddListener(OnGrabStart);
        interactable.selectExited.AddListener(OnGrabEnd);

        Collider[] colliders = GetComponents<Collider>();
        if (colliders.Length > 0)
        {
            interactable.colliders.Clear();
            foreach (Collider collider in colliders)
            {
                interactable.colliders.Add(collider);
            }
        }

        XRInteractionManager interactionManager = FindObjectOfType<XRInteractionManager>();
        if (interactionManager != null)
        {
            interactable.interactionManager = interactionManager;
        }
        else
        {
            Debug.LogError("No XR Interaction Manager found in the scene");
        }

        interactable.movementType = XRBaseInteractable.MovementType.Instantaneous;
        interactable.throwOnDetach = true;
        interactable.throwSmoothingDuration = 0.25f;
        interactable.throwVelocityScale = 1.5f;
        interactable.throwAngularVelocityScale = 1f;
        interactable.attachEaseInTime = 0.15f;
        interactable.matchAttachPosition = true;
        interactable.matchAttachRotation = true;
        //interactable.snapToColliderVolume = true;
        //interactable.reinitializeDynamicAttachEverySingleGrab = true;

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;
        rb.useGravity = false;

        StartCoroutine(UnfreezeAfterDelay());

        leftHand = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Left Hand/Left Hand Interaction Visual/L_Wrist").transform;
        rightHand = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Right Hand/Right Hand Interaction Visual/R_Wrist").transform;
        leftIndex = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Left Hand/Left Hand Interaction Visual/L_Wrist/L_IndexMetacarpal/L_IndexProximal/L_IndexIntermediate/L_IndexDistal/LeftIndexDistalCollider").transform;
        rightIndex = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Right Hand/Right Hand Interaction Visual/R_Wrist/R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal/RightIndexDistalCollider").transform;
        leftHandCollider = leftHand.GetComponent<Collider>();
        rightHandCollider = rightHand.GetComponent<Collider>();
        leftIndexCollider = leftIndex.GetComponent<Collider>();
        rightIndexCollider = rightIndex.GetComponent<Collider>();

        fixedY = tableHeight;
        lastRightHandPosition = rightHand.position;
        lastLeftHandPosition = leftHand.position;

        // Add Outline component if not present
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.enabled = false;
        }
    }

    private IEnumerator UnfreezeAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        if (currentlyManipulatedObject == gameObject)
        {
            HandleTranslation();
            HandleRotation();
        }
    }

    private void HandleTranslation()
    {
        // Only handle direct hand translation if we're not being grabbed by the ray
        if (isLeftHandTouching && currentlyManipulatedObject == gameObject && !interactable.isSelected)
        {
            if (!isBeingTranslated)
            {
                // Start translation - calculate offset from hand to object
                translationOffset = transform.position - leftHand.position;
                isBeingTranslated = true;
            }

            // Move object with hand while maintaining the offset and table height
            Vector3 targetPosition = leftHand.position + translationOffset;
            targetPosition.y = tableHeight;
            transform.position = targetPosition;
        }
        else if (isBeingTranslated && !isLeftHandTouching)
        {
            isBeingTranslated = false;
        }
    }

    private void HandleRotation()
    {
        if (isRightHandTouching && currentlyManipulatedObject == gameObject)
        {
            float rotationAmount = rotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0, Space.World);
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        float now = Time.time;

        if (other == leftIndexCollider)
        {
            leftIndexInside = true;
            isLeftIndexTouching = true;

            // Check for double tap
            if (lastLeftIndexTapTime > 0 && now - lastLeftIndexTapTime < doubleTapThreshold)
            {
                Debug.Log("Double tap detected!");
                SelectObject();
                lastLeftIndexTapTime = -1f; // Reset to prevent triple-tap
            }
            else
            {
                lastLeftIndexTapTime = now;
            }
        }

        if (other == leftHandCollider)
        {
            isLeftHandTouching = true;
            if (currentlyManipulatedObject == gameObject)
            {
                translationOffset = transform.position - leftHand.position;
                isBeingTranslated = true;
            }
        }
        if (other == rightHandCollider)
        {
            isRightHandTouching = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == leftIndexCollider)
        {
            leftIndexInside = false;
            isLeftIndexTouching = false;
        }
        if (other == leftHandCollider)
        {
            isLeftHandTouching = false;
            if (currentlyManipulatedObject == gameObject)
            {
                isBeingTranslated = false;
            }
        }
        if (other == rightHandCollider)
        {
            isRightHandTouching = false;
        }
    }

    float GetFingerAngle()
    {
        var dir = rightIndex.position - transform.position;
        return Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
    }

    public static void DeselectCurrent()
    {
        if (currentlyManipulatedObject == null)
            return;

        var script = currentlyManipulatedObject.GetComponent<GrabPushRotate>();
        if (script != null)
        {
            script.DeselectObject(currentlyManipulatedObject);

            script.isLeftHandTouching = false;
            script.isRightHandTouching = false;
            script.isLeftIndexTouching = false;
            script.isRightIndexTouching = false;
        }

        currentlyManipulatedObject = null;
        isAnyObjectBeingManipulated = false;
    }

    void OnGrabStart(SelectEnterEventArgs args)
    {
        Debug.Log("Grab started on: " + gameObject.name);
        // When grabbed by ray, stop direct hand translation
        isBeingTranslated = false;
        isLeftHandTouching = false;
    }

    void OnGrabEnd(SelectExitEventArgs args)
    {
        Debug.Log("Grab ended on: " + gameObject.name);
        isBeingTranslated = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        StartCoroutine(HandleUprightPlacement());
    }

    private IEnumerator HandleUprightPlacement()
    {
        bool hasLanded = false;
        float lastYVelocity = 0f;

        while (!hasLanded)
        {
            if (rb.velocity.y < 0 && Mathf.Abs(transform.position.y - tableHeight) < 0.1f)
            {
                hasLanded = true;
                Vector3 currentPos = transform.position;
                currentPos.y = tableHeight;
                transform.position = currentPos;

                Quaternion currentRot = transform.rotation;
                Vector3 eulerAngles = currentRot.eulerAngles;
                eulerAngles.x = 0;
                eulerAngles.z = 0;
                transform.rotation = Quaternion.Euler(eulerAngles);

                Vector3 velocity = rb.velocity;
                velocity.y = 0;
                rb.velocity = velocity;
            }

            lastYVelocity = rb.velocity.y;
            yield return new WaitForFixedUpdate();
        }
    }

    void SelectObject()
    {
        Debug.Log("Selecting object: " + gameObject.name);

        // Deselect previous object if exists
        if (currentlyManipulatedObject != null && currentlyManipulatedObject != gameObject)
        {
            DeselectObject(currentlyManipulatedObject);
        }

        // Select this object
        gameObject.layer = LayerMask.NameToLayer("Objects");
        currentlyManipulatedObject = gameObject;
        isAnyObjectBeingManipulated = true;
        handOffsetSet = false;
        isBeingTranslated = false;

        if (interactable != null)
            interactable.enabled = true;

        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    void DeselectObject(GameObject obj)
    {
        if (obj == null) return;

        obj.layer = LayerMask.NameToLayer("Default");
        var prevInteractable = obj.GetComponent<XRGrabInteractable>();
        if (prevInteractable != null)
            prevInteractable.enabled = false;

        var prevOutline = obj.GetComponent<Outline>();
        if (prevOutline != null)
        {
            prevOutline.enabled = false;
        }
    }

    private IEnumerator ReenableInteractable(XRGrabInteractable interactable)
    {
        yield return new WaitForSeconds(0.1f);
        if (interactable != null)
        {
            interactable.enabled = true;
            interactable.interactionLayers = InteractionLayerMask.GetMask("Default", "Objects");
        }
    }
}
