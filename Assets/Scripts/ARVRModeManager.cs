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
  private Transform floorTransform;
  private List<GameObject> portalObjects = new List<GameObject>();
  private Vector3 tableTopCenter;

  // Store original transform data for each building object
  private class BuildingObjectData
  {
    public Transform transform;
    public Vector3 originalScale;
    public Vector3 originalPosition;
    public Vector3 originalLocalPosition;
    public Transform originalParent;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public bool wasGrabbable;
  }

  private void Start()
  {
    // Find the Floor object
    GameObject floorObject = GameObject.FindGameObjectWithTag("Floor");
    if (floorObject != null)
    {
      floorTransform = floorObject.transform;
    }
    else
    {
      Debug.LogError("No object with 'Floor' tag found in the scene!");
    }

    // Store table center for reference
    if (tableTop != null)
    {
      tableTopCenter = tableTop.position;
    }

    // Find all portal objects
    GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");
    portalObjects.AddRange(portals);

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
      UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabComponent = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

      BuildingObjectData data = new BuildingObjectData
      {
        transform = obj.transform,
        originalScale = obj.transform.localScale,
        originalPosition = obj.transform.position,
        originalLocalPosition = obj.transform.localPosition,
        originalParent = obj.transform.parent,
        grabInteractable = grabComponent,
        wasGrabbable = grabComponent != null && grabComponent.enabled
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

    // Position player at floor height
    if (floorTransform != null)
    {
      float floorHeight = floorTransform.position.y;
      xrOrigin.position = new Vector3(floorTransform.position.x, floorHeight + groundOffset, floorTransform.position.z);
    }

    // Hide the tabletop and portals
    if (tableTop != null)
    {
      tableTop.gameObject.SetActive(false);
    }

    // Disable all portal objects
    foreach (GameObject portal in portalObjects)
    {
      if (portal != null)
      {
        portal.SetActive(false);
      }
    }

    // Create a temporary parent to maintain relative positions during transition
    GameObject tempParent = new GameObject("TempParent");
    tempParent.transform.position = floorTransform.position;
    tempParent.transform.rotation = Quaternion.identity;
    Transform tempParentTransform = tempParent.transform;

    // First, parent all objects to the temporary parent while maintaining their world positions
    foreach (BuildingObjectData data in buildingObjectsData)
    {
      if (data.transform != null)
      {
        // Store the world position before reparenting
        Vector3 worldPosition = data.transform.position;
        Quaternion worldRotation = data.transform.rotation;

        // Parent to temporary parent
        data.transform.SetParent(tempParentTransform, true);

        // Restore world position and rotation
        data.transform.position = worldPosition;
        data.transform.rotation = worldRotation;

        // Disable physics temporarily during transition
        Rigidbody rb = data.transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
          rb.isKinematic = true;
          rb.useGravity = false;
        }
      }
    }

    // Calculate the scale factor based on the table size
    float scaleFactor = 1.0f; // Default scale factor
    if (tableTop != null)
    {
      // Assuming the table is roughly 1 unit in size, scale up to real-world size
      scaleFactor = 10.0f; // Adjust this value based on your desired scale
    }

    // Scale up the temporary parent and all its children
    tempParent.transform.localScale = Vector3.one * scaleFactor;

    // Now parent all objects to the floor while maintaining their relative positions
    foreach (BuildingObjectData data in buildingObjectsData)
    {
      if (data.transform != null && floorTransform != null)
      {
        // Store the world position before reparenting
        Vector3 worldPosition = data.transform.position;
        Quaternion worldRotation = data.transform.rotation;

        // Parent to floor
        data.transform.SetParent(floorTransform, true);

        // Restore world position and rotation
        data.transform.position = worldPosition;
        data.transform.rotation = worldRotation;

        // Ensure the object is at the correct height
        Vector3 pos = data.transform.position;
        pos.y = floorTransform.position.y;
        data.transform.position = pos;

        // Re-enable physics with proper settings
        Rigidbody rb = data.transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
          rb.isKinematic = false;
          rb.useGravity = true;
          rb.velocity = Vector3.zero;
          rb.angularVelocity = Vector3.zero;
          rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent objects from rotating
        }

        // Disable grab interaction in VR mode
        if (data.grabInteractable != null)
        {
          data.grabInteractable.enabled = false;
        }
      }
    }

    // Clean up the temporary parent
    Destroy(tempParent);
  }

  private void SetARMode()
  {
    if (mainCamera != null)
    {
      mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    // Show the tabletop and portals
    if (tableTop != null)
    {
      tableTop.gameObject.SetActive(true);
    }

    // Enable all portal objects
    foreach (GameObject portal in portalObjects)
    {
      if (portal != null)
      {
        portal.SetActive(true);
      }
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
        // Restore original parent
        data.transform.SetParent(data.originalParent);

        // Reset scale and position
        data.transform.localScale = data.originalScale;
        data.transform.position = data.originalPosition;

        // Restore original grab interaction state
        if (data.grabInteractable != null)
        {
          data.grabInteractable.enabled = data.wasGrabbable;
        }
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

    if (floorTransform == null)
    {
      Debug.LogError("No Floor object found in scene!");
      return false;
    }

    return true;
  }
}