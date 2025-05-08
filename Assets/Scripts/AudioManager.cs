using UnityEngine;

public class AudioManager : MonoBehaviour
{
  [Header("Audio Settings")]
  [Tooltip("Sound to play when a button is clicked")]
  public AudioClip buttonClickSound;
  [Range(0f, 1f)]
  public float volume = 0.5f;

  private static AudioManager _instance;
  private AudioSource audioSource;

  public static AudioManager Instance
  {
    get
    {
      if (_instance == null)
      {
        GameObject audioManager = new GameObject("AudioManager");
        _instance = audioManager.AddComponent<AudioManager>();
        DontDestroyOnLoad(_instance.gameObject);
        _instance.Initialize();
      }
      return _instance;
    }
  }

  private void Awake()
  {
    if (_instance == null)
    {
      _instance = this;
      DontDestroyOnLoad(gameObject);
      Initialize();
    }
    else if (_instance != this)
    {
      Destroy(gameObject);
    }
  }

  private void Initialize()
  {
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.playOnAwake = false;
  }

  public void PlayButtonClickSound()
  {
    if (buttonClickSound != null && audioSource != null)
    {
      audioSource.PlayOneShot(buttonClickSound, volume);
    }
    else
    {
      Debug.LogWarning("Button click sound not assigned in AudioManager");
    }
  }
}