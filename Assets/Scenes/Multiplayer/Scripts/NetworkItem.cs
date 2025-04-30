using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkItem : NetworkBehaviour
{
    [Networked] private Vector3 Position { get; set; }
    [Networked] public Quaternion Rotation { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            Position = transform.position;
            Rotation = transform.rotation;
            Debug.Log($"[NetworkedItem][Host] Position: {Position}, Rotation: {Rotation.eulerAngles}, Player: {Runner.LocalPlayer}"); 
        }

        else
        {
            transform.position = Position;
            transform.rotation = Rotation;
            Debug.Log($"[NetworkedItem][Client] Position: {Position}, Rotation: {Rotation.eulerAngles}, Player: {Runner.LocalPlayer}");
        }
    }
}
