using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class XRModeToggleButton : MonoBehaviour
{
  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 1.0f;
  [Tooltip("Color to highlight the button when finger is near")]
  public Color hoverColor = new Color(0.7f, 0.9f, 0.7f, 1.0f);
  [Tooltip("Play sound when button is clicked")]
  public bool playAudio = true;
  [Tooltip("Detection radius around button to re-enable finger colliders")]
  public float detectionRadius = 0.1f;

  private float lastPressTime = -10f;
  private ARVRModeManager modeManager;
  private Image buttonImage;
  private Color originalColor;
  private bool isHovering = false;
  private Collider buttonCollider;
  private SphereCollider detectionSphere;
  private GameObject detectionObject;
  private List<Collider> nearbyFingerColliders = new List<Collider>();

  void Start()
  {
    // Try to tag this GameObject for easy reference by other scripts
    try
    {
      // Just try to set the tag directly - if the tag doesn't exist, this will use the default "Untagged"
      gameObject.tag = "ARVRToggleButton";
    }
    catch (System.Exception e)
    {
      Debug.LogWarning("Could not set ARVRToggleButton tag: " + e.Message);
    }

    modeManager = FindObjectOfType<ARVRModeManager>();
    if (modeManager == null)
    {
      Debug.LogError("XRModeToggleButton: ARVRModeManager not found in scene!");
    }

    // Get the button image for highlighting
    buttonImage = GetComponent<Image>();
    if (buttonImage != null)
    {
      originalColor = buttonImage.color;
    }

    // Get the button collider
    buttonCollider = GetComponent<Collider>();
    if (buttonCollider == null)
    {
      // If no collider on the button itself, try to find it in children
      buttonCollider = GetComponentInChildren<Collider>();
    }

    // Create a detection sphere to detect approaching fingers
    detectionObject = new GameObject("ButtonDetectionSphere");
    detectionObject.transform.SetParent(transform);
    detectionObject.transform.localPosition = Vector3.zero;
    detectionSphere = detectionObject.AddComponent<SphereCollider>();
    detectionSphere.radius = detectionRadius;
    detectionSphere.isTrigger = true;

    // Add a trigger script to the detection sphere
    DetectionSphereScript detectionScript = detectionObject.AddComponent<DetectionSphereScript>();
    detectionScript.Initialize(this);
  }

  private void OnTriggerEnter(Collider other)
  {
    // Only respond to hand/finger colliders
    if (other.CompareTag("IndexFingerCollider") || other.CompareTag("HandCollider") || other.CompareTag("ControllerTip"))
    {
      // Set hover state
      SetHoverState(true);

      // Cooldown to prevent double-presses
      if (Time.time - lastPressTime < pressCooldown)
        return;
      lastPressTime = Time.time;

      // Play button click sound
      if (playAudio)
      {
        AudioManager.Instance.PlayButtonClickSound();
      }

      if (modeManager != null)
      {
        modeManager.ToggleMode();
      }
    }
  }

  private void OnTriggerStay(Collider other)
  {
    // Keep highlighting while finger is near
    if (other.CompareTag("IndexFingerCollider") || other.CompareTag("HandCollider") || other.CompareTag("ControllerTip"))
    {
      SetHoverState(true);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    // Remove highlighting when finger moves away
    if (other.CompareTag("IndexFingerCollider") || other.CompareTag("HandCollider") || other.CompareTag("ControllerTip"))
    {
      SetHoverState(false);
    }
  }

  private void SetHoverState(bool hovering)
  {
    if (isHovering == hovering) return;

    isHovering = hovering;

    if (buttonImage != null)
    {
      buttonImage.color = hovering ? hoverColor : originalColor;
    }
  }

  // Method to enable a finger collider
  public void EnableFingerCollider(Collider fingerCollider)
  {
    if (fingerCollider != null && !fingerCollider.enabled)
    {
      fingerCollider.enabled = true;

      // Add to tracking list if not already there
      if (!nearbyFingerColliders.Contains(fingerCollider))
      {
        nearbyFingerColliders.Add(fingerCollider);
      }
    }
  }

  // Helper class for the detection sphere
  public class DetectionSphereScript : MonoBehaviour
  {
    private XRModeToggleButton parentScript;

    public void Initialize(XRModeToggleButton parent)
    {
      parentScript = parent;
    }

    private void OnTriggerEnter(Collider other)
    {
      // Check if this is a finger collider
      if (other.CompareTag("IndexFingerCollider") || other.CompareTag("HandCollider") || other.CompareTag("ControllerTip"))
      {
        // Enable the finger collider through the parent script
        parentScript.EnableFingerCollider(other);
      }
    }

    private void OnTriggerStay(Collider other)
    {
      // Keep finger colliders enabled while they're near
      if (other.CompareTag("IndexFingerCollider") || other.CompareTag("HandCollider") || other.CompareTag("ControllerTip"))
      {
        parentScript.EnableFingerCollider(other);
      }
    }
  }
}