using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;

public class ARVRModeManager : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private Camera mainCamera;
  [SerializeField] private Transform xrOrigin;
  [SerializeField] private Transform tableTop;
  [SerializeField] private Transform vrSpawnPoint; // Add an empty GameObject on your table as spawn point

  [Header("UI Elements")]
  [SerializeField] private Button toggleModeButton;
  [SerializeField] private TMP_Text buttonText;

  [Header("Mode Settings")]
  [SerializeField] private float vrScaleFactor = 20f; // How much to scale up in VR mode
  [SerializeField] private float playerHeight = 1.6f;
  [SerializeField] private Material skyboxMaterial; // Assign your skybox material

  private bool isVRMode = false;
  private Vector3 originalScale;
  private Vector3 originalPosition;
  private Quaternion originalRotation;

  private void Start()
  {
    // Ensure proper tags are set
    if (tableTop != null && tableTop.gameObject.tag != "TableTop")
    {
      tableTop.gameObject.tag = "TableTop";
    }

    if (vrSpawnPoint != null && vrSpawnPoint.gameObject.tag != "VRSpawnPoint")
    {
      vrSpawnPoint.gameObject.tag = "VRSpawnPoint";
    }

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
      originalRotation = xrOrigin.rotation;
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
    if (!ValidateReferences())
    {
      Debug.LogError("Missing required references for VR mode transition!");
      return;
    }

    if (mainCamera != null)
    {
      // Switch to skybox background
      mainCamera.clearFlags = CameraClearFlags.Skybox;
      RenderSettings.skybox = skyboxMaterial;
    }

    // Scale up the scene
    xrOrigin.localScale = originalScale * vrScaleFactor;

    // Position the player at the spawn point
    if (vrSpawnPoint != null)
    {
      xrOrigin.position = vrSpawnPoint.position;
      xrOrigin.rotation = vrSpawnPoint.rotation;
    }
    else
    {
      Debug.LogWarning("VR Spawn Point not set - using default positioning");
      // Fallback to a safe position above the table
      if (tableTop != null)
      {
        xrOrigin.position = tableTop.position + Vector3.up * 0.1f;
      }
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
      xrOrigin.rotation = originalRotation;
    }
  }

  private bool ValidateReferences()
  {
    bool isValid = true;

    if (mainCamera == null)
    {
      Debug.LogError("Main Camera reference is missing!");
      isValid = false;
    }

    if (xrOrigin == null)
    {
      Debug.LogError("XR Origin reference is missing!");
      isValid = false;
    }

    if (tableTop == null)
    {
      Debug.LogError("Table Top reference is missing!");
      isValid = false;
    }

    return isValid;
  }
}