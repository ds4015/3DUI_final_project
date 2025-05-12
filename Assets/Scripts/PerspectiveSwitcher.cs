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

  [Tooltip("AR/VR toggle button that should be hidden when perspective is changed")]
  public GameObject arvrToggleButton;

  [Header("Audio")]
  [Tooltip("Sound to play when switching perspective")]
  public AudioClip perspectiveSwitchSound;

  [Tooltip("Sound to play when resetting to original view")]
  public AudioClip resetSound;

  [Range(0f, 1f)]
  [Tooltip("Volume for perspective switch sounds")]
  public float audioVolume = 0.7f;

  // AudioSource component for playing sounds
  private AudioSource audioSource;

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
  private Vector3 originalParentPosition;
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
    public Vector3 localScale;

    public OriginalTransform(Transform t)
    {
      position = t.position;
      rotation = t.rotation;
      localPosition = t.localPosition;
      localRotation = t.localRotation;
      localScale = t.localScale;
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

    // Find the AR/VR toggle button if not assigned
    if (arvrToggleButton == null)
    {
      // Try multiple approaches to find the button

      // 1. Try to find by tag (if the tag exists)
      try
      {
        arvrToggleButton = GameObject.FindWithTag("ARVRToggleButton");
      }
      catch (UnityEngine.UnityException)
      {
        // Tag doesn't exist, continue to other methods
      }

      // 2. Try to find by component
      if (arvrToggleButton == null)
      {
        XRModeToggleButton[] toggleButtons = FindObjectsOfType<XRModeToggleButton>();
        if (toggleButtons.Length > 0)
        {
          arvrToggleButton = toggleButtons[0].gameObject;
        }
      }

      // 3. Try to find by name
      if (arvrToggleButton == null)
      {
        // Common names for the AR/VR toggle button
        string[] possibleNames = new string[] {
          "ARVRToggleButton",
          "ARVRModeButton",
          "ModeToggleButton",
          "XRModeToggleButton",
          "ARVRButton"
        };

        foreach (string name in possibleNames)
        {
          GameObject buttonObj = GameObject.Find(name);
          if (buttonObj != null)
          {
            arvrToggleButton = buttonObj;
            break;
          }
        }
      }

      if (arvrToggleButton != null)
      {
        Debug.Log("Found AR/VR toggle button: " + arvrToggleButton.name);
      }
      else
      {
        Debug.LogWarning("Could not find AR/VR toggle button automatically. Please assign it in the inspector.");
      }
    }

    // Set up audio source
    SetupAudioSource();

    // Store the original rotations and positions
    StoreOriginalTransforms();

    // Set initial button states
    UpdateButtonVisibility();

    // Debug log to verify button references
    Debug.Log("PerspectiveSwitcher initialized. Switch button: " +
              (perspectiveSwitchButton != null ? perspectiveSwitchButton.name : "null") +
              ", Reset button: " + (resetButton != null ? resetButton.name : "null") +
              ", AR/VR toggle button: " + (arvrToggleButton != null ? arvrToggleButton.name : "null"));
  }

  /// <summary>
  /// Set up audio source for playing perspective switch sounds
  /// </summary>
  private void SetupAudioSource()
  {
    // Get or add an AudioSource component
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
      audioSource = gameObject.AddComponent<AudioSource>();
      // Configure audio source defaults
      audioSource.playOnAwake = false;
      audioSource.spatialBlend = 0f; // 2D sound
      audioSource.volume = audioVolume;
    }
  }

  /// <summary>
  /// Store the original transforms of all table objects
  /// </summary>
  private void StoreOriginalTransforms()
  {
    originalParentPosition = tableObjectsParent.position;
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

    // Play perspective switch sound
    PlayPerspectiveSwitchSound();

    RotateTableToCurrentPerspective();

    // We're no longer in original perspective
    isInOriginalPerspective = false;
    UpdateButtonVisibility();

    // Debug log to verify button states after switching
    Debug.Log("Switched to perspective " + playerIndex +
              ". Switch button active: " + (perspectiveSwitchButton != null ? perspectiveSwitchButton.activeSelf.ToString() : "null") +
              ", Reset button active: " + (resetButton != null ? resetButton.activeSelf.ToString() : "null") +
              ", AR/VR toggle button active: " + (arvrToggleButton != null ? arvrToggleButton.activeSelf.ToString() : "null"));
  }

  /// <summary>
  /// Play sound for perspective switching
  /// </summary>
  private void PlayPerspectiveSwitchSound()
  {
    if (audioSource != null && perspectiveSwitchSound != null)
    {
      audioSource.clip = perspectiveSwitchSound;
      audioSource.volume = audioVolume;
      audioSource.Play();
    }
  }

  /// <summary>
  /// Play sound for resetting to original view
  /// </summary>
  private void PlayResetSound()
  {
    if (audioSource != null && resetSound != null)
    {
      audioSource.clip = resetSound;
      audioSource.volume = audioVolume;
      audioSource.Play();
    }
  }

  /// <summary>
  /// Reset the table to its original rotation
  /// </summary>
  public void ResetToOriginalView()
  {
    // Play reset sound
    PlayResetSound();

    // First, restore the parent to its original state
    tableObjectsParent.position = originalParentPosition;
    tableObjectsParent.rotation = originalParentRotation;

    // Then restore all child transforms to their original state
    RestoreOriginalTransforms();

    isInOriginalPerspective = true;
    UpdateButtonVisibility();

    // Debug log to verify button states after reset
    Debug.Log("Reset to original view. Switch button active: " +
              (perspectiveSwitchButton != null ? perspectiveSwitchButton.activeSelf.ToString() : "null") +
              ", Reset button active: " + (resetButton != null ? resetButton.activeSelf.ToString() : "null") +
              ", AR/VR toggle button active: " + (arvrToggleButton != null ? arvrToggleButton.activeSelf.ToString() : "null"));
  }

  /// <summary>
  /// Restore all transforms to their original state
  /// </summary>
  private void RestoreOriginalTransforms()
  {
    // First, restore the parent's world position and rotation
    tableObjectsParent.position = originalParentPosition;
    tableObjectsParent.rotation = originalParentRotation;

    // Then restore all child transforms
    foreach (var entry in originalChildTransforms)
    {
      Transform child = entry.Key;
      OriginalTransform originalTransform = entry.Value;

      if (child != null)
      {
        // Direct world transform restoration approach
        child.position = originalTransform.position;
        child.rotation = originalTransform.rotation;

        // Ensure proper scale
        child.localScale = originalTransform.localScale;
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
    Vector3 centerPoint = rotationCenter.position;

    // Reset to original first to ensure consistent rotation
    tableObjectsParent.position = originalParentPosition;
    tableObjectsParent.rotation = originalParentRotation;

    // Apply rotation around the center point
    float rotationAngle = targetPosition.rotationAngle;
    tableObjectsParent.RotateAround(centerPoint, Vector3.up, rotationAngle);

    Debug.Log("Rotated table by " + rotationAngle + " degrees around " + centerPoint);
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

    // Control AR/VR toggle button visibility
    if (arvrToggleButton != null)
    {
      // Show AR/VR toggle button only in original perspective
      arvrToggleButton.SetActive(isInOriginalPerspective);
      Debug.Log("UpdateButtonVisibility called - AR/VR toggle button active: " + arvrToggleButton.activeSelf);
    }
  }
}