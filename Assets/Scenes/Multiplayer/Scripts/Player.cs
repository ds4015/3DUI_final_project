using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour
{
  private NetworkCharacterController _cc;

  private void Awake()
  {
    _cc = GetComponent<NetworkCharacterController>();
  }

  public override void FixedUpdateNetwork()
  {
        Debug.Log("Running");
    if(!HasInputAuthority)
        {
            return;
        }
    if (GetInput(out NetworkInputData data))
    {
      data.direction.Normalize();
      _cc.Move(5*data.direction*Runner.DeltaTime);

            Debug.Log($"[{Runner.LocalPlayer}] Moving Player. New Position: {transform.position}");
    }
  }
}