using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class SimplePerspectiveSwitcher : MonoBehaviour
{
  [SerializeField] private Transform mainCamera;
  [SerializeField] private Transform tableCenter;
  [SerializeField] private float transitionSpeed = 5f;
  [SerializeField] private bool smoothTransition = true;
  [SerializeField] private PerspectiveUI uiComponent;

  [Header("Input Actions")]
  [SerializeField] private InputAction player1Action;
  [SerializeField] private InputAction player2Action;
  [SerializeField] private InputAction player3Action;
  [SerializeField] private InputAction player4Action;
  [SerializeField] private InputAction previousPerspectiveAction;
  [SerializeField] private InputAction nextPerspectiveAction;

  [Header("XR Settings")]
  [SerializeField] private bool isXRMode = true;
  [SerializeField] private Transform xrRig; // The parent object that contains the camera in XR

  private Transform[] playerMarkers = new Transform[4];
  private int currentPlayerIndex = 0;
  private bool isTransitioning = false;

  private void OnEnable()
  {
    // Enable all input actions
    player1Action.Enable();
    player2Action.Enable();
    player3Action.Enable();
    player4Action.Enable();
    previousPerspectiveAction.Enable();
    nextPerspectiveAction.Enable();

    // Add callbacks
    player1Action.performed += _ => { if (playerMarkers[0] != null) SwitchToPlayer(0); };
    player2Action.performed += _ => { if (playerMarkers[1] != null) SwitchToPlayer(1); };
    player3Action.performed += _ => { if (playerMarkers[2] != null) SwitchToPlayer(2); };
    player4Action.performed += _ => { if (playerMarkers[3] != null) SwitchToPlayer(3); };
    previousPerspectiveAction.performed += _ => SwitchToPreviousPlayer();
    nextPerspectiveAction.performed += _ => SwitchToNextPlayer();
  }

  private void OnDisable()
  {
    // Disable all input actions
    player1Action.Disable();
    player2Action.Disable();
    player3Action.Disable();
    player4Action.Disable();
    previousPerspectiveAction.Disable();
    nextPerspectiveAction.Disable();

    // Remove callbacks
    player1Action.performed -= _ => { if (playerMarkers[0] != null) SwitchToPlayer(0); };
    player2Action.performed -= _ => { if (playerMarkers[1] != null) SwitchToPlayer(1); };
    player3Action.performed -= _ => { if (playerMarkers[2] != null) SwitchToPlayer(2); };
    player4Action.performed -= _ => { if (playerMarkers[3] != null) SwitchToPlayer(3); };
    previousPerspectiveAction.performed -= _ => SwitchToPreviousPlayer();
    nextPerspectiveAction.performed -= _ => SwitchToNextPlayer();
  }

  private void Start()
  {
    // Find main camera if not set
    if (mainCamera == null)
    {
      mainCamera = Camera.main.transform;
      Debug.Log($"Using main camera: {(mainCamera ? mainCamera.name : "NONE FOUND")}");
    }

    // Check if in XR mode - find XR Rig
    if (isXRMode && mainCamera != null)
    {
      if (xrRig == null)
      {
        // Try to find XR rig (typically the parent of the camera or parent's parent)
        Transform current = mainCamera.parent;
        if (current != null)
        {
          xrRig = current;
          Debug.Log($"Auto-detected XR Rig: {xrRig.name}");
        }
        else
        {
          Debug.LogWarning("XR Mode is enabled but couldn't find an XR Rig - camera movements may not work");
        }
      }

      Debug.Log($"Camera parent hierarchy: {GetTransformPath(mainCamera)}");
    }

    // Find player markers
    FindPlayerMarkers();

    // Find UI component if not set
    if (uiComponent == null)
      uiComponent = FindObjectOfType<PerspectiveUI>();

    // Set initial position
    if (playerMarkers[0] != null)
    {
      SetCameraToPlayerPosition(0, false);
      UpdateUI();
    }

    // Set up default input actions if not already set
    SetupDefaultInputActions();
  }

  private void SetupDefaultInputActions()
  {
    // Only create default actions if they haven't been assigned
    if (player1Action == null)
    {
      player1Action = new InputAction(name: "Player1", InputActionType.Button);
      player1Action.AddBinding("<Keyboard>/1");
      player1Action.Enable();
    }

    if (player2Action == null)
    {
      player2Action = new InputAction(name: "Player2", InputActionType.Button);
      player2Action.AddBinding("<Keyboard>/2");
      player2Action.Enable();
    }

    if (player3Action == null)
    {
      player3Action = new InputAction(name: "Player3", InputActionType.Button);
      player3Action.AddBinding("<Keyboard>/3");
      player3Action.Enable();
    }

    if (player4Action == null)
    {
      player4Action = new InputAction(name: "Player4", InputActionType.Button);
      player4Action.AddBinding("<Keyboard>/4");
      player4Action.Enable();
    }

    if (previousPerspectiveAction == null)
    {
      previousPerspectiveAction = new InputAction(name: "PreviousPerspective", InputActionType.Button);
      previousPerspectiveAction.AddBinding("<Keyboard>/q");
      previousPerspectiveAction.Enable();
    }

    if (nextPerspectiveAction == null)
    {
      nextPerspectiveAction = new InputAction(name: "NextPerspective", InputActionType.Button);
      nextPerspectiveAction.AddBinding("<Keyboard>/e");
      nextPerspectiveAction.Enable();
    }
  }

  private void FindPlayerMarkers()
  {
    GameObject p1Marker = GameObject.Find("StartMarkerP1");
    GameObject p2Marker = GameObject.Find("StartMarkerP2");
    GameObject p3Marker = GameObject.Find("StartMarkerP3");
    GameObject p4Marker = GameObject.Find("StartMarkerP4");

    if (p1Marker != null) playerMarkers[0] = p1Marker.transform;
    if (p2Marker != null) playerMarkers[1] = p2Marker.transform;
    if (p3Marker != null) playerMarkers[2] = p3Marker.transform;
    if (p4Marker != null) playerMarkers[3] = p4Marker.transform;

    int foundMarkers = 0;
    for (int i = 0; i < playerMarkers.Length; i++)
    {
      if (playerMarkers[i] != null)
      {
        foundMarkers++;
        Debug.Log($"Found Player {i + 1} marker at position: {playerMarkers[i].position}");
      }
    }

    Debug.Log($"Found {foundMarkers} player markers");
  }

  public void SwitchToPlayer(int playerIndex)
  {
    if (playerIndex < 0 || playerIndex >= playerMarkers.Length || playerMarkers[playerIndex] == null)
    {
      Debug.LogWarning($"Cannot switch to Player {playerIndex + 1}: Invalid index or marker is null");
      return;
    }

    if (currentPlayerIndex == playerIndex)
    {
      Debug.Log($"Already at Player {playerIndex + 1} perspective");
      return;
    }

    Debug.Log($"Switching from Player {currentPlayerIndex + 1} to Player {playerIndex + 1}");
    currentPlayerIndex = playerIndex;

    if (smoothTransition)
    {
      StartCoroutine(TransitionToPlayer(playerIndex));
    }
    else
    {
      SetCameraToPlayerPosition(playerIndex, false);
      UpdateUI();
    }
  }

  private void SetCameraToPlayerPosition(int playerIndex, bool keepHeight)
  {
    if (playerMarkers[playerIndex] == null)
    {
      Debug.LogError($"Cannot move camera to Player {playerIndex + 1} position: marker is null");
      return;
    }

    Vector3 targetPosition = playerMarkers[playerIndex].position;
    Quaternion targetRotation = playerMarkers[playerIndex].rotation;

    Debug.Log($"Moving camera to Player {playerIndex + 1} position: {targetPosition}, rotation: {targetRotation.eulerAngles}");

    // Optionally keep camera at same height
    if (keepHeight)
    {
      targetPosition.y = mainCamera.position.y;
    }

    Vector3 oldPosition = mainCamera.position;
    Quaternion oldRotation = mainCamera.rotation;

    if (isXRMode && xrRig != null)
    {
      // In XR, we move the XR Rig, not the camera directly
      Vector3 rigOldPosition = xrRig.position;
      Quaternion rigOldRotation = xrRig.rotation;

      // Calculate the offset from the camera to the rig
      Vector3 cameraOffset = mainCamera.position - xrRig.position;

      // Move the rig to the target position, accounting for the camera offset
      xrRig.position = targetPosition - new Vector3(cameraOffset.x, 0, cameraOffset.z);
      xrRig.rotation = targetRotation;

      Debug.Log($"XR Rig moved from {rigOldPosition} to {xrRig.position}");
    }
    else
    {
      // Standard camera movement
      mainCamera.position = targetPosition;
      mainCamera.rotation = targetRotation;
    }

    Debug.Log($"Camera moved from {oldPosition} to {mainCamera.position}");
  }

  private IEnumerator TransitionToPlayer(int playerIndex)
  {
    isTransitioning = true;
    Debug.Log($"Starting transition to Player {playerIndex + 1}");

    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 targetPosition = playerMarkers[playerIndex].position;
    Quaternion targetRotation = playerMarkers[playerIndex].rotation;

    if (isXRMode && xrRig != null)
    {
      // For XR mode, we track the rig position
      startPosition = xrRig.position;
      startRotation = xrRig.rotation;

      // Calculate the offset from the camera to the rig
      Vector3 cameraOffset = mainCamera.position - xrRig.position;

      // Adjust target position for the rig
      targetPosition = targetPosition - new Vector3(cameraOffset.x, 0, cameraOffset.z);
    }
    else
    {
      startPosition = mainCamera.position;
      startRotation = mainCamera.rotation;
    }

    float journeyLength = Vector3.Distance(startPosition, targetPosition);
    float startTime = Time.time;

    Debug.Log($"Transition distance: {journeyLength}, will take ~{journeyLength / transitionSpeed} seconds");

    while (Time.time - startTime < journeyLength / transitionSpeed)
    {
      float t = (Time.time - startTime) / (journeyLength / transitionSpeed);

      // Smooth interpolation
      t = Mathf.SmoothStep(0, 1, t);

      if (isXRMode && xrRig != null)
      {
        xrRig.position = Vector3.Lerp(startPosition, targetPosition, t);
        xrRig.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
      }
      else
      {
        mainCamera.position = Vector3.Lerp(startPosition, targetPosition, t);
        mainCamera.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
      }

      yield return null;
    }

    // Ensure we reach exact target position
    if (isXRMode && xrRig != null)
    {
      xrRig.position = targetPosition;
      xrRig.rotation = targetRotation;
      Debug.Log($"Transition complete. XR Rig at {xrRig.position}, rotation: {xrRig.rotation.eulerAngles}");
    }
    else
    {
      mainCamera.position = targetPosition;
      mainCamera.rotation = targetRotation;
      Debug.Log($"Transition complete. Camera at {mainCamera.position}, rotation: {mainCamera.rotation.eulerAngles}");
    }

    UpdateUI();
    isTransitioning = false;
  }

  private void UpdateUI()
  {
    if (uiComponent != null)
    {
      uiComponent.UpdatePerspectiveText(currentPlayerIndex);
    }
  }

  // Public methods for external calls (e.g. UI buttons)
  public void SwitchToPreviousPlayer()
  {
    if (isTransitioning) return;

    int prevIndex = (currentPlayerIndex - 1);
    if (prevIndex < 0) prevIndex = 3;

    // Find the previous valid marker
    int attempts = 0;
    while (playerMarkers[prevIndex] == null && attempts < 4)
    {
      prevIndex = (prevIndex - 1);
      if (prevIndex < 0) prevIndex = 3;
      attempts++;
    }

    if (playerMarkers[prevIndex] != null)
    {
      SwitchToPlayer(prevIndex);
    }
  }

  public void SwitchToNextPlayer()
  {
    if (isTransitioning) return;

    int nextIndex = (currentPlayerIndex + 1) % 4;

    // Find the next valid marker
    int attempts = 0;
    while (playerMarkers[nextIndex] == null && attempts < 4)
    {
      nextIndex = (nextIndex + 1) % 4;
      attempts++;
    }

    if (playerMarkers[nextIndex] != null)
    {
      SwitchToPlayer(nextIndex);
    }
  }

  // Get the current player index
  public int GetCurrentPlayerIndex()
  {
    return currentPlayerIndex;
  }

  // Helper method to get the full path of a transform for debugging
  private string GetTransformPath(Transform transform)
  {
    string path = transform.name;
    Transform current = transform.parent;

    while (current != null)
    {
      path = current.name + "/" + path;
      current = current.parent;
    }

    return path;
  }
}