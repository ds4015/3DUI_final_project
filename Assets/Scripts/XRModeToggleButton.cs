using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class XRModeToggleButton : MonoBehaviour
{
  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 1.0f;
  private float lastPressTime = -10f;

  private ARVRModeManager modeManager;
  private Camera mainCamera;
  private Mouse mouse;

  // For UI raycasting
  private PointerEventData pointerEventData;
  private EventSystem eventSystem;

  void Start()
  {
    modeManager = FindObjectOfType<ARVRModeManager>();
    if (modeManager == null)
    {
      Debug.LogError("XRModeToggleButton: ARVRModeManager not found in scene!");
    }
    mainCamera = Camera.main;
    mouse = Mouse.current;
    eventSystem = EventSystem.current;

    // Verify collider exists if not a UI element
    if (!GetComponent<RectTransform>() && GetComponent<Collider>() == null)
    {
      Debug.LogError("XRModeToggleButton: No Collider component found on this GameObject! Mouse interaction won't work.");
    }
  }

  void Update()
  {
    // Check for mouse click
    if (mouse != null && mouse.leftButton.wasPressedThisFrame)
    {
      Debug.Log("Mouse click detected");

      // Check if this is a UI element
      if (GetComponent<RectTransform>() != null)
      {
        // UI raycast approach
        if (eventSystem != null)
        {
          pointerEventData = new PointerEventData(eventSystem);
          pointerEventData.position = mouse.position.ReadValue();

          List<RaycastResult> results = new List<RaycastResult>();
          EventSystem.current.RaycastAll(pointerEventData, results);

          foreach (RaycastResult result in results)
          {
            if (result.gameObject == gameObject)
            {
              Debug.Log("UI Button clicked!");
              // Cooldown to prevent double-presses
              if (Time.time - lastPressTime < pressCooldown)
                return;
              lastPressTime = Time.time;

              if (modeManager != null)
              {
                modeManager.ToggleMode();
              }
              break;
            }
          }
        }
      }
      else
      {
        // 3D object raycast approach
        Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
          Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
          if (hit.collider.gameObject == gameObject)
          {
            Debug.Log("Button clicked!");
            // Cooldown to prevent double-presses
            if (Time.time - lastPressTime < pressCooldown)
              return;
            lastPressTime = Time.time;

            if (modeManager != null)
            {
              modeManager.ToggleMode();
            }
          }
        }
        else
        {
          Debug.Log("Raycast didn't hit anything");
        }
      }
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    // Only respond to index finger colliders
    if (other.CompareTag("IndexFingerCollider"))
    {
      // Cooldown to prevent double-presses
      if (Time.time - lastPressTime < pressCooldown)
        return;
      lastPressTime = Time.time;

      if (modeManager != null)
      {
        modeManager.ToggleMode();
      }
    }
  }
}