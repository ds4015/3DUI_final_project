using UnityEngine;
using UnityEngine.UI;

public class PerspectiveSwitchButton : MonoBehaviour
{
  [Tooltip("Player index (0-3) this button will switch to")]
  public int playerIndex = 0;

  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 1.0f;

  [Tooltip("Color to highlight the button when finger is near")]
  public Color hoverColor = new Color(0.7f, 0.9f, 0.7f, 1.0f);

  [Tooltip("Color to use when this perspective is active")]
  public Color activeColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);

  private float lastPressTime = -10f;
  private SimplePerspectiveSwitcher perspectiveSwitcher;
  private Image buttonImage;
  private Color originalColor;
  private bool isHovering = false;

  void Start()
  {
    // Find the perspective switcher
    perspectiveSwitcher = FindObjectOfType<SimplePerspectiveSwitcher>();
    if (perspectiveSwitcher == null)
    {
      Debug.LogError("PerspectiveSwitchButton: SimplePerspectiveSwitcher not found in scene!");
    }

    // Get the button image for highlighting
    buttonImage = GetComponent<Image>();
    if (buttonImage != null)
    {
      originalColor = buttonImage.color;
    }

    // Check if this perspective is available
    UpdateButtonState();
  }

  void Update()
  {
    // Update the button's visual state to match current perspective
    UpdateButtonState();
  }

  private void UpdateButtonState()
  {
    if (perspectiveSwitcher == null || buttonImage == null) return;

    bool isAvailable = perspectiveSwitcher.HasPlayerMarker(playerIndex);
    bool isActive = perspectiveSwitcher.GetCurrentPlayerIndex() == playerIndex;

    // Disable button if perspective is not available
    Button button = GetComponent<Button>();
    if (button != null)
    {
      button.interactable = isAvailable;
    }

    // Update color based on active/hover state
    if (!isHovering)
    {
      buttonImage.color = isActive ? activeColor : originalColor;
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    // Only respond to hand/finger colliders
    if (other.CompareTag("IndexFingerCollider") || other.CompareTag("HandCollider") || other.CompareTag("ControllerTip"))
    {
      // Set hover state
      SetHoverState(true);

      // Check cooldown to prevent double-presses
      if (Time.time - lastPressTime < pressCooldown)
        return;

      lastPressTime = Time.time;

      if (perspectiveSwitcher != null && perspectiveSwitcher.HasPlayerMarker(playerIndex))
      {
        perspectiveSwitcher.SwitchToPlayer(playerIndex);
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
      if (hovering)
      {
        buttonImage.color = hoverColor;
      }
      else
      {
        // Reset to either active or original color
        UpdateButtonState();
      }
    }
  }
}