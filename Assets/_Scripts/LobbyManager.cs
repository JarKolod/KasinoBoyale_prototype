using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    private NetworkVariable<int> readyCount = new(0);
    private Dictionary<ulong, bool> readyStates = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        readyStates[clientId] = false;
        RecalculateReadyCount();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        readyStates.Remove(clientId);
        RecalculateReadyCount();
    }

    [Rpc(SendTo.Server)]
    public void SetReadyServerRpc(ulong clientId, bool ready)
    {
        if (!readyStates.ContainsKey(clientId))
            return;

        readyStates[clientId] = ready;
        RecalculateReadyCount();
    }

    [Rpc(SendTo.Server)]
    public void StartGameServerRpc()
    {
        if (!IsEveryoneReady())
            return;

        NetworkManager.SceneManager.LoadScene(
            "GameScene",
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }

    private void RecalculateReadyCount()
    {
        int count = 0;
        foreach (var kvp in readyStates)
        {
            if (kvp.Value)
                count++;
        }
        readyCount.Value = count;
    }

    private bool IsEveryoneReady()
    {
        if (readyStates.Count == 0)
            return false;

        foreach (var kvp in readyStates)
        {
            if (!kvp.Value)
                return false;
        }
        return true;
    }

    public int ReadyCount => readyCount.Value;
    public int PlayerCount => readyStates.Count;
}
