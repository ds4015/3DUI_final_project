using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Management;
using System.Linq;


[RequireComponent(typeof(Rigidbody),
  typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class GrabPushRotate : MonoBehaviour
{
	Transform leftFinger;
    Transform rightFinger;
    Transform activeFinger;
    Transform leftPush, rightPush;
    Rigidbody rb;
    Vector3 lastLeftTipProj;
	Vector3 lastRightTipProj;

    bool rotating = false;
	bool canTranslate = true;
    public float autoRotateSpeed = 45f;


    /* good morning */
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.drag = 1f;

        CreateInitialColliders();
    }

    /* not using push colliders/physics for translation anymore, but leaving it */
    void CreatePushCollider(Transform fingerTip, bool isLeft)
    {
        string colliderName = isLeft ? "LeftIndexPushCollider" : "RightIndexPushCollider";
        GameObject pushCollider = new GameObject(colliderName);
        pushCollider.transform.parent = fingerTip;
        pushCollider.transform.localPosition = Vector3.zero;
        pushCollider.transform.localRotation = Quaternion.identity;

        BoxCollider collider = pushCollider.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.02f, 0.02f, 0.02f);
        collider.isTrigger = false;

        Rigidbody rb = pushCollider.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (isLeft)
            leftPush = pushCollider.transform;
        else
            rightPush = pushCollider.transform;
    }

    /* I am triggered when a collider enters my trigger */
    void OnTriggerEnter(Collider other)
    {
        /* if it's not a finger, get lost */
        if (!other.CompareTag("IndexFingerCollider") && !other.CompareTag("ControllerTip"))
            return; 

        /* it's a left pointer */        
        if (other.name.Contains("LeftIndex") || other.name.Contains("L_IndexDistal"))
        {
            leftFinger = other.transform;

            /* old logic: not using physics for translation anymore, but leaving it anyway */
            if (!leftPush)
                CreatePushCollider(leftFinger, true);

            if (!rotating)
                leftPush.gameObject.SetActive(true);

            /* save position of the pointer and freeze rotation for translation later */
            lastLeftTipProj = ProjectXZ(leftFinger.position);
            canTranslate = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        /* it's a right pointer */
        else if (other.name.Contains("RightIndex") || other.name.Contains("R_IndexDistal"))
        {
            rightFinger = other.transform;

            if (!rightPush)
                CreatePushCollider(rightFinger, false);

            if (!rotating)
                rightPush.gameObject.SetActive(true);

            lastRightTipProj = ProjectXZ(rightFinger.position);
            canTranslate = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    /* bye bye fingers */
    void OnTriggerExit(Collider other)
    {
        if (other.transform == leftFinger)
        {
            leftFinger = null;

            /* again, old logic: not using push colliders/physics now */
            if (leftPush)
                leftPush.gameObject.SetActive(false);
        }
        else if (other.transform == rightFinger)
        {
            rightFinger = null;
            if (rightPush)
                rightPush.gameObject.SetActive(false);
        }

        /* where did my fingers go? */        
        if (!leftFinger && !rightFinger)
        {
            rotating = false;
            canTranslate = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    /* the main logic */
    void FixedUpdate()
    {
        bool rightFingerIsDown = false;
        bool leftFingerIsDown = false;
        
        /* case: setting up for rotation with right finger */
        if (rightFinger && !rotating)
        {
            /* check if the finger is pointed downward */
            Transform indexTip = rightFinger.transform;
            Vector3 fingerDirection = indexTip.forward;
            float angle = Vector3.Dot(fingerDirection, Vector3.down);
            rightFingerIsDown = angle > 0.8f;
            if (rightFingerIsDown)
            {
                rightPush.gameObject.SetActive(false);
                canTranslate = false;
                activeFinger = rightFinger;
                rotating = true;
                rb.constraints = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;
            }
        }

        /* case: setting up for rotation with left finger */
        if (leftFinger && !rotating)
        {
            Transform indexTip = leftFinger.transform;
            Vector3 fingerDirection = indexTip.forward;
            float angle = Vector3.Dot(fingerDirection, Vector3.down);
            leftFingerIsDown = angle > 0.8f;
            if (leftFingerIsDown)
            {
                leftPush.gameObject.SetActive(false);
                canTranslate = false;
                rotating = true;
                activeFinger = leftFinger;
                rb.constraints = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;
            }
        }        

        /* case: rotating */
        if (leftFinger && rightFinger && rotating)
        {
            float deltaAngle = autoRotateSpeed * Time.fixedDeltaTime;
            if (activeFinger == rightFinger) deltaAngle = -deltaAngle;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, deltaAngle, 0));
            return;
        }

        /* case: translating */
        if (canTranslate && !rotating && (leftFinger || rightFinger))
        {
            Transform finger = rightFinger;
            Vector3 lastTipProj = lastRightTipProj;
            if (leftFinger)
            {
                finger = leftFinger;
                lastTipProj = lastLeftTipProj;
            }
            else if (rightFinger)
            {
                finger = rightFinger;
                lastTipProj = lastRightTipProj;
            }

            Vector3 currentTipProj = ProjectXZ(finger.position);
            Vector3 delta = currentTipProj - lastTipProj;

            if (leftFinger) lastLeftTipProj = currentTipProj;
            if (rightFinger) lastRightTipProj = currentTipProj;

            if (delta.sqrMagnitude > 0.0001f)
            {
                Vector3 targetVelocity = delta / Time.fixedDeltaTime;

                Vector3 force = (targetVelocity - rb.velocity) * 5f;
                rb.AddForce(force, ForceMode.Force);
            }
        }
    }


    /* put colliders on les doigts - that's fingers for you insophisticates */
    void CreateInitialColliders()
    {
        var xrOrigin = GameObject.Find("XR Origin Hands (XR Rig)");
        if (xrOrigin == null)
            return;

        var leftHand = xrOrigin.transform.Find("Camera Offset/Left Hand");
        var rightHand = xrOrigin.transform.Find("Camera Offset/Right Hand");

        if (leftHand != null)
        {
            var leftVisual = leftHand.Find("Left Hand Interaction Visual");
            if (leftVisual != null)
            {
                var leftIndexTip =
                 leftVisual.Find("L_Wrist/L_IndexMetacarpal/L_IndexProximal/L_IndexIntermediate/L_IndexDistal");
                if (leftIndexTip != null)
                {
                    var tipCollider = leftIndexTip.GetComponent<SphereCollider>();
                    if (tipCollider == null)
                    {
                        tipCollider = leftIndexTip.gameObject.AddComponent<SphereCollider>();
                        tipCollider.radius = 0.01f;
                        tipCollider.isTrigger = true;
                        tipCollider.gameObject.tag = "IndexFingerCollider";
                    }
                }
            }
        }
        if (rightHand != null)
        {
            var rightVisual = rightHand.Find("Right Hand Interaction Visual");
            if (rightVisual != null)
            {
                var rightIndexTip =
                 rightVisual.Find("R_Wrist/R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal");
                if (rightIndexTip != null)
                {
                    var tipCollider = rightIndexTip.GetComponent<SphereCollider>();
                    if (tipCollider == null)
                    {
                        tipCollider = rightIndexTip.gameObject.AddComponent<SphereCollider>();
                        tipCollider.radius = 0.01f; // 1cm radius
                        tipCollider.isTrigger = true;
                        tipCollider.gameObject.tag = "IndexFingerCollider";
                    }
                }
            }
        }
    }

    /* helper - thanks for the help */
    Vector3 ProjectXZ(Vector3 w) => new Vector3(w.x, rb.position.y, w.z);

}
