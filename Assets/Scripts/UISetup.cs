using UnityEngine;
using UnityEngine.UI;

public class UISetup : MonoBehaviour
{
  [Header("UI Positioning")]
  [SerializeField] private float forwardOffset = 0.5f; // Distance forward from wrist
  [SerializeField] private float rightOffset = 0.3f; // Distance to the right
  [SerializeField] private float heightOffset = -0.1f; // Height offset from wrist
  [SerializeField] private Vector3 uiScale = new Vector3(0.001f, 0.001f, 0.001f); // Scale for world space UI
  [SerializeField] private float rotationOffset = 15f; // Angle to rotate towards player

  [Header("Smooth Movement")]
  [SerializeField] private float positionSmoothTime = 0.15f; // Time to smooth position movement
  [SerializeField] private float rotationSmoothTime = 0.1f; // Time to smooth rotation

  [Header("Wrist Attachment")]
  [SerializeField] private bool attachToRightWrist = true; // Whether to attach to right or left wrist

  private Canvas canvas;
  private Camera mainCamera;
  private RectTransform canvasRect;
  private Vector3 targetPosition;
  private Quaternion targetRotation;
  private Vector3 positionVelocity;
  private float rotationVelocity;
  private Transform wristTransform;

  private void Start()
  {
    canvas = GetComponent<Canvas>();
    mainCamera = Camera.main;
    canvasRect = GetComponent<RectTransform>();

    // Find the appropriate wrist transform
    FindWristTransform();

    if (canvas != null)
    {
      // Set up canvas for world space
      canvas.renderMode = RenderMode.WorldSpace;

      // Set the world space scale
      transform.localScale = uiScale;

      // Initialize position and rotation
      UpdateTargetTransforms();
      transform.position = targetPosition;
      transform.rotation = targetRotation;
    }
  }

  private void FindWristTransform()
  {
    string wristPath = attachToRightWrist ?
      "XR Origin Hands (XR Rig)/Camera Offset/Right Hand/Right Hand Interaction Visual/R_Wrist" :
      "XR Origin Hands (XR Rig)/Camera Offset/Left Hand/Left Hand Interaction Visual/L_Wrist";

    wristTransform = GameObject.Find(wristPath)?.transform;

    if (wristTransform == null)
    {
      Debug.LogWarning($"Could not find wrist transform at path: {wristPath}. UI will follow camera instead.");
    }
  }

  private void LateUpdate()
  {
    // Check if wrist is available, if not, try to find it again
    if (wristTransform == null)
    {
      FindWristTransform();

      // If still null, fallback to camera-based positioning
      if (wristTransform == null && mainCamera != null)
      {
        UpdateCameraBasedTargetTransforms();
        SmoothlyUpdatePosition();
        return;
      }
    }

    // Update transforms and position
    UpdateTargetTransforms();
    SmoothlyUpdatePosition();
  }

  private void UpdateTargetTransforms()
  {
    if (wristTransform != null)
    {
      // Calculate position directly on top of the wrist
      Vector3 wristUpDirection = wristTransform.up;
      Vector3 directionToCamera = (mainCamera.transform.position - wristTransform.position).normalized;

      // Project the direction to camera onto the plane defined by the wrist's up direction
      // to get a forward vector that points towards the player but stays level with the wrist
      Vector3 forwardDirection = Vector3.ProjectOnPlane(directionToCamera, wristUpDirection).normalized;

      // Use the cross product to get a right vector perpendicular to both up and forward
      Vector3 rightDirection = Vector3.Cross(wristUpDirection, forwardDirection).normalized;
      if (!attachToRightWrist)
      {
        // Flip right direction for left wrist
        rightDirection = -rightDirection;
      }

      // Calculate the target position - directly on top of the wrist
      targetPosition = wristTransform.position +
                   wristUpDirection * heightOffset +  // Height above wrist
                   forwardDirection * forwardOffset + // Slight forward adjustment
                   rightDirection * rightOffset;      // Slight right/left adjustment

      // Make UI face the player by looking at the camera, but keeping aligned with wrist orientation
      Vector3 lookDirection = mainCamera.transform.position - targetPosition;
      targetRotation = Quaternion.LookRotation(lookDirection, wristUpDirection);

      // Apply rotation offset if needed
      if (rotationOffset != 0)
      {
        // Create a rotation around the up axis of the UI
        Quaternion additionalRotation = Quaternion.AngleAxis(rotationOffset, wristUpDirection);
        targetRotation = additionalRotation * targetRotation;
      }
    }
    else
    {
      UpdateCameraBasedTargetTransforms();
    }
  }

  private void UpdateCameraBasedTargetTransforms()
  {
    if (mainCamera != null && canvas != null)
    {
      Transform cameraTransform = mainCamera.transform;

      // Calculate position offset from camera
      Vector3 rightDirection = cameraTransform.right;
      Vector3 forwardDirection = cameraTransform.forward;

      // Calculate the target position
      targetPosition = cameraTransform.position +
                     forwardDirection * forwardOffset +
                     rightDirection * rightOffset +
                     Vector3.up * heightOffset;

      // Calculate rotation to face the player but maintain vertical orientation
      Vector3 lookDirection = targetPosition - cameraTransform.position;
      lookDirection.y = 0; // Keep UI vertical

      // Determine target rotation with offset
      targetRotation = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0, rotationOffset, 0);
    }
  }

  private void SmoothlyUpdatePosition()
  {
    // Smoothly interpolate position
    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, positionSmoothTime);

    // Smoothly interpolate rotation (using slerp with a damp-like behavior)
    transform.rotation = SmoothDampQuaternion(transform.rotation, targetRotation, ref rotationVelocity, rotationSmoothTime);
  }

  // Helper method to smooth quaternion rotation similar to SmoothDamp
  private Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref float velocityRef, float smoothTime)
  {
    float delta = Quaternion.Angle(current, target);

    if (delta > 0f)
    {
      float t = Mathf.SmoothDampAngle(0f, delta, ref velocityRef, smoothTime);
      t = 1.0f - (t / delta);
      return Quaternion.Slerp(current, target, t);
    }

    return current;
  }
}