using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerspectiveButtonController : MonoBehaviour
{
  [Header("Perspective Settings")]
  [SerializeField] private int perspectiveIndex = 0; // 0-3 for Player 1-4
  [SerializeField] private SimplePerspectiveSwitcher perspectiveSwitcher;

  [Header("Visual Feedback")]
  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 0.5f;
  [Tooltip("Color to highlight the button when finger is near")]
  public Color hoverColor = new Color(0.7f, 0.9f, 0.7f, 1.0f);
  [Tooltip("Color for the active/current perspective")]
  public Color activeColor = new Color(0.2f, 0.8f, 0.2f, 1.0f);
  [Tooltip("Normal color for available perspectives")]
  public Color normalColor = new Color(0.2f, 0.2f, 0.8f, 1.0f);

  [Header("Optional Components")]
  [SerializeField] private TextMeshProUGUI buttonText;
  [SerializeField] private PerspectiveSwitcherMenu menuController;

  private float lastPressTime = -10f;
  private Image buttonImage;
  private Color originalColor;
  private bool isHovering = false;

  void Start()
  {
    // Find the perspective switcher if not set
    if (perspectiveSwitcher == null)
    {
      perspectiveSwitcher = FindObjectOfType<SimplePerspectiveSwitcher>();
      if (perspectiveSwitcher == null)
      {
        Debug.LogError("PerspectiveButtonController: SimplePerspectiveSwitcher not found in scene!");
      }
    }

    // Get the button image for highlighting
    buttonImage = GetComponent<Image>();
    if (buttonImage != null)
    {
      originalColor = buttonImage.color;
    }

    // Set initial button text if assigned
    if (buttonText != null)
    {
      buttonText.text = $"Player {perspectiveIndex + 1}";
    }

    // Find menu controller if not assigned
    if (menuController == null)
    {
      menuController = GetComponentInParent<PerspectiveSwitcherMenu>();
    }

    // Check if this perspective is available
    UpdateButtonState();
  }

  void Update()
  {
    // Periodically update button state to reflect current perspective
    UpdateButtonState();
  }

  public void UpdateButtonState()
  {
    if (perspectiveSwitcher == null) return;

    bool isAvailable = perspectiveSwitcher.HasPlayerMarker(perspectiveIndex);
    bool isCurrentPerspective = perspectiveSwitcher.GetCurrentPlayerIndex() == perspectiveIndex;

    // Update button interactability
    Button button = GetComponent<Button>();
    if (button != null)
    {
      button.interactable = isAvailable;
    }

    // Update color (if not currently hovering)
    if (buttonImage != null && !isHovering)
    {
      if (isCurrentPerspective)
      {
        buttonImage.color = activeColor;
      }
      else if (isAvailable)
      {
        buttonImage.color = normalColor;
      }
      else
      {
        buttonImage.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.5f); // Dimmed version
      }

      // Update original color for hover state reference
      originalColor = buttonImage.color;
    }

    // Update gameObject active state if not available
    gameObject.SetActive(isAvailable);
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

      // Switch to this perspective
      if (perspectiveSwitcher != null && perspectiveSwitcher.HasPlayerMarker(perspectiveIndex))
      {
        perspectiveSwitcher.SwitchToPlayer(perspectiveIndex);

        // Collapse menu if it has a controller
        if (menuController != null)
        {
          menuController.SetMenuExpandedState(false);
        }
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