using UnityEngine;
using UnityEngine.UI;

public class UISetup : MonoBehaviour
{
  [Header("UI Positioning")]
  [SerializeField] private float forwardOffset = 0.5f; // Distance forward from camera
  [SerializeField] private float rightOffset = 0.3f; // Distance to the right
  [SerializeField] private float heightOffset = -0.1f; // Height offset from camera center
  [SerializeField] private Vector3 uiScale = new Vector3(0.001f, 0.001f, 0.001f); // Scale for world space UI
  [SerializeField] private float rotationOffset = 15f; // Angle to rotate towards player

  [Header("Smooth Movement")]
  [SerializeField] private float positionSmoothTime = 0.15f; // Time to smooth position movement
  [SerializeField] private float rotationSmoothTime = 0.1f; // Time to smooth rotation

  private Canvas canvas;
  private Camera mainCamera;
  private RectTransform canvasRect;
  private Vector3 targetPosition;
  private Quaternion targetRotation;
  private Vector3 positionVelocity;
  private float rotationVelocity;

  private void Start()
  {
    canvas = GetComponent<Canvas>();
    mainCamera = Camera.main;
    canvasRect = GetComponent<RectTransform>();

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

  private void LateUpdate()
  {
    UpdateTargetTransforms();
    SmoothlyUpdatePosition();
  }

  private void UpdateTargetTransforms()
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