
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    
    public float moveSpeed = 2.0f;
    
    public float forwardThreshold = 25.0f;
    
    public Transform leftFingerTip;
    
    private Camera mainCamera;
    
    
    private void Start()
    {
        mainCamera = Camera.main;
        leftFingerTip = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Left Hand/Left Hand Interaction Visual/L_Wrist/L_IndexMetacarpal/L_IndexProximal/L_IndexIntermediate/L_IndexDistal/L_IndexTip").transform;
            
    }
    
    private void Update()
    {
       if (leftFingerTip == null || mainCamera == null)
            return;
        
        Vector3 fingerDirection = leftFingerTip.forward;
        Vector3 headForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        
        Vector3 fingerDirectionHorizontal = Vector3.ProjectOnPlane(fingerDirection, Vector3.up).normalized;
        float forwardAngle = Vector3.Angle(fingerDirectionHorizontal, headForward);

        float verticalAngle = Vector3.Angle(fingerDirection, Vector3.up);
        
        bool isVertical = verticalAngle < 45.0f || verticalAngle > 135.0f;

        /* don't move if the finger is pointing up or down */
        if (isVertical) return;
        
        /* only if pointing forward */
        bool isPointingForward = forwardAngle < forwardThreshold;
        if (isPointingForward)
            transform.position += headForward * moveSpeed * Time.deltaTime;

        }
}