using UnityEngine;
using UnityEngine.UI;

public class ResetPerspectiveButton : MonoBehaviour
{
  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 1.0f;
  [Tooltip("Color to highlight the button when finger is near")]
  public Color hoverColor = new Color(0.7f, 0.9f, 0.7f, 1.0f);
  [Tooltip("The PerspectiveSwitcher component to control")]
  public PerspectiveSwitcher perspectiveSwitcher;
  [Tooltip("The GameObject containing this button")]
  public GameObject buttonGameObject;
  [Tooltip("The perspective switch button to show when reset")]
  public GameObject perspectiveSwitchButton;
  [Tooltip("Play sound when button is clicked")]
  public bool playAudio = true;

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
        Debug.LogError("ResetPerspectiveButton: PerspectiveSwitcher not found in scene!");
      }
      else
      {
        // Register this button with the perspective switcher
        perspectiveSwitcher.resetButton = buttonGameObject != null ? buttonGameObject : gameObject;
      }
    }

    // If buttonGameObject is not assigned, use this object
    if (buttonGameObject == null)
    {
      buttonGameObject = gameObject;
    }

    // If perspective switch button is not assigned, try to get it from the perspective switcher
    if (perspectiveSwitchButton == null && perspectiveSwitcher != null)
    {
      perspectiveSwitchButton = perspectiveSwitcher.perspectiveSwitchButton;
    }

    // Get the button image for highlighting
    buttonImage = GetComponent<Image>();
    if (buttonImage != null)
    {
      originalColor = buttonImage.color;
    }

    // Initially hide this button
    gameObject.SetActive(false);

    // Debug log to verify this script is running
    Debug.Log("ResetPerspectiveButton initialized on " + gameObject.name);
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

      // Reset perspective to original view
      if (perspectiveSwitcher != null)
      {
        perspectiveSwitcher.ResetToOriginalView();

        // Hide this button
        gameObject.SetActive(false);

        // Show the perspective switch button
        if (perspectiveSwitchButton != null)
        {
          perspectiveSwitchButton.SetActive(true);
        }

        // Debug log to verify this code is executing
        Debug.Log("Reset button pressed, hiding reset button and showing switch button");
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