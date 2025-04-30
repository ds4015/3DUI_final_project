using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    [SerializeField]
    private Transform[] spawnMarkers;
    private int spawnIndex = 0;


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            if (spawnMarkers != null && spawnIndex < spawnMarkers.Length)
            {
                spawnPosition = spawnMarkers[spawnIndex].position;
                spawnRotation = spawnMarkers[spawnIndex].rotation;
                spawnIndex++;
            }

            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, spawnRotation, player);
            Debug.Log($"Spawned Player: {player} - State Auth: {networkPlayerObject.HasStateAuthority}, InputAuthority: {networkPlayerObject.HasInputAuthority}");
            _spawnedCharacters.Add(player, networkPlayerObject);
            if (!networkPlayerObject.HasInputAuthority)
            {
                AudioListener listener = networkPlayerObject.GetComponentInChildren<AudioListener>();
                listener.enabled = false; //disable audiosource for non-authority players
            }

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }
    /*
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    var data = new NetworkInputData();

    if (Keyboard.current.wKey.isPressed)
        data.direction += Vector3.forward;

    if (Keyboard.current.sKey.isPressed)
        data.direction += Vector3.back;

    if (Keyboard.current.aKey.isPressed)
        data.direction += Vector3.left;

    if (Keyboard.current.dKey.isPressed)
        data.direction += Vector3.right;

        input.Set(data);
    }    
    */
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
    //public static NetworkRunner Instance { get; private set; }

    private NetworkRunner _runner;

    private async void Start()
    {
        _runner = GetComponent<NetworkRunner>();
        if( _runner == null )
            _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        Debug.Log($"Auto-starting: {result.Ok}, Mode: {_runner.Mode}");
        
    }

}
