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
  [SerializeField] private float wristRaiseThreshold = 0.3f; // How high the wrist needs to be raised (relative to camera height)
  [SerializeField] private float wristShowDelay = 0.5f; // How long to wait before showing UI after wrist is raised
  [SerializeField] private float wristHideDelay = 1.0f; // How long to wait before hiding UI after wrist is lowered

  private Canvas canvas;
  private Camera mainCamera;
  private RectTransform canvasRect;
  private Vector3 targetPosition;
  private Quaternion targetRotation;
  private Vector3 positionVelocity;
  private float rotationVelocity;
  private Transform wristTransform;
  private bool isWristRaised = false;
  private float wristRaiseTimer = 0f;
  private float wristLowerTimer = 0f;
  private bool isUIVisible = false;

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

      // Initially hide the UI
      SetUIVisibility(false);
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

    // Check if wrist is raised
    CheckWristRaised();

    // Update transforms and position
    UpdateTargetTransforms();
    SmoothlyUpdatePosition();
  }

  private void CheckWristRaised()
  {
    if (wristTransform == null || mainCamera == null) return;

    // Calculate how high the wrist is relative to camera eye level
    float wristHeight = wristTransform.position.y;
    float cameraHeight = mainCamera.transform.position.y;
    bool wristCurrentlyRaised = (wristHeight - cameraHeight) > wristRaiseThreshold;

    // Handle wrist raise detection with delay
    if (wristCurrentlyRaised && !isWristRaised)
    {
      wristRaiseTimer += Time.deltaTime;
      if (wristRaiseTimer >= wristShowDelay)
      {
        isWristRaised = true;
        wristRaiseTimer = 0f;
        SetUIVisibility(true);
      }
    }
    else if (!wristCurrentlyRaised && isWristRaised)
    {
      wristLowerTimer += Time.deltaTime;
      if (wristLowerTimer >= wristHideDelay)
      {
        isWristRaised = false;
        wristLowerTimer = 0f;
        SetUIVisibility(false);
      }
    }
    else
    {
      // Reset timers if state hasn't changed
      if (!wristCurrentlyRaised) wristRaiseTimer = 0f;
      if (wristCurrentlyRaised) wristLowerTimer = 0f;
    }
  }

  private void SetUIVisibility(bool visible)
  {
    if (isUIVisible == visible) return;

    isUIVisible = visible;

    // Enable/disable all child renderers and canvases
    foreach (var renderer in GetComponentsInChildren<Renderer>(true))
    {
      renderer.enabled = visible;
    }

    foreach (var graphic in GetComponentsInChildren<Graphic>(true))
    {
      graphic.enabled = visible;
    }

    // Make sure any child canvases update their visibility
    foreach (var childCanvas in GetComponentsInChildren<Canvas>(true))
    {
      if (childCanvas != canvas) // Don't disable the main canvas
      {
        childCanvas.enabled = visible;
      }
    }

    // Keep the canvas component enabled but make it not render
    canvas.enabled = true;
  }

  private void UpdateTargetTransforms()
  {
    if (wristTransform != null)
    {
      // Calculate position relative to the wrist
      Vector3 forwardDirection = wristTransform.forward;
      Vector3 rightDirection = wristTransform.right;

      // Calculate the target position
      targetPosition = wristTransform.position +
                    forwardDirection * forwardOffset +
                    rightDirection * rightOffset +
                    Vector3.up * heightOffset;

      // Calculate rotation to face the player
      Vector3 directionToCamera = mainCamera.transform.position - targetPosition;
      targetRotation = Quaternion.LookRotation(directionToCamera) * Quaternion.Euler(0, rotationOffset, 0);
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