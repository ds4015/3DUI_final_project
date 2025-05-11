using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Fusion;

public class GrabPushRotate : MonoBehaviour
{
	private static GameObject currentlyManipulatedObject = null;
	private static Outline currentlySelectedOutline = null;
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
	public float moveSpeed = 2f; 
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
	private float doubleTapThreshold = 0.5f;
	private GameObject lastPokedObject = null;
	private bool leftIndexInside = false;
	private bool rightIndexInside = false;
	private float lastFingerAngle = 0f;
	private bool handOffsetSet = false;
	XRGrabInteractable grabInteractable;

    void Awake()
    {
		/* add necessary components if not present */
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();

        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject == null)
            networkObject = gameObject.AddComponent<NetworkObject>();

        NetworkTransform networkTransform = GetComponent<NetworkTransform>();
        if (networkTransform == null)
            networkTransform = gameObject.AddComponent<NetworkTransform>();

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

        Outline outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.enabled = false;

        grabInteractable.selectEntered.AddListener(OnGrabStart);
        grabInteractable.selectExited.AddListener(OnGrabEnd);

        Collider[] colliders = GetComponents<Collider>();
        if (colliders.Length > 0)
        {
            grabInteractable.colliders.Clear();
            foreach (Collider collider in colliders)
            {
                grabInteractable.colliders.Add(collider);
            }
        }

        XRInteractionManager interactionManager = FindObjectOfType<XRInteractionManager>();
        if (interactionManager != null)
        {
            grabInteractable.interactionManager = interactionManager;
        }
        else
        {
            Debug.LogError("No XR Interaction Manager found in the scene");
        }

        grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grabInteractable.throwOnDetach = true;
        grabInteractable.throwSmoothingDuration = 0.25f;
        grabInteractable.throwVelocityScale = 1.5f;
        grabInteractable.throwAngularVelocityScale = 1f;
        grabInteractable.attachEaseInTime = 0.15f;
        grabInteractable.matchAttachPosition = true;
        grabInteractable.matchAttachRotation = true;
        grabInteractable.snapToColliderVolume = true;
        grabInteractable.reinitializeDynamicAttachEverySingleGrab = true;

        grabInteractable.enabled = false;

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

    void FixedUpdate()
    {
        if (currentlyManipulatedObject != gameObject)
            return;

        /* rotation */
        if (isRightHandTouching)
        {
            float rotationAmount = rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, rotationAmount, 0, Space.World);
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        /* translation */
        else if (isLeftHandTouching && !isRightHandTouching)
        {
            if (handOffsetSet)
            {
                Vector3 targetPos = new Vector3(
                    leftIndex.position.x + handOffset.x,
                    fixedY,
                    leftIndex.position.z + handOffset.z
                );
                float deadZone = 0.01f;
                if ((targetPos - transform.position).sqrMagnitude > deadZone * deadZone)
                {
                    Vector3 newPos = Vector3.Lerp(transform.position, targetPos, 0.5f);
                    rb.MovePosition(newPos);
                }
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
        else if (audioSource.isPlaying)
            audioSource.Stop();

        lastRightHandPosition = rightHand.position;
        lastLeftHandPosition = leftHand.position;
    }

    void OnTriggerEnter(Collider other)
    {
        float now = Time.time;
        /* detect double tap selection */
        if (other == leftIndexCollider && !leftIndexInside)
        {
            leftIndexInside = true;
            isLeftIndexTouching = true;

            if (currentlyManipulatedObject == gameObject && !handOffsetSet)
            {
                handOffset = transform.position - leftIndex.position;
                handOffsetSet = true;
            }
            if (lastPokedObject != gameObject)
            {
                lastLeftIndexTapTime = -1f;
                lastPokedObject = gameObject;
            }
            if (now - lastLeftIndexTapTime < doubleTapThreshold)
            {
                if (currentlyManipulatedObject != null && currentlyManipulatedObject != gameObject)
                {
                    if (currentlySelectedOutline != null)
                        currentlySelectedOutline.enabled = false;
                    var prevGrabInteractable = currentlyManipulatedObject.GetComponent<XRGrabInteractable>();
                    if (prevGrabInteractable != null)
                        prevGrabInteractable.enabled = false;
                    currentlyManipulatedObject = null;
                    isAnyObjectBeingManipulated = false;
                }
                var outline = gameObject.GetComponent<Outline>();
                if (outline != null)
                    outline.enabled = true;
                currentlySelectedOutline = outline;
                currentlyManipulatedObject = gameObject;
                isAnyObjectBeingManipulated = true;
                handOffsetSet = false;
                grabInteractable.enabled = true;
            }
            lastLeftIndexTapTime = now;
        }
        if (other == leftHandCollider)
        {
            isLeftHandTouching = true;
        }
        if (other == rightHandCollider)
        {
            isRightHandTouching = true;
            lastFingerAngle = GetFingerAngle();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == leftIndexCollider)
        {
            leftIndexInside = false;
            isLeftIndexTouching = false;
            handOffsetSet = false;
        }
        if (other == rightIndexCollider)
        {
            rightIndexInside = false;
            isRightIndexTouching = false;
        }
        if (other == leftHandCollider)
        {
            isLeftHandTouching = false;
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

    void OnGrabStart(SelectEnterEventArgs args)
    {
        Debug.Log("Grab started");
    }

    void OnGrabEnd(SelectExitEventArgs args)
    {
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
}
	
