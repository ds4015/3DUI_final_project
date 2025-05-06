using UnityEngine;

public class PerspectiveUIManager : MonoBehaviour
{
  [Tooltip("The panel containing player perspective buttons")]
  public GameObject perspectivePanel;

  [Tooltip("The PerspectiveSwitcher component")]
  public PerspectiveSwitcher perspectiveSwitcher;

  private void Start()
  {
    // Find the perspective switcher if not assigned
    if (perspectiveSwitcher == null)
    {
      perspectiveSwitcher = FindObjectOfType<PerspectiveSwitcher>();
      if (perspectiveSwitcher == null)
      {
        Debug.LogError("PerspectiveUIManager: PerspectiveSwitcher not found in scene!");
      }
    }

    // Initially hide the panel
    if (perspectivePanel != null)
    {
      perspectivePanel.SetActive(false);
    }
  }

  /// <summary>
  /// Toggle the visibility of the perspective selection panel
  /// </summary>
  public void TogglePerspectivePanel()
  {
    if (perspectivePanel != null)
    {
      perspectivePanel.SetActive(!perspectivePanel.activeSelf);
    }
  }

  /// <summary>
  /// Show the perspective selection panel
  /// </summary>
  public void ShowPerspectivePanel()
  {
    if (perspectivePanel != null)
    {
      perspectivePanel.SetActive(true);
    }
  }

  /// <summary>
  /// Hide the perspective selection panel
  /// </summary>
  public void HidePerspectivePanel()
  {
    if (perspectivePanel != null)
    {
      perspectivePanel.SetActive(false);
    }
  }
}