using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReqButton : MonoBehaviour
{
    public UnityEvent onPress;
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("IndexFingerCollider"))
        {
            return;
        }
        onPress?.Invoke();
    }
}
