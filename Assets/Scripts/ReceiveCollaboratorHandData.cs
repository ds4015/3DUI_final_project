using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReceiveCollaboratorHandData : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI debugText;
    public GameObject CollaboratorVirtualHandLeft;
    public GameObject CollaboratorVirtualHandRight;
    public GameObject map;
    [SerializeField] private Animator CollaboratorLeftHandAnimator = null;
    [SerializeField] private Animator CollaboratorRightHandAnimator = null;
    [SerializeField] private Transform CollaboratorPointTargetLeft;
    [SerializeField] private Transform CollaboratorPointTargetRight;
    [SerializeField] private ExperimentManager experimentManager;

    private int m_animLayerIndexThumb = -1;
    private int m_animLayerIndexPoint = -1;
    private int m_animParamIndexFlex = -1;
    private int m_animParamIndexPose = -1;
    private const string ANIM_LAYER_NAME_POINT = "Point Layer";
    private const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
    private const string ANIM_PARAM_NAME_FLEX = "Flex";
    private const string ANIM_PARAM_NAME_POSE = "Pose";

    private void Start()
    {
        // Get animator layer indices by name, for later use switching between hand visuals
        m_animLayerIndexPoint = CollaboratorLeftHandAnimator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
        m_animLayerIndexThumb = CollaboratorLeftHandAnimator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
        m_animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
        m_animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    /// <summary>
    /// 
    /// </summary>
    /// Scheme of data[]:
    ///    0  VECTOR3       leftHand.position,
    ///    1  QUATERNION    leftHand.rotation,
    ///    2  VECTOR3       rightHand.position,
    ///    3  QUATERNION    rightHand.rotation,
    ///    4  FLOAT         mapRotator.tableRotation,
    ///    5  FLOAT         leftHandAnimationController.FLEX,
    ///    6  FLOAT         leftHandAnimationController.POINT,
    ///    7  FLOAT         leftHandAnimationController.THUMBSUP,
    ///    8  FLOAT         leftHandAnimationController.PINCH,
    ///    9  FLOAT         rightHandAnimationController.FLEX,
    ///    10 FLOAT         rightHandAnimationController.POINT,
    ///    11 FLOAT         rightHandAnimationController.THUMBSUP,
    ///    12 FLOAT         rightHandAnimationController.PINCH
    ///    13 VECTOR3       pointTargetLeft.position,
    ///    14 VECTOR3       pointTargetRight.position
    ///    15 BOOL          leftGripPressed
    ///    16 BOOL          rightGripPressed
    ///
    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == SendViewFollowerHandData.SendViewFollowerHandDataEventCode)
        {
            if (experimentManager.HandPresenceMode == "Hands")
            {
                object[] data = (object[])photonEvent.CustomData;

                CollaboratorVirtualHandLeft.SetActive(true);
                CollaboratorVirtualHandRight.SetActive(true);
                CollaboratorPointTargetLeft.gameObject.SetActive(true);
                CollaboratorPointTargetRight.gameObject.SetActive(true);
                // Virtual hand alignment.
                CollaboratorVirtualHandLeft.transform.position = (Vector3)data[0];
                CollaboratorVirtualHandLeft.transform.rotation = (Quaternion)data[1];
                CollaboratorVirtualHandRight.transform.position = (Vector3)data[2];
                CollaboratorVirtualHandRight.transform.rotation = (Quaternion)data[3];
                CollaboratorVirtualHandLeft.transform.RotateAround(map.gameObject.transform.position, Vector3.up, -1 * (float)data[4] + map.transform.eulerAngles.y);
                CollaboratorVirtualHandRight.transform.RotateAround(map.gameObject.transform.position, Vector3.up, -1 * (float)data[4] + map.transform.eulerAngles.y);

                //HandPose grabPose = m_defaultGrabPose;
                //// Pose
                //HandPoseId handPoseId = grabPose.PoseId;
                //m_animator.SetInteger(m_animParamIndexPose, (int)handPoseId);
                // Synchronize animation states
                CollaboratorLeftHandAnimator.SetFloat(m_animParamIndexFlex, (float)data[5]);
                CollaboratorLeftHandAnimator.SetLayerWeight(m_animLayerIndexPoint, (float)data[6]);
                CollaboratorLeftHandAnimator.SetLayerWeight(m_animLayerIndexThumb, (float)data[7]);
                CollaboratorLeftHandAnimator.SetFloat("Pinch", (float)data[8]);
                CollaboratorRightHandAnimator.SetFloat(m_animParamIndexFlex, (float)data[9]);
                CollaboratorRightHandAnimator.SetLayerWeight(m_animLayerIndexPoint, (float)data[10]);
                CollaboratorRightHandAnimator.SetLayerWeight(m_animLayerIndexThumb, (float)data[11]);
                CollaboratorRightHandAnimator.SetFloat("Pinch", (float)data[12]);
                // Point target alignment.
                CollaboratorPointTargetLeft.position = (Vector3)data[13];
                CollaboratorPointTargetRight.position = (Vector3)data[14];
                CollaboratorPointTargetLeft.transform.RotateAround(map.gameObject.transform.position, Vector3.up, -1 * (float)data[4] + map.transform.eulerAngles.y);
                CollaboratorPointTargetRight.transform.RotateAround(map.gameObject.transform.position, Vector3.up, -1 * (float)data[4] + map.transform.eulerAngles.y);
                //DrawStraightLineFromVirtualControllerToTarget(CollaboratorVirtualHandLeft.transform, CollaboratorPointTargetLeft, (bool)data[15]);
                //DrawStraightLineFromVirtualControllerToTarget(CollaboratorVirtualHandRight.transform, CollaboratorPointTargetRight, (bool)data[16]);
                DrawStraightLineFromVirtualControllerToTarget(
                    SceneComponents.Instance.PlayerTwoObjects.LeftHandLineRenderer,
                    SceneComponents.Instance.PlayerTwoObjects.LeftHandIndexTip,
                    CollaboratorPointTargetLeft,
                    (bool)data[15]
                );
                DrawStraightLineFromVirtualControllerToTarget(
                    SceneComponents.Instance.PlayerTwoObjects.RightHandLineRenderer,
                    SceneComponents.Instance.PlayerTwoObjects.RightHandIndexTip,
                    CollaboratorPointTargetRight,
                    (bool)data[16]
                );
            }
            else
            {
                CollaboratorVirtualHandLeft.SetActive(false);
                CollaboratorVirtualHandRight.SetActive(false);
                CollaboratorPointTargetLeft.gameObject.SetActive(false);
                CollaboratorPointTargetRight.gameObject.SetActive(false);
            }
        }
    }

    private void DrawStraightLineFromVirtualControllerToTarget(
        LineRenderer lineRenderer,
        Transform virtualPointOrigin,
        Transform virtualPointTarget,
        bool enabled)
    {
        lineRenderer.enabled = enabled;
        // Set line positions to 2. (Bezier curve has 50 line positions).
        lineRenderer.positionCount = 2;
        // Set line start position to virtual hand
        lineRenderer.SetPosition(0, virtualPointOrigin.position);
        // Set line end position to virtual point target
        lineRenderer.SetPosition(1, virtualPointTarget.position);
    }

}
