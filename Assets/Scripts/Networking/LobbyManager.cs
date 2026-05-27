using System;
using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

// Manages Steam lobbies and NGO host/client lifecycle.
// Attach to a persistent GameObject alongside NetworkManager + FacepunchTransport.
// Requires steam_appid.txt (987820) in the project root for editor use.
public class LobbyManager : MonoBehaviour {

    public static LobbyManager me;

    public Lobby? CurrentLobby { get; private set; }

    public event Action<Lobby> OnLobbyReady;
    public event Action OnLobbyLeft;

    FacepunchTransport _transport;

    void Awake() {
        if (me != null && me != this) { Destroy(gameObject); return; }
        me = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        _transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated       += HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered       += HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined  += HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave   += HandleMemberLeft;
        SteamFriends.OnGameLobbyJoinRequested += HandleJoinRequested;
    }

    void OnDestroy() {
        SteamMatchmaking.OnLobbyCreated       -= HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered       -= HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined  -= HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave   -= HandleMemberLeft;
        SteamFriends.OnGameLobbyJoinRequested -= HandleJoinRequested;
        UnsubscribeNGO();
    }

    void OnApplicationQuit() => Disconnect();

    // --- Public API ---

    // Returns the lobby ID as a string (share with friends for manual join, or use Steam overlay invites).
    public async Task<string> StartHost(int maxPlayers = 2) {
        SubscribeNGO();
        NetworkManager.Singleton.StartHost();

        Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
        if (!lobby.HasValue) {
            Debug.LogError("[LobbyManager] Lobby creation failed.");
            NetworkManager.Singleton.Shutdown();
            return null;
        }

        lobby.Value.SetFriendsOnly();
        lobby.Value.SetData("game", "CHRONO");
        lobby.Value.SetJoinable(true);
        CurrentLobby = lobby;
        return lobby.Value.Id.Value.ToString();
    }

    public async Task JoinLobby(SteamId lobbyId) {
        Lobby? lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
        if (!lobby.HasValue)
            Debug.LogError($"[LobbyManager] JoinLobbyAsync failed for lobby {lobbyId}");
        // HandleLobbyEntered fires on success and starts the NGO client
    }

    public void Disconnect() {
        CurrentLobby?.Leave();
        CurrentLobby = null;
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        UnsubscribeNGO();
        OnLobbyLeft?.Invoke();
    }

    // --- Steam Callbacks ---

    void HandleLobbyCreated(Result result, Lobby lobby) {
        if (result != Result.OK) {
            Debug.LogError($"[LobbyManager] Lobby creation failed: {result}");
            NetworkManager.Singleton.Shutdown();
            return;
        }
        Debug.Log($"[LobbyManager] Hosting. Lobby={lobby.Id}");
        OnLobbyReady?.Invoke(lobby);
    }

    void HandleLobbyEntered(Lobby lobby) {
        CurrentLobby = lobby;
        if (NetworkManager.Singleton.IsHost) return;
        ConnectToHost(lobby.Owner.Id);
    }

    // Fires on the invitee when they accept a Steam friend invite
    void HandleJoinRequested(Lobby lobby, SteamId friendId) {
        _ = JoinLobby(lobby.Id);
    }

    void HandleMemberJoined(Lobby lobby, Friend friend) =>
        Debug.Log($"[LobbyManager] {friend.Name} joined.");

    void HandleMemberLeft(Lobby lobby, Friend friend) =>
        Debug.Log($"[LobbyManager] {friend.Name} left.");

    // --- Internal ---

    void ConnectToHost(SteamId hostId) {
        SubscribeNGO();
        _transport.targetSteamId = hostId;
        if (!NetworkManager.Singleton.StartClient())
            Debug.LogError("[LobbyManager] StartClient() failed.");
    }

    void SubscribeNGO() {
        NetworkManager.Singleton.OnServerStarted            += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback  += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void UnsubscribeNGO() {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnServerStarted            -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback  -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    void OnServerStarted()              => Debug.Log("[LobbyManager] Server started.");
    void OnClientConnected(ulong id)    => Debug.Log($"[LobbyManager] Client {id} connected.");
    void OnClientDisconnected(ulong id) => Debug.Log($"[LobbyManager] Client {id} disconnected.");
}
