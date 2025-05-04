using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


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

	[Header("Settings")]
	public float rotationSpeed = 35f; 
	public float moveSpeed = 2f; 
	public float tableHeight = 1.122f; 
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
	void Start()
	{
		rb = GetComponent<Rigidbody>();

		/* suspend physics temporarily on load until settled */
		/* this prevents objects from colliding with one another initially and messing up scene */
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
}
	
