using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Management;

public class OpenObjectMenu : MonoBehaviour
{
    public GameObject objectMenu;
    public AudioClip buttonClickSound;
    public float palmVelocityThreshold = 0.5f;
    public float palmUpThreshold = 0.7f;
    public float palmDownThreshold = -0.7f; 
    public float buttonPressDepth = 0.05f;
    public float buttonPressSpeed = 0.3f;
    public float buttonCooldown = 1.0f; 

    private XRHandSubsystem hands;
    private XRHand leftHand, rightHand;
    private Vector3 lastPalmPosition;
    private Vector3 origPosition;
    private bool isButtonPressed = false;
    private AudioSource audioSource;
    private bool isPressed = false;
    private bool isInCooldown = false;
    public GameObject tabletop2;
    public XROrigin xrRig;
    public ScreenFader screenFader;

    public AudioClip objectSelectedAudio;

    /* passed in from SelectNewObject.cs */
    [HideInInspector] public GameObject objectSpawned;
    [HideInInspector] public GameObject objectSpawnedCubeContainer;
    [HideInInspector] public bool justSpawned = false;
    public OverheadSwap overheadSwap;

    public Material spawnCubeContainerMaterial;

    void Start()
    {
        lastPalmPosition = Vector3.zero;
        hands = XRGeneralSettings.Instance?
                .Manager?
                .activeLoader?
                .GetLoadedSubsystem<XRHandSubsystem>();

        if (hands != null)
        {
            leftHand = hands.leftHand;
            rightHand = hands.rightHand;
        }

        origPosition = transform.localPosition;
        audioSource = GetComponent<AudioSource>();

        if (objectMenu != null)
        {
            objectMenu.SetActive(false);
        }
    }

    void Update()
    {
        if (hands != null)
           // CheckPalmGesture();
           Debug.Log("justSpawned: " + justSpawned);

        /* continuation of item spawn from SelectNewObject.cs */
        if (justSpawned && !isPressed)  
        {
            if (objectSelectedAudio != null)
                audioSource.PlayOneShot(objectSelectedAudio);
            overheadSwap.AddItem(objectSpawned);
            StartCoroutine(SwapTabletopRoutine());
        }
    }

    void CheckPalmGesture()
    {
        CheckHandGesture(leftHand);
        CheckHandGesture(rightHand);
    }

    /* for open object menu gesture, alternative to button */
    void CheckHandGesture(XRHand hand)
    {
        if (!hand.isTracked)
            return;

        XRHandJoint palmJoint = hand.GetJoint(XRHandJointID.Palm);
        if (palmJoint == null)
            return;

        bool gotPose = palmJoint.TryGetPose(out Pose palmPose);
        if (!gotPose)
            return;

        Vector3 palmPosition = palmPose.position;
        Quaternion palmRotation = palmPose.rotation;

        Vector3 palmVelocity = (palmPosition - lastPalmPosition) / Time.deltaTime;
        lastPalmPosition = palmPosition;

        Vector3 palmUp = palmRotation * Vector3.up;
        float dotProduct = Vector3.Dot(palmUp, Vector3.up);

        if (dotProduct > palmUpThreshold && palmVelocity.y > palmVelocityThreshold)
            ActivateObjectMenu();
        else if (dotProduct < palmDownThreshold && palmVelocity.y < -palmVelocityThreshold)
            ActivateObjectMenu();
    }

    void OnTriggerEnter(Collider other)
    {
        /* pressa de Alain de Botton */
        if (other.gameObject.CompareTag("IndexFingerCollider") && !isButtonPressed && !isInCooldown)
        {
            isPressed = true;
            if (audioSource != null && buttonClickSound != null)
                audioSource.PlayOneShot(buttonClickSound);
            ActivateObjectMenu();
            StartCoroutine(ButtonPressAnimation());
            StartCoroutine(ButtonCooldown());
            transform.position = origPosition;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("IndexFingerCollider"))
            isPressed = false;
    }

    IEnumerator ButtonPressAnimation()
    {
        isButtonPressed = true;
        
        Vector3 targetPosition = origPosition - new Vector3(-buttonPressDepth, 0, 0);
        float elapsedTime = 0f;
        float pressDuration = 0.2f; 

        while (elapsedTime < pressDuration)
        {
            transform.localPosition = Vector3.Lerp(
                origPosition,
                targetPosition,
                elapsedTime / pressDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
        
       yield return new WaitForSeconds(0.1f);

        elapsedTime = 0f;
        while (elapsedTime < pressDuration)
        {
            transform.localPosition = Vector3.Lerp(
                targetPosition,
                origPosition,
                elapsedTime / pressDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = origPosition;
        
        isButtonPressed = false;
        isPressed = false;
    }

    IEnumerator ButtonCooldown()
    {
        isInCooldown = true;
        yield return new WaitForSeconds(buttonCooldown);
        isInCooldown = false;
    }

    void ActivateObjectMenu()
    {
        if (objectMenu != null)
            objectMenu.SetActive(!objectMenu.activeSelf);
    }

    private IEnumerator SwapTabletopRoutine()
	{
        justSpawned = false;
        overheadSwap.isOverhead = true;


		yield return screenFader.FadeOut();

		xrRig.transform.rotation = tabletop2.transform.rotation;
		xrRig.MoveCameraToWorldLocation(tabletop2.transform.position); 


		yield return new WaitForSeconds(0.2f);


		yield return screenFader.FadeIn();
	}

	private IEnumerator WaitForSeconds(float seconds)
	{
		yield return new WaitForSeconds(seconds);
	}
}
