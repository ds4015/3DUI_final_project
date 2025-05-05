using UnityEngine;
using UnityEngine.UI;

public class PerspectiveMenuController : MonoBehaviour
{
  [Tooltip("Button to toggle the menu open/closed")]
  public Button toggleButton;

  [Tooltip("The panel containing perspective buttons")]
  public RectTransform menuPanel;

  [Tooltip("Animation speed for expanding/collapsing")]
  public float animationSpeed = 5f;

  [Tooltip("Toggled by finger proximity, similar to buttons")]
  public bool useFingerToggle = true;

  private bool isMenuOpen = false;
  private Vector2 closedSize = new Vector2(70, 70); // Size when closed
  private Vector2 openSize; // Size when open, set in Start
  private Vector2 targetSize;

  void Start()
  {
    // Store the original open size of the menu
    if (menuPanel != null)
    {
      openSize = menuPanel.sizeDelta;
      targetSize = closedSize;
      menuPanel.sizeDelta = closedSize;
    }

    // Add click listener to toggle button if assigned
    if (toggleButton != null)
    {
      toggleButton.onClick.AddListener(ToggleMenu);
    }
  }

  void Update()
  {
    // Animate the menu size
    if (menuPanel != null)
    {
      menuPanel.sizeDelta = Vector2.Lerp(menuPanel.sizeDelta, targetSize, Time.deltaTime * animationSpeed);
    }
  }

  public void ToggleMenu()
  {
    isMenuOpen = !isMenuOpen;
    targetSize = isMenuOpen ? openSize : closedSize;

    // Enable/disable child buttons except the toggle button
    if (menuPanel != null)
    {
      foreach (Transform child in menuPanel)
      {
        // Skip the toggle button
        if (toggleButton != null && child.gameObject == toggleButton.gameObject)
          continue;

        // For perspective buttons
        PerspectiveSwitchButton perspectiveButton = child.GetComponent<PerspectiveSwitchButton>();
        if (perspectiveButton != null)
        {
          // Only show colliders when menu is open
          Collider collider = child.GetComponent<Collider>();
          if (collider != null)
          {
            collider.enabled = isMenuOpen;
          }
        }
      }
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!useFingerToggle) return;

    // Only respond to hand/finger colliders
    if (other.CompareTag("IndexFingerCollider") || other.CompareTag("HandCollider") || other.CompareTag("ControllerTip"))
    {
      if (!isMenuOpen)
      {
        ToggleMenu(); // Open menu when finger approaches
      }
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if (!useFingerToggle) return;

    // Only respond to hand/finger colliders
    if (other.CompareTag("IndexFingerCollider") || other.CompareTag("HandCollider") || other.CompareTag("ControllerTip"))
    {
      if (isMenuOpen)
      {
        // Start a delayed close
        Invoke("CloseMenuDelayed", 2.0f);
      }
    }
  }

  private void CloseMenuDelayed()
  {
    // Only close if it's still open (another trigger might have kept it open)
    if (isMenuOpen)
    {
      ToggleMenu();
    }
  }
}