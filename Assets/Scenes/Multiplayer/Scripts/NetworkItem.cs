using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.XR.CoreUtils;
using UnityEngine;

public class NetworkItem : NetworkBehaviour
{
    public Transform playArea;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    private HardwareRig hardwareRig;
    private XROrigin xrOrigin;
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            hardwareRig = FindObjectOfType<HardwareRig>();
            xrOrigin = hardwareRig.GetComponent<XROrigin>();
            if (hardwareRig == null)
            {
                Debug.LogError("No hardware Rig found ");
            }
            xrOrigin = hardwareRig.GetComponent<XROrigin>();
            xrOrigin.MoveCameraToWorldLocation(transform.position);
            xrOrigin.transform.rotation = transform.rotation;
            //hardwareRig.transform.position = transform.position;
            //hardwareRig.transform.rotation = transform.rotation;
        }

    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput<RigIn>(out var inp))
        {
            playArea.position = inp.playAreaPosition;
            playArea.rotation = inp.playAreaRotation;
            head.position = inp.headsetPosition;
            head.rotation = inp.headsetRotation;
            leftHand.position = inp.leftHandPosition;
            leftHand.rotation = inp.leftHandRotation;
            rightHand.position = inp.rightHandPosition;
            rightHand.rotation = inp.rightHandRotation;

        }
    }
    public override void Render()
    {
        if (Object.HasInputAuthority && hardwareRig != null)
        {
            playArea.position = hardwareRig.playArea.position;
            playArea.rotation = hardwareRig.playArea.rotation;
            head.position = hardwareRig.headset.position;
            head.rotation = hardwareRig.headset.rotation;
            leftHand.position = hardwareRig.leftHand.position;
            leftHand.rotation = hardwareRig.leftHand.rotation;
            rightHand.position = hardwareRig.rightHand.position;
            rightHand.rotation = hardwareRig.rightHand.rotation;
        }
    }
}
