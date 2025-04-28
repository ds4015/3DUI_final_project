using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class GrabPushRotate : MonoBehaviour
{
	private Transform leftHand;
	private Transform rightHand;

	[Header("Object References")]
	public AudioSource audioSource;

	[Header("Settings")]
	public float rotationSpeed = 45f; 
	public float moveSpeed = 2f; 
	public float tableHeight = 1.122f; 

	private Collider leftHandCollider;
	private Collider rightHandCollider;
	[HideInInspector] public bool isLeftHandTouching = false;
	[HideInInspector] public bool isRightHandTouching = false;
	private float fixedY;
	private Rigidbody rb;
	private Vector3 lastHandPosition;
	private Vector3 handOffset;

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
		leftHandCollider = leftHand.GetComponent<Collider>();
		rightHandCollider = rightHand.GetComponent<Collider>();

		fixedY = tableHeight;
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
		/* left hand translates object */
		if (isLeftHandTouching)
		{
			Vector3 targetPos = new Vector3(
				leftHand.position.x + handOffset.x,
				fixedY,
				leftHand.position.z + handOffset.z
			);

			Vector3 targetVelocity = ((targetPos - transform.position) / Time.fixedDeltaTime) * 2.5f;
			targetVelocity.y = 0f;

			rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, 0.1f);
			rb.constraints = RigidbodyConstraints.FreezeRotation;
		}

		/* right hand rotates object */
		if (isRightHandTouching && Mathf.Abs(transform.position.y - fixedY) < 0.01f)
		{
			transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
			if (!audioSource.isPlaying)
				audioSource.Play();
		}
		else if (audioSource.isPlaying)
			audioSource.Stop();
	}

	void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("HandCollider"))
			return;

		lastHandPosition = other.transform.position;
		if (other == leftHandCollider)
		{
			isLeftHandTouching = true;
			handOffset = transform.position - leftHand.position;
		}
		else if (other == rightHandCollider)
			isRightHandTouching = true;
	}

	void OnTriggerExit(Collider other)
	{
		if (!other.gameObject.CompareTag("HandCollider"))
			return;

		if (other == leftHandCollider)
		{
			isLeftHandTouching = false;
			rb.constraints = RigidbodyConstraints.FreezeRotation;
		}
		else if (other == rightHandCollider)
		{
			isRightHandTouching = false;
			audioSource.Stop();
		}
		rb.velocity = Vector3.zero;
	}
}
	
