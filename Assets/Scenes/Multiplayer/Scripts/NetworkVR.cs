using Fusion;
using UnityEngine;

public class NetworkedVR : NetworkBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Networked] private Vector3 HeadPosition { get; set; }
    [Networked] private Quaternion HeadRotation { get; set; }
    [Networked] private Vector3 LeftHandPosition { get; set; }
    [Networked] private Quaternion LeftHandRotation { get; set; }
    [Networked] private Vector3 RightHandPosition { get; set; }
    [Networked] private Quaternion RightHandRotation { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            HeadPosition = head.position;
            HeadRotation = head.rotation;
            LeftHandPosition = leftHand.position;
            LeftHandRotation = leftHand.rotation;
            RightHandPosition = rightHand.position;
            RightHandRotation = rightHand.rotation;
        }
        else
        {
            head.SetPositionAndRotation(HeadPosition, HeadRotation);
            leftHand.SetPositionAndRotation(LeftHandPosition, LeftHandRotation);
            rightHand.SetPositionAndRotation(RightHandPosition, RightHandRotation);
        }
    }
}
