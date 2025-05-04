using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerspectiveUI : MonoBehaviour
{
  [SerializeField] private SimplePerspectiveSwitcher perspectiveSwitcher;
  [SerializeField] private TextMeshProUGUI perspectiveText;
  [SerializeField] private Button previousButton;
  [SerializeField] private Button nextButton;

  private void Start()
  {
    if (perspectiveText == null)
    {
      Debug.LogWarning("No text component assigned to PerspectiveUI");
    }

    if (perspectiveSwitcher == null)
    {
      perspectiveSwitcher = FindObjectOfType<SimplePerspectiveSwitcher>();
    }

    // Set up button listeners
    if (previousButton != null)
    {
      previousButton.onClick.AddListener(OnPreviousButtonClicked);
    }

    if (nextButton != null)
    {
      nextButton.onClick.AddListener(OnNextButtonClicked);
    }
  }

  // This method should be called from SimplePerspectiveSwitcher when perspective changes
  public void UpdatePerspectiveText(int playerIndex)
  {
    if (perspectiveText != null)
    {
      perspectiveText.text = $"Player {playerIndex + 1} Perspective";
    }
  }

  private void OnPreviousButtonClicked()
  {
    if (perspectiveSwitcher != null)
    {
      perspectiveSwitcher.SwitchToPreviousPlayer();
    }
  }

  private void OnNextButtonClicked()
  {
    if (perspectiveSwitcher != null)
    {
      perspectiveSwitcher.SwitchToNextPlayer();
    }
  }
}