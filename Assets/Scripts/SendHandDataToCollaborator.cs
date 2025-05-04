using ExitGames.Client.Photon;
using OVRTouchSample;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class SendHandDataToCollaborator : MonoBehaviour
{

    public Transform leftHand;
    public Transform rightHand;
    public MapRotator mapRotator;
    public float tableRotation;
    public HandLeader leftHandAnimationController;
    public HandLeader rightHandAnimationController;
    public Transform pointTargetLeft;
    public Transform pointTargetRight;

    public Vector3 hitPosition;
    private Vector3 hitNormal;
    private int lineInt;
    private bool hitValid;
    private Ray ray;

    public bool leftGripPressed;
    public bool rightGripPressed;

    public const byte SendHandDataEventCode = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Initialization.Instance.Initialized)
        {
            leftGripPressed = InputComponents.Instance.LeftHandActions.Select.ReadValue<float>() > 0.05f;
            if (leftGripPressed)
            {
                SceneComponents.Instance.PlayerOneObjects.LeftRayInteractor.enabled = true;
                leftRaycast();
            }
            else
            {
                SceneComponents.Instance.PlayerOneObjects.LeftRayInteractor.enabled = false;
            }

            rightGripPressed = InputComponents.Instance.RightHandActions.Select.ReadValue<float>() > 0.05f;
            if (rightGripPressed)
            {
                SceneComponents.Instance.PlayerOneObjects.RightRayInteractor.enabled = true;
                rightRaycast();
            }
            else
            {
                SceneComponents.Instance.PlayerOneObjects.RightRayInteractor.enabled = false;
            }

            SendHandDataToCollaboratorEvent();
        }
    }

    private bool leftRaycast()
    {
        SceneComponents.Instance.PlayerOneObjects.LeftRayInteractor.TryGetHitInfo(out hitPosition, out hitNormal, out lineInt, out hitValid);

        if (!hitValid)
        {
            SceneComponents.Instance.PlayerOneObjects.LeftHandPointTarget.position = SceneComponents.Instance.PlayerOneObjects.LeftHandIndexTip.transform.position;
            return false;
        }

        SceneComponents.Instance.PlayerOneObjects.LeftHandPointTarget.position = hitPosition;
        return true;
    }

    private bool rightRaycast()
    {
        SceneComponents.Instance.PlayerOneObjects.RightRayInteractor.TryGetHitInfo(out hitPosition, out hitNormal, out lineInt, out hitValid);

        if (!hitValid)
        {
            SceneComponents.Instance.PlayerOneObjects.RightHandPointTarget.position = SceneComponents.Instance.PlayerOneObjects.RightHandIndexTip.transform.position;
            return false;
        }

        SceneComponents.Instance.PlayerOneObjects.RightHandPointTarget.position = hitPosition;
        return true;
    }

    public void SendHandDataToCollaboratorEvent()
    {
        object[] content = new object[] {
            leftHand.position,
            leftHand.rotation,
            rightHand.position,
            rightHand.rotation,
            mapRotator.transform.eulerAngles.y,
            leftHandAnimationController.FLEX,
            leftHandAnimationController.POINT,
            leftHandAnimationController.THUMBSUP,
            leftHandAnimationController.PINCH,
            rightHandAnimationController.FLEX,
            rightHandAnimationController.POINT,
            rightHandAnimationController.THUMBSUP,
            rightHandAnimationController.PINCH,
            SceneComponents.Instance.PlayerOneObjects.LeftHandPointTarget.position,
            SceneComponents.Instance.PlayerOneObjects.RightHandPointTarget.position,
            leftGripPressed,
            rightGripPressed,
            SceneComponents.Instance.PlayerOneObjects.LeftHandIndexTip.position,
            SceneComponents.Instance.PlayerOneObjects.RightHandIndexTip.position
        };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(SendHandDataEventCode, content, raiseEventOptions, SendOptions.SendUnreliable);
    }
}
