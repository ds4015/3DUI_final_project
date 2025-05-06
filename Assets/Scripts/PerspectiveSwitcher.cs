using UnityEngine;

public class PerspectiveSwitcher : MonoBehaviour
{
  [Tooltip("Parent object containing all table objects to rotate")]
  public Transform tableObjectsParent;

  [Tooltip("Center point to rotate around (usually the center of the table)")]
  public Transform rotationCenter;

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
  }

  /// <summary>
  /// Reset the table to its original rotation
  /// </summary>
  public void ResetToOriginalView()
  {
    tableObjectsParent.rotation = originalRotation;
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
}