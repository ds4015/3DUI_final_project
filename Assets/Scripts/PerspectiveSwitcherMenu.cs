using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerspectiveSwitcherMenu : MonoBehaviour
{
  [Header("Menu References")]
  [SerializeField] private GameObject menuPanel;
  [SerializeField] private Button expandButton;
  [SerializeField] private Button[] playerButtons = new Button[4];
  [SerializeField] private TextMeshProUGUI[] playerButtonTexts = new TextMeshProUGUI[4];
  [SerializeField] private GameObject[] playerButtonObjects = new GameObject[4];

  [Header("Components")]
  [SerializeField] private SimplePerspectiveSwitcher perspectiveSwitcher;

  [Header("Settings")]
  [SerializeField] private Color activeButtonColor = new Color(0.2f, 0.8f, 0.2f);
  [SerializeField] private Color normalButtonColor = new Color(0.2f, 0.2f, 0.8f);
  [SerializeField] private bool collapseAfterSelection = true;
  [SerializeField] private Vector3 collapsedPosition = new Vector3(0, 0, 0);
  [SerializeField] private Vector3 expandedPosition = new Vector3(0, 0, 0);
  [SerializeField] private float animationDuration = 0.3f;

  private bool isMenuExpanded = false;
  private Coroutine animationCoroutine;

  private void Start()
  {
    // Find perspective switcher if not assigned
    if (perspectiveSwitcher == null)
    {
      perspectiveSwitcher = FindObjectOfType<SimplePerspectiveSwitcher>();
    }

    // Set up expand button
    if (expandButton != null)
    {
      expandButton.onClick.AddListener(ToggleMenu);
    }

    // Set up player buttons
    for (int i = 0; i < playerButtons.Length; i++)
    {
      if (playerButtons[i] != null)
      {
        int playerIndex = i; // Need to capture for lambda
        playerButtons[i].onClick.AddListener(() => OnPlayerButtonClicked(playerIndex));

        // Hide buttons initially if their corresponding markers don't exist
        UpdateButtonState(i);
      }
    }

    // Collapse menu initially
    menuPanel.transform.localPosition = collapsedPosition;
    SetMenuExpandedState(false);
  }

  public void UpdateButtonStates()
  {
    for (int i = 0; i < playerButtons.Length; i++)
    {
      UpdateButtonState(i);
    }
  }

  private void UpdateButtonState(int playerIndex)
  {
    if (playerButtons[playerIndex] == null) return;

    bool hasMarker = perspectiveSwitcher.HasPlayerMarker(playerIndex);

    // Enable/disable button based on whether the marker exists
    if (playerButtonObjects[playerIndex] != null)
    {
      playerButtonObjects[playerIndex].SetActive(hasMarker);
    }
    else
    {
      playerButtons[playerIndex].gameObject.SetActive(hasMarker);
    }

    // Update button color based on current perspective
    if (hasMarker)
    {
      bool isCurrentPerspective = (perspectiveSwitcher.GetCurrentPlayerIndex() == playerIndex);
      Image buttonImage = playerButtons[playerIndex].GetComponent<Image>();

      if (buttonImage != null)
      {
        buttonImage.color = isCurrentPerspective ? activeButtonColor : normalButtonColor;
      }

      // Update button text
      if (playerButtonTexts[playerIndex] != null)
      {
        playerButtonTexts[playerIndex].text = $"Player {playerIndex + 1}";
      }
    }
  }

  private void OnPlayerButtonClicked(int playerIndex)
  {
    // Switch perspective
    if (perspectiveSwitcher != null)
    {
      perspectiveSwitcher.SwitchToPlayer(playerIndex);
    }

    // Update button states
    UpdateButtonStates();

    // Collapse menu if needed
    if (collapseAfterSelection)
    {
      SetMenuExpandedState(false);
    }
  }

  public void ToggleMenu()
  {
    SetMenuExpandedState(!isMenuExpanded);
  }

  public void SetMenuExpandedState(bool expanded)
  {
    isMenuExpanded = expanded;

    // Stop any running animation
    if (animationCoroutine != null)
    {
      StopCoroutine(animationCoroutine);
    }

    // Start animation
    animationCoroutine = StartCoroutine(AnimateMenu(expanded ? expandedPosition : collapsedPosition));

    // Update button states if expanding
    if (expanded)
    {
      UpdateButtonStates();
    }
  }

  private IEnumerator AnimateMenu(Vector3 targetPosition)
  {
    Vector3 startPosition = menuPanel.transform.localPosition;
    float elapsed = 0f;

    while (elapsed < animationDuration)
    {
      elapsed += Time.deltaTime;
      float t = Mathf.Clamp01(elapsed / animationDuration);
      t = Mathf.SmoothStep(0, 1, t); // Add easing

      menuPanel.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);

      yield return null;
    }

    // Ensure final position is exact
    menuPanel.transform.localPosition = targetPosition;

    // Make buttons interactive only when expanded
    for (int i = 0; i < playerButtons.Length; i++)
    {
      if (playerButtons[i] != null)
      {
        playerButtons[i].interactable = isMenuExpanded;
      }
    }

    animationCoroutine = null;
  }
}