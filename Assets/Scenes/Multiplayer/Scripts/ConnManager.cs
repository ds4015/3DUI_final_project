using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System.Linq;

public class ConnManager : MonoBehaviour, INetworkRunnerCallbacks
{
    //public GameMode gameMode = GameMode.AutoHostOrClient;
    public GameMode gameMode = GameMode.Shared;
    public string roomName = "TabletopVR";
    public NetworkPrefabRef networkRigPref;
    private NetworkRunner _runner;
    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();
    [SerializeField]
    private Transform[] spawnMarkers;
    //private int spawnIndex = 0;
    private async void Start()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        var hardwareRig = FindObjectOfType<HardwareRig>();
        if(hardwareRig != null )
        {
            _runner.AddCallbacks(hardwareRig);
        }
        //begin session
        var startGameArguments = new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };
        var res = await _runner.StartGame(startGameArguments);
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if(_spawnedPlayers.TryGetValue(player, out NetworkObject obj))
        {
            runner.Despawn(obj);
            _spawnedPlayers.Remove(player);
        }
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //if(runner.IsServer)
        //{
            if (player != runner.LocalPlayer) return;
            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;
            int spawnIndex = (runner.ActivePlayers.Count()-1) % spawnMarkers.Length;

            if (spawnMarkers != null && spawnIndex < spawnMarkers.Length)
            {
                spawnPosition = spawnMarkers[spawnIndex].position;
                Debug.Log($"SpawnMarker Position: {spawnMarkers[spawnIndex].position}");
                spawnRotation = spawnMarkers[spawnIndex].rotation;
                spawnIndex++;
            }
            //Vector3 spawnPosition = new Vector3(player.RawEncoded * 2, 0, 0);
            Debug.Log($"Spawning at {spawnPosition}, index at {spawnIndex - 1}");
            NetworkObject playerXRRig = runner.Spawn(networkRigPref, spawnPosition, spawnRotation, player);
            _spawnedPlayers[player] = playerXRRig;
        //}
    }
    public void OnSceneLoadStart(NetworkRunner runner) {  }
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
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }


}
