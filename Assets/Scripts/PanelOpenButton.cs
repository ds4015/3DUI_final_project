using UnityEngine;
using UnityEngine.UI;

public class PanelOpenButton : MonoBehaviour
{
  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 1.0f;
  [Tooltip("Color to highlight the button when finger is near")]
  public Color hoverColor = new Color(0.7f, 0.9f, 0.7f, 1.0f);
  [Tooltip("The panel to show/hide when button is pressed")]
  public GameObject targetPanel;
  [Tooltip("UI Manager that controls the perspective panel")]
  public PerspectiveUIManager uiManager;
  [Tooltip("The PerspectiveSwitcher component")]
  public PerspectiveSwitcher perspectiveSwitcher;
  [Tooltip("The GameObject containing this button")]
  public GameObject buttonGameObject;
  [Tooltip("Play sound when button is clicked")]
  public bool playAudio = true;

  private float lastPressTime = -10f;
  private Image buttonImage;
  private Color originalColor;
  private bool isHovering = false;

  void Start()
  {
    // Check if target panel is assigned
    if (targetPanel == null && uiManager == null)
    {
      Debug.LogError("PanelOpenButton: Either target panel or UI Manager must be assigned!");
    }

    // Check if perspective switcher is assigned
    if (perspectiveSwitcher == null)
    {
      perspectiveSwitcher = FindObjectOfType<PerspectiveSwitcher>();
      if (perspectiveSwitcher == null)
      {
        Debug.LogWarning("PanelOpenButton: PerspectiveSwitcher not found in scene!");
      }
      else
      {
        // Register this button with the perspective switcher
        perspectiveSwitcher.perspectiveSwitchButton = buttonGameObject != null ? buttonGameObject : gameObject;
      }
    }

    // Check if UI manager is assigned
    if (uiManager == null)
    {
      uiManager = FindObjectOfType<PerspectiveUIManager>();
      if (uiManager == null)
      {
        Debug.LogWarning("PanelOpenButton: PerspectiveUIManager not found in scene!");
      }
    }

    // If buttonGameObject is not assigned, use this object
    if (buttonGameObject == null)
    {
      buttonGameObject = gameObject;
    }

    // Get the button image for highlighting
    buttonImage = GetComponent<Image>();
    if (buttonImage != null)
    {
      originalColor = buttonImage.color;
    }

    // Ensure panel is initially hidden
    if (targetPanel != null)
    {
      targetPanel.SetActive(false);
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

      // Play button click sound
      if (playAudio)
      {
        AudioManager.Instance.PlayButtonClickSound();
      }

      // Toggle panel visibility
      if (uiManager != null)
      {
        uiManager.TogglePerspectivePanel();
      }
      else if (targetPanel != null)
      {
        targetPanel.SetActive(!targetPanel.activeSelf);
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