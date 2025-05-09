using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Fusion.Sockets;
using System;

public class HardwareRig : MonoBehaviour, INetworkRunnerCallbacks
{
    public Transform playArea;
    public Transform headset;
    public Transform leftHand;
    public Transform rightHand;

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        RigIn rigInp = new RigIn
        {
            playAreaPosition = playArea.position,
            playAreaRotation = playArea.rotation,
            headsetPosition = headset.position,
            headsetRotation = headset.rotation,
            leftHandPosition = leftHand.position,
            leftHandRotation = leftHand.rotation,
            rightHandPosition = rightHand.position,
            rightHandRotation = rightHand.rotation
        };
        input.Set(rigInp);

    }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken tok) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reas)
    {

    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

}
