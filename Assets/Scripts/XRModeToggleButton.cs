using UnityEngine;

public class XRModeToggleButton : MonoBehaviour
{
  [Tooltip("Cooldown time in seconds between button presses.")]
  public float pressCooldown = 1.0f;
  private float lastPressTime = -10f;

  private ARVRModeManager modeManager;

  void Start()
  {
    modeManager = FindObjectOfType<ARVRModeManager>();
    if (modeManager == null)
    {
      Debug.LogError("XRModeToggleButton: ARVRModeManager not found in scene!");
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