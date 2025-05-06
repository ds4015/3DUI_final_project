using UnityEngine;

public class PerspectiveSwitcher : MonoBehaviour
{
  [Tooltip("Parent object containing all table objects to rotate")]
  public Transform tableObjectsParent;

  [Tooltip("Center point to rotate around (usually the center of the table)")]
  public Transform rotationCenter;

  [Tooltip("Button that opens the perspective selection panel")]
  public GameObject perspectiveSwitchButton;

  [Tooltip("Reset button that appears after changing perspective")]
  public GameObject resetButton;

  [System.Serializable]
  public class PlayerPosition
  {
    public string playerName;
    public Transform playerMarker;
    [Tooltip("Rotation in degrees needed to view from this player's perspective")]
    public float rotationAngle;
  }

  [Tooltip("Define all player positions and their corresponding rotation angles")]
  public PlayerPosition[] playerPositions;

  // Current active perspective (index in the playerPositions array)
  private int currentPerspective = 0;

  // Store original rotation
  private Quaternion originalRotation;

  // Track if we're in a different perspective than original
  private bool isInOriginalPerspective = true;

  void Start()
  {
    if (tableObjectsParent == null)
    {
      Debug.LogError("PerspectiveSwitcher: Table objects parent not assigned!");
      return;
    }

    if (rotationCenter == null)
    {
      Debug.LogWarning("PerspectiveSwitcher: Rotation center not assigned, using table parent position as center");
      rotationCenter = tableObjectsParent;
    }

    // Store the original rotation
    originalRotation = tableObjectsParent.rotation;

    // Set initial button states
    UpdateButtonVisibility();
  }

  /// <summary>
  /// Switch to a specific player's perspective
  /// </summary>
  /// <param name="playerIndex">Index of the player in the playerPositions array</param>
  public void SwitchToPerspective(int playerIndex)
  {
    if (playerIndex < 0 || playerIndex >= playerPositions.Length)
    {
      Debug.LogError($"PerspectiveSwitcher: Invalid player index: {playerIndex}");
      return;
    }

    currentPerspective = playerIndex;
    RotateTableToCurrentPerspective();

    // We're no longer in original perspective
    isInOriginalPerspective = false;
    UpdateButtonVisibility();
  }

  /// <summary>
  /// Reset the table to its original rotation
  /// </summary>
  public void ResetToOriginalView()
  {
    tableObjectsParent.rotation = originalRotation;
    isInOriginalPerspective = true;
    UpdateButtonVisibility();
  }

  /// <summary>
  /// Rotate the table to match the current perspective
  /// </summary>
  private void RotateTableToCurrentPerspective()
  {
    PlayerPosition targetPosition = playerPositions[currentPerspective];

    // Calculate the rotation needed
    Quaternion targetRotation = Quaternion.Euler(0, targetPosition.rotationAngle, 0);

    // Apply rotation around the center point
    Vector3 centerPoint = rotationCenter.position;
    tableObjectsParent.RotateAround(centerPoint, Vector3.up, targetPosition.rotationAngle);
  }

  /// <summary>
  /// Update the visibility of perspective switch and reset buttons
  /// </summary>
  private void UpdateButtonVisibility()
  {
    if (perspectiveSwitchButton != null)
    {
      perspectiveSwitchButton.SetActive(isInOriginalPerspective);
    }

    if (resetButton != null)
    {
      resetButton.SetActive(!isInOriginalPerspective);
    }
  }
}