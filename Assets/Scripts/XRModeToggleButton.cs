using UnityEngine;
using UnityEngine.UI;

public class XRModeToggleButton : MonoBehaviour
{
  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 1.0f;
  [Tooltip("Color to highlight the button when finger is near")]
  public Color hoverColor = new Color(0.7f, 0.9f, 0.7f, 1.0f);

  private float lastPressTime = -10f;
  private ARVRModeManager modeManager;
  private Image buttonImage;
  private Color originalColor;
  private bool isHovering = false;

  void Start()
  {
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
}