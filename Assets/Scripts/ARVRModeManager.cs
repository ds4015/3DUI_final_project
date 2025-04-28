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
  [SerializeField] private int playerNumber = 1; // Which player is this (1-4)

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
  private Transform vrSpawnPoint; // The spawn point for VR mode
  private Transform arStartMarker; // The start marker for AR mode based on player number

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
    public Vector3 lastARPosition; // Store the most recent AR position
    public Quaternion lastARRotation; // Store the most recent AR rotation
  }

  private void Start()
  {
    // Find the Floor object - now using DropSurface tag
    GameObject floorObject = GameObject.FindGameObjectWithTag("DropSurface");
    if (floorObject != null)
    {
      floorTransform = floorObject.transform;
    }
    else
    {
      Debug.LogError("No object with 'DropSurface' tag found in the scene!");
    }

    // Store table center for reference
    if (tableTop != null)
    {
      tableTopCenter = tableTop.position;
    }

    // Find all portal objects
    GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");
    portalObjects.AddRange(portals);

    // Find the VR spawn point
    GameObject vrSpawnObj = GameObject.FindGameObjectWithTag("VRSpawnPoint");
    if (vrSpawnObj != null)
    {
      vrSpawnPoint = vrSpawnObj.transform;
    }
    else
    {
      Debug.LogWarning("No VRSpawnPoint found in the scene. Will use default spawn location.");
    }

    // Find the appropriate AR start marker based on player number
    string markerName = $"StartMarkerP{playerNumber}";
    GameObject startMarkerObj = GameObject.Find(markerName);
    if (startMarkerObj != null)
    {
      arStartMarker = startMarkerObj.transform;
      // Store this as the original position to return to in AR mode
      originalPosition = arStartMarker.position;
      originalRotation = arStartMarker.rotation;
    }
    else
    {
      Debug.LogWarning($"No {markerName} found in the scene. Will use current position as start position.");
      // Fall back to current position if no marker found
      if (xrOrigin != null)
      {
        originalPosition = xrOrigin.position;
        originalRotation = xrOrigin.rotation;
      }
    }

    if (toggleModeButton != null)
    {
      toggleModeButton.onClick.AddListener(ToggleMode);
      UpdateButtonText();
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
        wasGrabbable = grabComponent != null && grabComponent.enabled,
        lastARPosition = obj.transform.position, // Initialize with current position
        lastARRotation = obj.transform.rotation  // Initialize with current rotation
      };
      buildingObjectsData.Add(data);
    }

    // Start in AR mode
    SetARMode();
  }

  private void Update()
  {
    // If in AR mode, update the last known positions of objects
    if (!isVRMode)
    {
      foreach (BuildingObjectData data in buildingObjectsData)
      {
        if (data.transform != null)
        {
          data.lastARPosition = data.transform.position;
          data.lastARRotation = data.transform.rotation;
        }
      }
    }
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

    // Position player at VR spawn point if available, otherwise use floor height
    if (vrSpawnPoint != null)
    {
      // Teleport the player to the VR spawn point
      xrOrigin.position = vrSpawnPoint.position;
      xrOrigin.rotation = vrSpawnPoint.rotation;

      // Hide the spawn point marker
      vrSpawnPoint.gameObject.SetActive(false);
    }
    else if (floorTransform != null)
    {
      // Fall back to the floor position if no spawn point is available
      float floorHeight = floorTransform.position.y;
      xrOrigin.position = new Vector3(floorTransform.position.x, floorHeight + groundOffset, floorTransform.position.z);
    }

    // Hide the tabletop
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

    // Check if we need to refresh our building objects list (for objects added after scene start)
    RefreshBuildingObjectsList();

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
          rb.velocity = Vector3.zero;
          rb.angularVelocity = Vector3.zero;
        }
        else
        {
          Debug.LogWarning($"No Rigidbody found on {data.transform.name}");
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

          // Force a small upward position adjustment to ensure it's above the floor
          pos.y += 0.01f;
          data.transform.position = pos;
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

  // Helper method to refresh the building objects list
  private void RefreshBuildingObjectsList()
  {
    // Find any new objects that weren't in our original list
    GameObject[] currentBuildingObjects = GameObject.FindGameObjectsWithTag("BuildingObject");
    HashSet<Transform> existingTransforms = new HashSet<Transform>();

    // Create a set of existing transforms for fast lookup
    foreach (BuildingObjectData data in buildingObjectsData)
    {
      if (data.transform != null)
      {
        existingTransforms.Add(data.transform);
      }
    }

    // Check for new objects
    foreach (GameObject obj in currentBuildingObjects)
    {
      if (!existingTransforms.Contains(obj.transform))
      {
        // This is a new object, add it to our list
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabComponent =
          obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        BuildingObjectData data = new BuildingObjectData
        {
          transform = obj.transform,
          originalScale = obj.transform.localScale,
          originalPosition = obj.transform.position,
          originalLocalPosition = obj.transform.localPosition,
          originalParent = obj.transform.parent,
          grabInteractable = grabComponent,
          wasGrabbable = grabComponent != null && grabComponent.enabled,
          lastARPosition = obj.transform.position,
          lastARRotation = obj.transform.rotation
        };
        buildingObjectsData.Add(data);
        Debug.Log($"Added new building object to tracking: {obj.name}");
      }
    }
  }

  private void SetARMode()
  {
    if (mainCamera != null)
    {
      mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    // Show the tabletop
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

    // Show the VR spawn point marker if it exists
    if (vrSpawnPoint != null)
    {
      vrSpawnPoint.gameObject.SetActive(true);
    }

    // Reset XR Origin to the appropriate start position based on player number
    if (arStartMarker != null)
    {
      xrOrigin.position = arStartMarker.position;
      xrOrigin.rotation = arStartMarker.rotation;
    }
    else if (xrOrigin != null)
    {
      // Fall back to the original position if no start marker is found
      xrOrigin.position = originalPosition;
      xrOrigin.rotation = originalRotation;
    }

    // Check if we need to refresh our building objects list
    RefreshBuildingObjectsList();

    // Reset all building objects to their last known AR state
    foreach (BuildingObjectData data in buildingObjectsData)
    {
      if (data.transform != null)
      {
        // Restore original parent
        data.transform.SetParent(data.originalParent);

        // Reset scale and position to last known AR state
        data.transform.localScale = data.originalScale;
        data.transform.position = data.lastARPosition;
        data.transform.rotation = data.lastARRotation;

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