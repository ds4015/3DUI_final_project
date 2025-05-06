using UnityEngine;
using UnityEngine.UI;

public class PerspectiveButton : MonoBehaviour
{
  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 1.0f;
  [Tooltip("Color to highlight the button when finger is near")]
  public Color hoverColor = new Color(0.7f, 0.9f, 0.7f, 1.0f);
  [Tooltip("The PerspectiveSwitcher component to control")]
  public PerspectiveSwitcher perspectiveSwitcher;
  [Tooltip("The index of the player perspective to switch to")]
  public int targetPlayerIndex = 0;
  [Tooltip("Optional text component to display the player name")]
  public Text playerNameText;

  private float lastPressTime = -10f;
  private Image buttonImage;
  private Color originalColor;
  private bool isHovering = false;

  void Start()
  {
    // Check if perspective switcher is assigned
    if (perspectiveSwitcher == null)
    {
      perspectiveSwitcher = FindObjectOfType<PerspectiveSwitcher>();
      if (perspectiveSwitcher == null)
      {
        Debug.LogError("PerspectiveButton: PerspectiveSwitcher not found in scene!");
      }
    }

    // Get the button image for highlighting
    buttonImage = GetComponent<Image>();
    if (buttonImage != null)
    {
      originalColor = buttonImage.color;
    }

    // Set player name text if available
    UpdatePlayerNameText();
  }

  private void UpdatePlayerNameText()
  {
    if (playerNameText != null && perspectiveSwitcher != null &&
        targetPlayerIndex >= 0 && targetPlayerIndex < perspectiveSwitcher.playerPositions.Length)
    {
      playerNameText.text = perspectiveSwitcher.playerPositions[targetPlayerIndex].playerName;
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

      // Switch perspective
      if (perspectiveSwitcher != null)
      {
        perspectiveSwitcher.SwitchToPerspective(targetPlayerIndex);
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