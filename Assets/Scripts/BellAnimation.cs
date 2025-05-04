using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

public class BellAnimation : MonoBehaviour
{
	public float shakeForce = 1.0f;
	public float damping = 0.1f;
	public float maxAngle = 30.0f;
	public AudioClip bellSound;
	private Rigidbody bellRigidbody;
	private AudioSource audioSource;
	private Vector3 originalPosition;
	private Quaternion originalRotation;
	private float currentAngle = 0f;
	private float velocity = 0f;

	public float minInitialAngle = 20f;
    public float maxInitialAngle = 45f;
    public float swingDuration = 0.5f;
    public float dampingFactor = 0.85f;
    public Vector3 swingAxis = Vector3.forward;

	public OverheadSwap overheadSwap;
    private Quaternion restRotation;
    private bool isSwinging = false;

	public ScreenFader screenFader;
	public XROrigin xrRig;
	public Transform tabletop2;
	private bool justRung = false;

	void Start()
	{
		restRotation = transform.localRotation;
		bellRigidbody = GetComponent<Rigidbody>();
		if (bellRigidbody == null)
		{
			bellRigidbody = gameObject.AddComponent<Rigidbody>();
			bellRigidbody.isKinematic = true;
			bellRigidbody.useGravity = false;
		}

		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}
		audioSource.clip = bellSound;

		originalPosition = transform.position;
		originalRotation = transform.rotation;
	}


	public void ShakeBell()
	{
	    float initialAngle = Random.Range(minInitialAngle, maxInitialAngle);
        int startDirection = Random.value < 0.5f ? 1 : -1;

        StartCoroutine(SwingCoroutine(initialAngle, startDirection));
		
		if (audioSource != null && bellSound != null)
		{
			audioSource.PlayOneShot(bellSound);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (!isSwinging)
		{
			if (overheadSwap != null)
			{
				if (tabletop2.gameObject.name.Contains("StartMarker"))
					overheadSwap.isOverhead = false;
				else
					overheadSwap.isOverhead = true;
			}
			ShakeBell();
			StartCoroutine(SwapTabletopRoutine());
		}
	}
	
    private IEnumerator SwingCoroutine(float currentMaxAngle, int direction)
    {
        isSwinging = true;

        while (currentMaxAngle > 0.5f)
        {
            float startAngle = -currentMaxAngle * direction;
            float endAngle   =  currentMaxAngle * direction;
            float elapsed    = 0f;

            while (elapsed < swingDuration)
            {
                float t = elapsed / swingDuration;
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                transform.localRotation = restRotation *
                    Quaternion.AngleAxis(angle, swingAxis);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localRotation = restRotation *
                Quaternion.AngleAxis(endAngle, swingAxis);

            currentMaxAngle *= dampingFactor;
            direction *= -1;
        }

        float returnTime = 0.2f;
        float returnElapsed = 0f;
        Quaternion startRotation = transform.localRotation;
        
        while (returnElapsed < returnTime)
        {
            float t = returnElapsed / returnTime;
            t = t * t * (3f - 2f * t);
            transform.localRotation = Quaternion.Slerp(startRotation, restRotation, t);
            returnElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = restRotation;
        isSwinging = false;
    }

	/* switch to overview view */
	private IEnumerator SwapTabletopRoutine()
	{
		yield return screenFader.FadeOut();

		xrRig.transform.rotation = tabletop2.rotation;
		xrRig.MoveCameraToWorldLocation(tabletop2.position); 

		yield return new WaitForSeconds(0.2f);

		yield return screenFader.FadeIn();
	}

	private IEnumerator WaitForSeconds(float seconds)
	{
		yield return new WaitForSeconds(seconds);
	}
} 