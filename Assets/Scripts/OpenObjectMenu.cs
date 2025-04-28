using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
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

    public AudioClip objectSelectedAudio;

    /* passed in from SelectNewObject.cs */
    [HideInInspector] public GameObject objectSpawned;
    [HideInInspector] public GameObject objectSpawnedCubeContainer;
    [HideInInspector] public bool justSpawned = false;

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

        /* continuation of item spawn from SelectNewObject.cs */
        if (justSpawned && !isPressed)  // Only handle spawning if button wasn't just pressed
        {
            if (objectSelectedAudio != null)
                audioSource.PlayOneShot(objectSelectedAudio);

            if (objectSpawnedCubeContainer != null)
            {
                var rend = objectSpawnedCubeContainer.GetComponent<Renderer>();
                if (spawnCubeContainerMaterial != null)
                    rend.material = spawnCubeContainerMaterial;
            }
            justSpawned = false;
        }

        if (objectSpawned != null && objectSpawnedCubeContainer != null)
        {
            float distance = Vector3.Distance(objectSpawned.transform.position,
                objectSpawnedCubeContainer.transform.position);
            float containerSize = objectSpawnedCubeContainer.transform.localScale.magnitude;
            
            /* delete transparent cube once the newly spawned object is moved */
            if (distance > containerSize * 0.5f)
            {
                Destroy(objectSpawnedCubeContainer);
                objectSpawnedCubeContainer = null;
            }
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
            Debug.Log("Logger: Button pressed");
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
        Debug.Log("Logger: Button press animation started");
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
        Debug.Log("Logger: Button cooldown started");
        isInCooldown = true;
        yield return new WaitForSeconds(buttonCooldown);
        isInCooldown = false;
        Debug.Log("Logger: Button cooldown ended");
    }

    void ActivateObjectMenu()
    {
        Debug.Log("Logger: Activating object menu");
        if (objectMenu != null)
            objectMenu.SetActive(!objectMenu.activeSelf);
        Debug.Log("Logger: Object menu activated: " + objectMenu.activeSelf);
    }
}
