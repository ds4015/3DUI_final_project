using UnityEngine;
using System.Collections.Generic;

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

  // Store original state
  private Quaternion originalParentRotation;
  private Dictionary<Transform, OriginalTransform> originalChildTransforms = new Dictionary<Transform, OriginalTransform>();

  // Track if we're in a different perspective than original
  private bool isInOriginalPerspective = true;

  // Class to store original transform data
  private class OriginalTransform
  {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localPosition;
    public Quaternion localRotation;

    public OriginalTransform(Transform t)
    {
      position = t.position;
      rotation = t.rotation;
      localPosition = t.localPosition;
      localRotation = t.localRotation;
    }
  }

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

    // Store the original rotations and positions
    StoreOriginalTransforms();

    // Set initial button states
    UpdateButtonVisibility();

    // Debug log to verify button references
    Debug.Log("PerspectiveSwitcher initialized. Switch button: " +
              (perspectiveSwitchButton != null ? perspectiveSwitchButton.name : "null") +
              ", Reset button: " + (resetButton != null ? resetButton.name : "null"));
  }

  /// <summary>
  /// Store the original transforms of all table objects
  /// </summary>
  private void StoreOriginalTransforms()
  {
    originalParentRotation = tableObjectsParent.rotation;
    originalChildTransforms.Clear();

    // Store transforms for all children recursively
    StoreChildrenTransforms(tableObjectsParent);

    Debug.Log("Stored original transforms for " + originalChildTransforms.Count + " objects");
  }

  /// <summary>
  /// Recursively store transforms for all children
  /// </summary>
  private void StoreChildrenTransforms(Transform parent)
  {
    foreach (Transform child in parent)
    {
      if (!originalChildTransforms.ContainsKey(child))
      {
        originalChildTransforms[child] = new OriginalTransform(child);
        StoreChildrenTransforms(child);
      }
    }
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

    // Debug log to verify button states after switching
    Debug.Log("Switched to perspective " + playerIndex +
              ". Switch button active: " + (perspectiveSwitchButton != null ? perspectiveSwitchButton.activeSelf.ToString() : "null") +
              ", Reset button active: " + (resetButton != null ? resetButton.activeSelf.ToString() : "null"));
  }

  /// <summary>
  /// Reset the table to its original rotation
  /// </summary>
  public void ResetToOriginalView()
  {
    // Restore the parent rotation
    tableObjectsParent.rotation = originalParentRotation;

    // Restore all child transforms to their original state
    RestoreOriginalTransforms();

    isInOriginalPerspective = true;
    UpdateButtonVisibility();

    // Debug log to verify button states after reset
    Debug.Log("Reset to original view. Switch button active: " +
              (perspectiveSwitchButton != null ? perspectiveSwitchButton.activeSelf.ToString() : "null") +
              ", Reset button active: " + (resetButton != null ? resetButton.activeSelf.ToString() : "null"));
  }

  /// <summary>
  /// Restore all transforms to their original state
  /// </summary>
  private void RestoreOriginalTransforms()
  {
    foreach (var entry in originalChildTransforms)
    {
      Transform child = entry.Key;
      OriginalTransform originalTransform = entry.Value;

      if (child != null)
      {
        // Restore local transform values
        child.localPosition = originalTransform.localPosition;
        child.localRotation = originalTransform.localRotation;
      }
    }

    Debug.Log("Restored original transforms for " + originalChildTransforms.Count + " objects");
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
      // Debug log to verify reset button state
      Debug.Log("UpdateButtonVisibility called - Reset button active: " + resetButton.activeSelf);
    }
  }
}