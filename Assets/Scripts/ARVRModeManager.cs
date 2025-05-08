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
  [SerializeField] private Material transparentMaterial; // Material to use for invisible objects in VR
  [SerializeField] private float groundOffset = 0.1f; // Offset from table surface to place player
  [SerializeField] private float eyeHeight = 1.6f; // Average human eye height in meters
  [SerializeField] private float vrHeightBoost = 0.7f; // Additional height in VR mode for better camera position

  private bool isVRMode = false;
  private Vector3 originalPosition;
  private Quaternion originalRotation;
  private List<BuildingObjectData> buildingObjectsData = new List<BuildingObjectData>();
  private Transform floorTransform;
  private List<GameObject> portalObjects = new List<GameObject>();
  private Vector3 tableTopCenter;
  private Transform vrSpawnPoint; // The spawn point for VR mode
  private Transform arStartMarker; // The start marker for AR mode based on player number
  private Material originalVRSpawnMaterial; // Store the original material
  private bool vrSpawnRigidbodyState; // Store the original Rigidbody state
  private Transform cameraOffsetTransform; // The camera offset child of the XR Origin
  private Vector3 vrSpawnPointOriginalPosition; // Store the original position of VR spawn point
  private Quaternion vrSpawnPointOriginalRotation; // Store the original rotation of VR spawn point
  private Vector3 vrSpawnPointOriginalScale; // Store the original scale of VR spawn point
  private Transform vrSpawnPointOriginalParent; // Store the original parent of VR spawn point
  private bool wasInVRModePreviously = false; // Track if we were in VR mode in the previous state

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
    public GrabPushRotate grabPushRotate; // Reference to the GrabPushRotate component
    public bool wasGrabPushRotateEnabled; // Whether the GrabPushRotate was enabled
  }

  private void Start()
  {
    // Validate that the transparent material is set
    if (transparentMaterial == null)
    {
      Debug.LogWarning("No transparent material assigned to ARVRModeManager. VR spawn point will remain visible in VR mode.");
    }

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

    // Find the camera offset transform inside the XR Origin
    if (xrOrigin != null)
    {
      // The camera offset is typically the first child of the XR Origin
      if (xrOrigin.childCount > 0)
      {
        cameraOffsetTransform = xrOrigin.GetChild(0);
        Debug.Log($"Found camera offset transform: {cameraOffsetTransform.name}");
      }
      else
      {
        Debug.LogWarning("XR Origin has no children. Cannot find camera offset.");
      }
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

      // Store the original position, rotation and scale
      vrSpawnPointOriginalPosition = vrSpawnPoint.position;
      vrSpawnPointOriginalRotation = vrSpawnPoint.rotation;
      vrSpawnPointOriginalScale = vrSpawnPoint.localScale;
      vrSpawnPointOriginalParent = vrSpawnPoint.parent;

      // Store the original material if it has a renderer
      Renderer renderer = vrSpawnPoint.GetComponent<Renderer>();
      if (renderer != null && renderer.material != null)
      {
        originalVRSpawnMaterial = renderer.material;
        Debug.Log($"Stored original VR spawn point material: {originalVRSpawnMaterial.name}");
      }

      // Store original rigidbody state
      Rigidbody rb = vrSpawnPoint.GetComponent<Rigidbody>();
      if (rb != null)
      {
        vrSpawnRigidbodyState = rb.isKinematic == false;
        // Make sure rigidbody is initially kinematic to prevent it from moving
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
      }

      Debug.Log($"Found VRSpawnPoint: {vrSpawnObj.name} at position {vrSpawnObj.transform.position}, parent: {vrSpawnObj.transform.parent?.name}");
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
      GrabPushRotate grabPushComponent = obj.GetComponent<GrabPushRotate>();

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
        lastARRotation = obj.transform.rotation,  // Initialize with current rotation
        grabPushRotate = grabPushComponent,
        wasGrabPushRotateEnabled = grabPushComponent != null && grabPushComponent.enabled
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

    // Save information about the previous mode
    wasInVRModePreviously = !isVRMode;

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

    // If we lost reference to the VR spawn point, try to find it again
    if (vrSpawnPoint == null)
    {
      GameObject vrSpawnObj = GameObject.FindGameObjectWithTag("VRSpawnPoint");
      if (vrSpawnObj != null)
      {
        vrSpawnPoint = vrSpawnObj.transform;

        // Store the original position and rotation if we don't have it
        if (vrSpawnPointOriginalPosition == Vector3.zero)
        {
          vrSpawnPointOriginalPosition = vrSpawnPoint.position;
          vrSpawnPointOriginalRotation = vrSpawnPoint.rotation;
          vrSpawnPointOriginalScale = vrSpawnPoint.localScale;
        }

        // Store the original material if it has a renderer
        Renderer renderer = vrSpawnPoint.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null && originalVRSpawnMaterial == null)
        {
          originalVRSpawnMaterial = renderer.material;
          Debug.Log($"Re-stored original VR spawn point material: {originalVRSpawnMaterial.name}");
        }

        // Store original rigidbody state
        Rigidbody rb = vrSpawnPoint.GetComponent<Rigidbody>();
        if (rb != null)
        {
          vrSpawnRigidbodyState = rb.isKinematic == false;
        }

        Debug.Log($"Refound VRSpawnPoint: {vrSpawnObj.name} at position {vrSpawnObj.transform.position}");
      }
    }

    // Position player at VR spawn point if available, otherwise use floor height
    if (vrSpawnPoint != null)
    {
      // Make sure the object is active first
      vrSpawnPoint.gameObject.SetActive(true);

      // Save the original position before any modifications
      Vector3 savedPosition = vrSpawnPoint.position;
      Quaternion savedRotation = vrSpawnPoint.rotation;

      // Make the VR spawn point invisible
      Renderer renderer = vrSpawnPoint.GetComponent<Renderer>();
      if (renderer != null)
      {
        if (transparentMaterial != null)
        {
          // Store original material if it hasn't been stored yet
          if (originalVRSpawnMaterial == null)
          {
            originalVRSpawnMaterial = renderer.material;
          }

          // Apply transparent material
          renderer.material = transparentMaterial;
          Debug.Log($"Applied transparent material to VRSpawnPoint");
        }
        else
        {
          // If no transparent material is set, just disable the renderer
          renderer.enabled = false;
          Debug.Log("No transparent material available, disabled renderer instead");
        }
      }

      // Ensure the Rigidbody is properly configured
      Rigidbody rb = vrSpawnPoint.GetComponent<Rigidbody>();
      if (rb != null)
      {
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Add freeze constraints to prevent any physics movement
        rb.constraints = RigidbodyConstraints.FreezeAll;
      }
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

    // Track the VR spawn point as well, adding it to the temp parent
    Vector3 vrSpawnPointWorldPosition = Vector3.zero;
    Quaternion vrSpawnPointWorldRotation = Quaternion.identity;
    Transform vrSpawnPointOriginalParentSaved = null;

    if (vrSpawnPoint != null)
    {
      // Store the world position before reparenting
      vrSpawnPointWorldPosition = vrSpawnPoint.position;
      vrSpawnPointWorldRotation = vrSpawnPoint.rotation;
      vrSpawnPointOriginalParentSaved = vrSpawnPoint.parent;

      // Parent to temporary parent
      vrSpawnPoint.SetParent(tempParentTransform, true);

      // Restore world position and rotation
      vrSpawnPoint.position = vrSpawnPointWorldPosition;
      vrSpawnPoint.rotation = vrSpawnPointWorldRotation;

      Debug.Log($"Added VR spawn point to temp parent at position: {vrSpawnPointWorldPosition}");
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

          // Special handling for Bench object that tends to fall through the floor
          if (data.transform.name.Contains("Bench"))
          {
            // Give benches a larger offset from the floor
            pos.y += 0.25f; // Increased from 0.05f to 0.25f to ensure bench sits completely above floor

            // Ensure the bench has proper physics settings
            rb.mass = 10f; // Increase mass to prevent it from moving too easily
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision detection

            // Add freeze position Y to constraints to prevent falling
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

            Debug.Log($"Applied special physics handling for bench: {data.transform.name} with height offset of 0.25f");
          }

          data.transform.position = pos;
        }

        // Disable interaction components in VR mode
        if (data.grabInteractable != null)
        {
          data.grabInteractable.enabled = false;
        }

        // Disable GrabPushRotate script in VR mode
        if (data.grabPushRotate != null)
        {
          data.grabPushRotate.enabled = false;
        }
      }
    }

    // Now handle the VR spawn point separately, parent it to the floor
    Vector3 finalVRSpawnPointPosition = Vector3.zero;
    Quaternion finalVRSpawnPointRotation = Quaternion.identity;

    if (vrSpawnPoint != null && floorTransform != null)
    {
      // Store the world position after scaling 
      Vector3 worldPosition = vrSpawnPoint.position;
      Quaternion worldRotation = vrSpawnPoint.rotation;

      // Parent to floor
      vrSpawnPoint.SetParent(floorTransform, true);

      // Restore world position and rotation
      vrSpawnPoint.position = worldPosition;
      vrSpawnPoint.rotation = worldRotation;

      // Ensure it's at the correct height 
      Vector3 pos = vrSpawnPoint.position;
      pos.y = floorTransform.position.y + 0.01f; // Small offset to avoid z-fighting
      vrSpawnPoint.position = pos;

      // Store the final position and rotation for player teleportation
      finalVRSpawnPointPosition = vrSpawnPoint.position;
      finalVRSpawnPointRotation = vrSpawnPoint.rotation;

      Debug.Log($"VR spawn point final position after scaling and placement: {finalVRSpawnPointPosition}");
    }

    // Clean up the temporary parent
    Destroy(tempParent);

    // Now teleport the player to the scaled VR spawn point position
    if (vrSpawnPoint != null)
    {
      // Calculate target position accounting for player height
      Vector3 targetPosition = finalVRSpawnPointPosition;

      // If we have the camera offset, adjust position to account for eye height
      if (cameraOffsetTransform != null)
      {
        // The XR Origin should be positioned on the ground, with the camera at eye level
        // We need to adjust the XR Origin position to place the player's view at the target height

        // Get current height of the camera relative to XR Origin
        float cameraHeight = 0f;
        foreach (Transform child in cameraOffsetTransform)
        {
          if (child.GetComponent<Camera>() != null)
          {
            cameraHeight = child.localPosition.y;
            break;
          }
        }

        // If found a camera in the offset, use its height
        if (cameraHeight > 0)
        {
          // Lower the XR Origin so the camera is at the spawn point's height
          // Add the VR height boost for more comfortable viewing
          targetPosition.y = targetPosition.y - cameraHeight + vrHeightBoost;
          Debug.Log($"Adjusting for camera height ({cameraHeight}m) with additional height boost of {vrHeightBoost}m");
        }
        else
        {
          // If camera height not found, use default eye height adjustment
          // Add the VR height boost for more comfortable viewing
          targetPosition.y = targetPosition.y - eyeHeight + vrHeightBoost;
          Debug.Log($"Using default eye height adjustment ({eyeHeight}m) with additional height boost of {vrHeightBoost}m");
        }
      }

      // Log the target position
      Debug.Log($"Teleporting to SCALED VRSpawnPoint at position {targetPosition}, rotation: {finalVRSpawnPointRotation.eulerAngles}");

      // Teleport the player to the VR spawn point
      xrOrigin.position = targetPosition;
      xrOrigin.rotation = finalVRSpawnPointRotation;
      // xrOrigin.GetComponent<MovePlayer>().enabled = true;
    }
    else if (floorTransform != null)
    {
      // Fall back to the floor position if no spawn point is available
      float floorHeight = floorTransform.position.y;

      // Determine camera height adjustment
      float heightAdjustment = eyeHeight;
      if (cameraOffsetTransform != null)
      {
        foreach (Transform child in cameraOffsetTransform)
        {
          if (child.GetComponent<Camera>() != null)
          {
            heightAdjustment = child.localPosition.y;
            break;
          }
        }
      }

      // Calculate target position - the XR Origin should be positioned such that
      // the player's view is at an appropriate height above the floor
      // Add the VR height boost for more comfortable viewing
      Vector3 targetPosition = new Vector3(
        floorTransform.position.x,
        floorHeight - heightAdjustment + groundOffset + vrHeightBoost,
        floorTransform.position.z
      );

      Debug.Log($"No VRSpawnPoint found. Teleporting to floor at adjusted height {targetPosition.y} with height boost of {vrHeightBoost}m");
      xrOrigin.position = targetPosition;

    }
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
        GrabPushRotate grabPushComponent = obj.GetComponent<GrabPushRotate>();

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
          lastARRotation = obj.transform.rotation,
          grabPushRotate = grabPushComponent,
          wasGrabPushRotateEnabled = grabPushComponent != null && grabPushComponent.enabled
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

    // Ensure the VR spawn point marker is active if it exists
    if (vrSpawnPoint != null)
    {
      // Make sure the object is active first
      vrSpawnPoint.gameObject.SetActive(true);

      // Reset the parent if we have the original
      if (vrSpawnPointOriginalParent != null)
      {
        vrSpawnPoint.SetParent(vrSpawnPointOriginalParent);
      }

      // Force the position reset to the exact original position
      vrSpawnPoint.position = vrSpawnPointOriginalPosition;
      vrSpawnPoint.rotation = vrSpawnPointOriginalRotation;
      vrSpawnPoint.localScale = vrSpawnPointOriginalScale;
      Debug.Log($"Forced VR spawn point to exact original position: {vrSpawnPointOriginalPosition}");

      // Restore the original material
      Renderer renderer = vrSpawnPoint.GetComponent<Renderer>();
      if (renderer != null)
      {
        // Re-enable the renderer in case it was disabled
        renderer.enabled = true;

        // Restore original material if we have it
        if (originalVRSpawnMaterial != null)
        {
          renderer.material = originalVRSpawnMaterial;
          Debug.Log($"Restored original material to VRSpawnPoint: {originalVRSpawnMaterial.name}");
        }
      }

      // Completely reset the rigidbody
      Rigidbody rb = vrSpawnPoint.GetComponent<Rigidbody>();
      if (rb != null)
      {
        // Always make it kinematic and disable gravity in AR mode to prevent it from moving
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Remove all constraints
        rb.constraints = RigidbodyConstraints.None;

        Debug.Log("Reset VR spawn point physics - made kinematic with no gravity");
      }
    }
    else
    {
      // Try to find it if we lost the reference
      GameObject vrSpawnObj = GameObject.FindGameObjectWithTag("VRSpawnPoint");
      if (vrSpawnObj != null)
      {
        vrSpawnPoint = vrSpawnObj.transform;
        vrSpawnPoint.gameObject.SetActive(true);

        // Restore the original parent if we have it
        if (vrSpawnPointOriginalParent != null)
        {
          vrSpawnPoint.SetParent(vrSpawnPointOriginalParent);
        }

        // Force the position reset to the exact original position
        if (vrSpawnPointOriginalPosition != Vector3.zero)
        {
          vrSpawnPoint.position = vrSpawnPointOriginalPosition;
          vrSpawnPoint.rotation = vrSpawnPointOriginalRotation;
          vrSpawnPoint.localScale = vrSpawnPointOriginalScale;
          Debug.Log($"Forced rediscovered VR spawn point to exact original position: {vrSpawnPointOriginalPosition}");
        }

        // Restore material if possible
        Renderer renderer = vrSpawnPoint.GetComponent<Renderer>();
        if (renderer != null)
        {
          // Re-enable the renderer in case it was disabled
          renderer.enabled = true;

          if (originalVRSpawnMaterial != null)
          {
            renderer.material = originalVRSpawnMaterial;
            Debug.Log($"Restored original material to rediscovered VRSpawnPoint: {originalVRSpawnMaterial.name}");
          }
        }

        // Always make it kinematic in AR mode
        Rigidbody rb = vrSpawnPoint.GetComponent<Rigidbody>();
        if (rb != null)
        {
          rb.isKinematic = true;
          rb.useGravity = false;
          rb.velocity = Vector3.zero;
          rb.angularVelocity = Vector3.zero;
          rb.constraints = RigidbodyConstraints.None;
          Debug.Log("Reset rediscovered VR spawn point physics - made kinematic with no gravity");
        }
      }
    }

    // Determine camera height adjustment
    float heightAdjustment = eyeHeight;
    if (cameraOffsetTransform != null)
    {
      foreach (Transform child in cameraOffsetTransform)
      {
        if (child.GetComponent<Camera>() != null)
        {
          heightAdjustment = child.localPosition.y;
          Debug.Log($"AR mode: Found camera height: {heightAdjustment}m");
          break;
        }
      }
    }

    // Reset XR Origin to the appropriate start position based on player number
    if (arStartMarker != null)
    {
      // Calculate target position adjusted for camera height
      Vector3 targetPosition = arStartMarker.position;

      // Lower the XR Origin by the camera height so your view is at the marker position
      targetPosition.y -= heightAdjustment;

      Debug.Log($"Setting AR position to adjusted marker at {targetPosition} (original: {arStartMarker.position}, camera height: {heightAdjustment})");
      xrOrigin.position = targetPosition;
      xrOrigin.rotation = arStartMarker.rotation;
    }
    else if (xrOrigin != null)
    {
      // Fall back to the original position if no start marker is found
      Vector3 targetPosition = originalPosition;
      targetPosition.y -= heightAdjustment;

      Debug.Log($"Setting AR position to adjusted original position at {targetPosition}");
      xrOrigin.position = targetPosition;
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

        // Special handling for bench objects when returning to AR mode
        if (data.transform.name.Contains("Bench"))
        {
          // Get the rigidbody component
          Rigidbody rb = data.transform.GetComponent<Rigidbody>();
          if (rb != null)
          {
            // Reset to default physics values for AR mode
            rb.mass = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Ensure the bench is properly positioned
            data.transform.position = data.lastARPosition;

            Debug.Log($"Restored bench physics settings in AR mode: {data.transform.name}");
          }
        }

        // Restore original grab interaction state
        if (data.grabInteractable != null)
        {
          data.grabInteractable.enabled = data.wasGrabbable;
        }

        // Restore original GrabPushRotate state
        if (data.grabPushRotate != null)
        {
          data.grabPushRotate.enabled = data.wasGrabPushRotateEnabled;
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