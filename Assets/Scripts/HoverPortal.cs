using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverPortal : MonoBehaviour
{
    private Vector3 portalStartPos = Vector3.zero;
    private Vector3 portalEndPos = Vector3.zero;


	void Start()
	{
		portalStartPos = transform.position;
        portalEndPos = portalStartPos + Vector3.up * 0.05f;
	}

	void Update()
	{
		float t = Mathf.PingPong(Time.time * 0.3f, 1f);
        transform.localPosition = Vector3.Lerp(portalStartPos, portalEndPos, t);
	}
}
