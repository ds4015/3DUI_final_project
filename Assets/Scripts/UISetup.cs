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

  private Canvas canvas;
  private Camera mainCamera;
  private RectTransform canvasRect;

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

      // Set initial position
      UpdateCanvasPosition();
    }
  }

  private void LateUpdate()
  {
    UpdateCanvasPosition();
  }

  private void UpdateCanvasPosition()
  {
    if (mainCamera != null && canvas != null)
    {
      Transform cameraTransform = mainCamera.transform;

      // Calculate position offset from camera
      Vector3 rightDirection = cameraTransform.right;
      Vector3 forwardDirection = cameraTransform.forward;

      // Calculate the target position
      Vector3 targetPosition = cameraTransform.position +
                             forwardDirection * forwardOffset +
                             rightDirection * rightOffset +
                             Vector3.up * heightOffset;

      // Update position
      transform.position = targetPosition;

      // Calculate rotation to face the player but maintain vertical orientation
      Vector3 lookDirection = transform.position - cameraTransform.position;
      lookDirection.y = 0; // Keep UI vertical

      // Rotate towards player with offset
      Quaternion targetRotation = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0, rotationOffset, 0);
      transform.rotation = targetRotation;
    }
  }
}