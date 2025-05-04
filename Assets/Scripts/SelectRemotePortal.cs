using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SelectRemotePortal : MonoBehaviour
{
    public TransferItem transferItem;
    public GameObject actInact;
    private GameObject button1act, button2act, button3act, button4act;
    private GameObject button1inact, button2inact, button3inact, button4inact;
    private ButtonCooldownManager cooldownManager;
    private AudioSource audioSource;
    public AudioClip audioClip;

    void Start()
    {
        cooldownManager = transform.parent.parent.GetComponent<ButtonCooldownManager>();
        if (cooldownManager == null)
            cooldownManager = transform.parent.parent.gameObject.AddComponent<ButtonCooldownManager>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("No audio source found");
        

        
        
        Transform buttonParent = transform.parent.parent;
        if (!buttonParent.gameObject.name.Contains("Player 1"))
        {
            button1act = buttonParent.Find("Button1 Active").gameObject;
            button1inact = buttonParent.Find("Button1").gameObject;
        }
        if (!buttonParent.gameObject.name.Contains("Player 2"))
        {
            button2act = buttonParent.Find("Button2 Active").gameObject;
            button2inact = buttonParent.Find("Button2").gameObject;
        }
        if (!buttonParent.gameObject.name.Contains("Player 3"))
        {
            button3act = buttonParent.Find("Button3 Active").gameObject;
            button3inact = buttonParent.Find("Button3").gameObject;
        }
        if (!buttonParent.gameObject.name.Contains("Player 4"))
        {
            button4act = buttonParent.Find("Button4 Active").gameObject;
            button4inact = buttonParent.Find("Button4").gameObject;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("IndexFingerCollider") && cooldownManager.CanInteract)
        {
            cooldownManager.StartCooldown();
            int player = 0;
            if (transform.parent.parent.gameObject.name.Contains("Player 1"))
                player = 1;
            else if (transform.parent.parent.gameObject.name.Contains("Player 2"))
                player = 2;
            else if (transform.parent.parent.gameObject.name.Contains("Player 3"))
                player = 3;
            else if (transform.parent.parent.gameObject.name.Contains("Player 4"))
                player = 4;

            if (audioSource != null && audioClip != null)
            {
                
                if (audioSource.isPlaying)
                    audioSource.Stop();
                
                if (!audioSource.isPlaying)
                    audioSource.Play();                
                
                StartCoroutine(PauseForAudio());
            }
            else
                Debug.LogError($"AudioSource or clip missing");

            transform.parent.gameObject.SetActive(!transform.parent.gameObject.activeSelf);
            actInact.SetActive(!actInact.activeSelf);
            if (transform.parent.gameObject.name == "Button1")
            {
                transferItem.p1Active = true;
                transferItem.p2Active = false;
                transferItem.p3Active = false;
                transferItem.p4Active = false;
                DisableOtherButtons(player, 1);
            }
            else if (transform.parent.gameObject.name == "Button2")
            {
                transferItem.p2Active = true;
                transferItem.p1Active = false;
                transferItem.p3Active = false;
                transferItem.p4Active = false;
                DisableOtherButtons(player, 2);
            }
            else if (transform.parent.gameObject.name == "Button3")
            {
                transferItem.p3Active = true;
                transferItem.p1Active = false;
                transferItem.p2Active = false;
                transferItem.p4Active = false;
                DisableOtherButtons(player, 3);

            }
            else if (transform.parent.gameObject.name == "Button4")
            {
                transferItem.p4Active = true;
                transferItem.p1Active = false;
                transferItem.p2Active = false;
                transferItem.p3Active = false;
                DisableOtherButtons(player, 4);
            }
            else if (transform.parent.gameObject.name.Contains("Active"))
                DisableOtherButtons(player, 5);
        }
    }

    void DisableOtherButtons(int player, int button)
    {
        if (button == 1)
        {
            if (player != 2)
            {
                button2act.SetActive(false);
                button2inact.SetActive(true);
            }
            if (player != 3)
            {
                button3act.SetActive(false);
                button3inact.SetActive(true);
            }
            if (player != 4)
            {
                button4act.SetActive(false);
                button4inact.SetActive(true);
            }
        }
        else if (button == 2)
        {
            if (player != 1)
            {
                button1act.SetActive(false);
                button1inact.SetActive(true);
            }
            if (player != 3)
            {
                button3act.SetActive(false);
                button3inact.SetActive(true);
            }
            if (player != 4)
            {
                button4act.SetActive(false);
                button4inact.SetActive(true);
            }
        }
        else if (button == 3)
        {
            if (player != 1)
            {
                button1act.SetActive(false);
                button1inact.SetActive(true);
            }
            if (player != 2)
            {
                button2act.SetActive(false);
                button2inact.SetActive(true);
            }
            if (player != 4)
            {
                button4act.SetActive(false);
                button4inact.SetActive(true);
            }
        }
        else if (button == 4)
        {
            if (player != 1)
            {
                button1act.SetActive(false);
                button1inact.SetActive(true);
            }
            if (player != 2)
            {
                button2act.SetActive(false);
                button2inact.SetActive(true);
            }
            if (player != 3)
            {
                button3act.SetActive(false);
                button3inact.SetActive(true);
            }
        }
        else if (button == 5)
        {
            if (player != 1)
            {
                button1act.SetActive(false);
                button1inact.SetActive(true);
            }
            if (player != 2)
            {
                button2act.SetActive(false);
                button2inact.SetActive(true);
            }
            if (player != 3)
            {
                button3act.SetActive(false);
                button3inact.SetActive(true);
            }
            if (player != 4)
            {
                button4act.SetActive(false);
                button4inact.SetActive(true);
            }
        }
    }
    IEnumerator PauseForAudio()
    {
        if (audioSource != null && audioSource.clip != null)
            yield return new WaitForSeconds(1f);
        else
        {
            Debug.LogError("AudioSource or clip missing");
            yield return null;
        }
    }
}

public class ButtonCooldownManager : MonoBehaviour
{
    public bool CanInteract { get; private set; } = true;
    private float cooldownTime = 0.5f;

    public void StartCooldown()
    {
        CanInteract = false;
        StartCoroutine(ResetCooldown());
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        CanInteract = true;
    }
}
