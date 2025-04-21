using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;

public class ARVRModeManager : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private Camera mainCamera;
  [SerializeField] private Transform xrOrigin;
  [SerializeField] private Transform tableTop; // Reference to your tabletop transform

  [Header("UI Elements")]
  [SerializeField] private Button toggleModeButton;
  [SerializeField] private TMP_Text buttonText;

  [Header("Mode Settings")]
  [SerializeField] private float vrScaleFactor = 20f; // How much to scale up in VR mode
  [SerializeField] private Vector3 vrPositionOffset = new Vector3(0f, 1.6f, 0f); // Height offset in VR mode
  [SerializeField] private Material skyboxMaterial; // Assign your skybox material

  private bool isVRMode = false;
  private Vector3 originalScale;
  private Vector3 originalPosition;

  private void Start()
  {
    if (toggleModeButton != null)
    {
      toggleModeButton.onClick.AddListener(ToggleMode);
      UpdateButtonText();
    }

    // Store original transform values
    if (xrOrigin != null)
    {
      originalScale = xrOrigin.localScale;
      originalPosition = xrOrigin.position;
    }

    // Start in AR mode
    SetARMode();
  }

  private void UpdateButtonText()
  {
    if (buttonText != null)
    {
      buttonText.text = isVRMode ? "Switch to AR Mode" : "Switch to VR Mode";
    }
  }

  public void ToggleMode()
  {
    isVRMode = !isVRMode;

    if (isVRMode)
    {
      SetVRMode();
    }
    else
    {
      SetARMode();
    }

    UpdateButtonText();
  }

  private void SetVRMode()
  {
    if (mainCamera != null)
    {
      // Switch to skybox background
      mainCamera.clearFlags = CameraClearFlags.Skybox;
      RenderSettings.skybox = skyboxMaterial;
    }

    if (xrOrigin != null && tableTop != null)
    {
      // Scale up the scene and adjust position
      xrOrigin.localScale = originalScale * vrScaleFactor;

      // Position the player at ground level in the scaled scene
      Vector3 tableCenter = tableTop.position;
      xrOrigin.position = tableCenter + Vector3.Scale(vrPositionOffset, xrOrigin.localScale);
    }
  }

  private void SetARMode()
  {
    if (mainCamera != null)
    {
      // Switch to solid color background for AR passthrough
      mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    if (xrOrigin != null)
    {
      // Reset to original scale and position
      xrOrigin.localScale = originalScale;
      xrOrigin.position = originalPosition;
    }
  }
}