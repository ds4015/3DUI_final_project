using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectRotate : MonoBehaviour
{

  GrabPushRotate gpr;

    void Awake()
    {
        gpr = GetComponentInParent<GrabPushRotate>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("LeftIndex") || other.name.Contains("L_IndexDistal"))
        {
            gpr.leftFinger = other.transform;

            gpr.lastLeftTipProj = gpr.ProjectXZ(gpr.leftFinger.position);
            gpr.canTranslate = true;
            gpr.rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else if (other.name.Contains("RightIndex") || other.name.Contains("R_IndexDistal"))
        {
            gpr.rightFinger = other.transform;

            gpr.lastRightTipProj = gpr.ProjectXZ(gpr.rightFinger.position);
            gpr.canTranslate = true;
            gpr.rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform == gpr.leftFinger)
        {
            gpr.leftFinger = null;
        }
        else if (other.transform == gpr.rightFinger)
        {
            gpr.rightFinger = null;
        }

    }

}
