using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
using System.Collections.Generic;

public class ARVRModeManager : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private Camera mainCamera;
  [SerializeField] private Transform xrOrigin;
  [SerializeField] private Transform tableTop;

  [Header("UI Elements")]
  [SerializeField] private Button toggleModeButton;
  [SerializeField] private TMP_Text buttonText;

  [Header("Mode Settings")]
  [SerializeField] private Material skyboxMaterial;
  [SerializeField] private float groundOffset = 0.1f; // Offset from table surface to place player

  private bool isVRMode = false;
  private Vector3 originalPosition;
  private Quaternion originalRotation;
  private List<BuildingObjectData> buildingObjectsData = new List<BuildingObjectData>();

  // Store original transform data for each building object
  private class BuildingObjectData
  {
    public Transform transform;
    public Vector3 originalScale;
    public Vector3 originalPosition;
    public Vector3 originalLocalPosition;
  }

  private void Start()
  {
    if (toggleModeButton != null)
    {
      toggleModeButton.onClick.AddListener(ToggleMode);
      UpdateButtonText();
    }

    // Store original XR Origin transform
    if (xrOrigin != null)
    {
      originalPosition = xrOrigin.position;
      originalRotation = xrOrigin.rotation;
    }

    // Find and store all building objects' original transforms
    GameObject[] buildingObjects = GameObject.FindGameObjectsWithTag("BuildingObject");
    foreach (GameObject obj in buildingObjects)
    {
      BuildingObjectData data = new BuildingObjectData
      {
        transform = obj.transform,
        originalScale = obj.transform.localScale,
        originalPosition = obj.transform.position,
        originalLocalPosition = obj.transform.localPosition
      };
      buildingObjectsData.Add(data);
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
    if (!ValidateReferences()) return;

    // Switch to skybox
    if (mainCamera != null)
    {
      mainCamera.clearFlags = CameraClearFlags.Skybox;
      RenderSettings.skybox = skyboxMaterial;
    }

    // Position player at table height
    if (tableTop != null)
    {
      float tableHeight = tableTop.position.y;
      xrOrigin.position = new Vector3(tableTop.position.x, tableHeight + groundOffset, tableTop.position.z);
    }

    // Scale up all building objects to life size
    foreach (BuildingObjectData data in buildingObjectsData)
    {
      if (data.transform != null)
      {
        // Set scale to 1,1,1 for life-size
        data.transform.localScale = Vector3.one;

        // Adjust position relative to table
        Vector3 relativePos = data.transform.position - tableTop.position;
        data.transform.position = new Vector3(
          relativePos.x + tableTop.position.x,
          relativePos.y + tableTop.position.y,
          relativePos.z + tableTop.position.z
        );
      }
    }
  }

  private void SetARMode()
  {
    if (mainCamera != null)
    {
      mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    // Reset XR Origin
    if (xrOrigin != null)
    {
      xrOrigin.position = originalPosition;
      xrOrigin.rotation = originalRotation;
    }

    // Reset all building objects to their original state
    foreach (BuildingObjectData data in buildingObjectsData)
    {
      if (data.transform != null)
      {
        data.transform.localScale = data.originalScale;
        data.transform.position = data.originalPosition;
      }
    }
  }

  private bool ValidateReferences()
  {
    if (mainCamera == null)
    {
      Debug.LogError("Main Camera reference is missing!");
      return false;
    }

    if (xrOrigin == null)
    {
      Debug.LogError("XR Origin reference is missing!");
      return false;
    }

    if (tableTop == null)
    {
      Debug.LogError("Table Top reference is missing!");
      return false;
    }

    return true;
  }
}