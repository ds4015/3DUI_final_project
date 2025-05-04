using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class ARMenuPositioner : MonoBehaviour
{
  [Header("Positioning")]
  [SerializeField] private Transform cameraTransform;
  [SerializeField] private bool followCamera = true;
  [SerializeField] private Vector3 offsetFromCamera = new Vector3(0f, -0.1f, 0.5f);
  [SerializeField] private bool lookAtCamera = true;
  [SerializeField] private float smoothFollowSpeed = 5f;
  [SerializeField] private float minDistanceToUpdate = 0.05f;

  [Header("Boundaries")]
  [SerializeField] private float minDistance = 0.3f;
  [SerializeField] private float maxDistance = 0.8f;
  [SerializeField] private float viewportMargin = 0.1f; // Keep menu within this margin of viewport edges

  private Canvas canvas;
  private Vector3 desiredPosition;
  private Vector3 lastCameraPosition;
  private Quaternion lastCameraRotation;
  private float distanceThresholdSquared;

  void Start()
  {
    canvas = GetComponent<Canvas>();

    if (cameraTransform == null)
    {
      cameraTransform = Camera.main.transform;
    }

    // Initialize to camera position
    UpdateCanvasPosition(true);

    lastCameraPosition = cameraTransform.position;
    lastCameraRotation = cameraTransform.rotation;
    distanceThresholdSquared = minDistanceToUpdate * minDistanceToUpdate;
  }

  void LateUpdate()
  {
    if (!followCamera) return;

    // Only update position if camera has moved enough
    float moveDistanceSqr = (cameraTransform.position - lastCameraPosition).sqrMagnitude;
    float rotationDifference = Quaternion.Angle(cameraTransform.rotation, lastCameraRotation);

    if (moveDistanceSqr > distanceThresholdSquared || rotationDifference > 5f)
    {
      UpdateCanvasPosition(false);
      lastCameraPosition = cameraTransform.position;
      lastCameraRotation = cameraTransform.rotation;
    }
  }

  public void UpdateCanvasPosition(bool immediate)
  {
    // Determine target position based on camera
    Vector3 targetPosition = cameraTransform.position + cameraTransform.rotation * offsetFromCamera;

    // Ensure minimum/maximum distance
    Vector3 directionToTarget = targetPosition - cameraTransform.position;
    float distanceToTarget = directionToTarget.magnitude;

    if (distanceToTarget < minDistance)
    {
      targetPosition = cameraTransform.position + directionToTarget.normalized * minDistance;
    }
    else if (distanceToTarget > maxDistance)
    {
      targetPosition = cameraTransform.position + directionToTarget.normalized * maxDistance;
    }

    // Keep in viewport bounds
    Camera cam = cameraTransform.GetComponent<Camera>();
    if (cam != null)
    {
      Vector3 viewportPosition = cam.WorldToViewportPoint(targetPosition);
      viewportPosition.x = Mathf.Clamp(viewportPosition.x, viewportMargin, 1 - viewportMargin);
      viewportPosition.y = Mathf.Clamp(viewportPosition.y, viewportMargin, 1 - viewportMargin);

      // Convert back to world space if bounds were changed
      if (viewportPosition.x != cam.WorldToViewportPoint(targetPosition).x ||
          viewportPosition.y != cam.WorldToViewportPoint(targetPosition).y)
      {
        // Need to make sure z is preserved correctly for WorldToViewportPoint
        viewportPosition.z = Vector3.Distance(cam.transform.position, targetPosition);
        targetPosition = cam.ViewportToWorldPoint(viewportPosition);
      }
    }

    desiredPosition = targetPosition;

    // Apply position immediately or smoothly
    if (immediate)
    {
      transform.position = desiredPosition;
    }
    else
    {
      transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothFollowSpeed * Time.deltaTime);
    }

    // Look at camera
    if (lookAtCamera)
    {
      transform.LookAt(2 * transform.position - cameraTransform.position);
    }
  }

  public void ResetPosition()
  {
    UpdateCanvasPosition(true);
  }

  // Call this method to manually position the menu in front of the camera
  public void PositionInFrontOfCamera()
  {
    transform.position = cameraTransform.position + cameraTransform.forward * offsetFromCamera.z;
    transform.LookAt(2 * transform.position - cameraTransform.position);
  }
}