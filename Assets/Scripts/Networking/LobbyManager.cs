using System;
using System.IO;
using System.Runtime.InteropServices;
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
    public event Action OnLobbyUpdated;

    FacepunchTransport _transport;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    [DllImport("libdl.dylib", EntryPoint = "dlopen")]
    static extern IntPtr dlopen(string path, int flags);
    [DllImport("libdl.dylib", EntryPoint = "dlsym")]
    static extern IntPtr dlsym(IntPtr handle, string symbol);
#endif

    void Awake() {
        Debug.Log($"[LobbyManager] Awake — existing instance: {(me != null ? me.gameObject.name : "none")}");
        if (me != null && me != this) { Destroy(gameObject); return; }
        me = this;
        DontDestroyOnLoad(gameObject);
        PreloadSteamLibrary();
        InitSteam();
    }

    static void PreloadSteamLibrary() {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#if UNITY_EDITOR
        string path = Path.Combine(Application.dataPath, "Plugins", "libsteam_api.dylib");
#else
        string path = Path.Combine(Application.dataPath, "PlugIns", "libsteam_api.dylib");
#endif
        Debug.Log($"[LobbyManager] Preloading Steam lib from: {path}");
        if (!File.Exists(path)) {
            Debug.LogError($"[LobbyManager] libsteam_api.dylib NOT FOUND at: {path}");
            return;
        }
        const int RTLD_NOW    = 2;
        const int RTLD_GLOBAL = 8;
        IntPtr handle = dlopen(path, RTLD_NOW | RTLD_GLOBAL);
        if (handle == IntPtr.Zero) {
            Debug.LogError($"[LobbyManager] dlopen failed — try: codesign -f -s - \"{path}\"");
            return;
        }
        Debug.Log($"[LobbyManager] Steam library preloaded OK.");
#endif
    }

    static void InitSteam() {
        if (SteamClient.IsValid) {
            Debug.Log($"[LobbyManager] Steam already initialized — Name={SteamClient.Name} IsLoggedOn={SteamClient.IsLoggedOn}");
            return;
        }
        Debug.Log("[LobbyManager] Calling SteamClient.Init(987820)...");
        try {
            SteamClient.Init(987820, false);
            SteamNetworkingUtils.InitRelayNetworkAccess();
            Debug.Log($"[LobbyManager] SteamClient.Init OK — Name={SteamClient.Name} IsLoggedOn={SteamClient.IsLoggedOn} SteamId={SteamClient.SteamId}");
        } catch (Exception e) {
            Debug.LogError($"[LobbyManager] SteamClient.Init FAILED: {e.Message}");
        }
    }

    void Start() {
        Debug.Log("[LobbyManager] Start — subscribing Steam callbacks.");
        _transport = NetworkManager.Singleton?.GetComponent<FacepunchTransport>();
        Debug.Log($"[LobbyManager] FacepunchTransport found: {_transport != null}");

        SteamMatchmaking.OnLobbyCreated       += HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered       += HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined  += HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave   += HandleMemberLeft;
        SteamFriends.OnGameLobbyJoinRequested += HandleJoinRequested;
        Debug.Log("[LobbyManager] Steam callbacks subscribed.");
    }

    void OnDestroy() {
        SteamMatchmaking.OnLobbyCreated       -= HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered       -= HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined  -= HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave   -= HandleMemberLeft;
        SteamFriends.OnGameLobbyJoinRequested -= HandleJoinRequested;
        UnsubscribeNGO();
    }

    void Update() {
        // asyncCallbacks=false means Facepunch won't dispatch on its own.
        // FacepunchTransport only ticks this while NGO is listening, so we pump
        // it here to cover the pre-NGO lobby join/create phase as well.
        if (SteamClient.IsValid)
            SteamClient.RunCallbacks();
    }

    void OnApplicationQuit() => Disconnect();

    // --- Public API ---

    public async Task<string> StartHost(int maxPlayers = 2) {
        Debug.Log($"[LobbyManager] StartHost called. SteamClient.IsValid={SteamClient.IsValid}");
        if (!SteamClient.IsValid) {
            Debug.Log("[LobbyManager] Steam not valid, re-initialising...");
            InitSteam();
        }
        if (!SteamClient.IsValid || !SteamClient.IsLoggedOn) {
            Debug.LogError($"[LobbyManager] Steam not ready — IsValid={SteamClient.IsValid}. Is Steam running?");
            return null;
        }
        Debug.Log($"[LobbyManager] Steam OK. Starting NGO host...");
        SubscribeNGO();
        NetworkManager.Singleton.StartHost();
        Debug.Log("[LobbyManager] NGO host started. Creating Steam lobby...");

        Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
        if (!lobby.HasValue) {
            Debug.LogError("[LobbyManager] CreateLobbyAsync returned null — lobby creation failed.");
            NetworkManager.Singleton.Shutdown();
            return null;
        }

        Debug.Log($"[LobbyManager] Lobby created: id={lobby.Value.Id} memberCount={lobby.Value.MemberCount}");
        lobby.Value.SetFriendsOnly();
        lobby.Value.SetData("game", "CHRONO");
        lobby.Value.SetJoinable(true);
        CurrentLobby = lobby;
        Debug.Log($"[LobbyManager] CurrentLobby set in StartHost. Returning id string.");
        return lobby.Value.Id.Value.ToString();
    }

    bool _joining;
    public async Task JoinLobby(SteamId lobbyId) {
        Debug.Log($"[LobbyManager] JoinLobby called. lobbyId={lobbyId} _joining={_joining} SteamClient.IsValid={SteamClient.IsValid}");
        if (_joining) {
            Debug.LogWarning("[LobbyManager] JoinLobby ignored — already joining.");
            return;
        }
        _joining = true;
        if (!SteamClient.IsValid) {
            Debug.Log("[LobbyManager] Steam not valid, re-initialising before join...");
            InitSteam();
        }
        Debug.Log("[LobbyManager] Calling JoinLobbyAsync...");
        Lobby? lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
        _joining = false;
        if (!lobby.HasValue) {
            Debug.LogError($"[LobbyManager] JoinLobbyAsync returned null for lobby {lobbyId}.");
            return;
        }
        Debug.Log($"[LobbyManager] JoinLobbyAsync succeeded. Lobby={lobby.Value.Id} Owner={lobby.Value.Owner.Name} MemberCount={lobby.Value.MemberCount}");
        CurrentLobby = lobby;
        Debug.Log("[LobbyManager] CurrentLobby set in JoinLobby.");
        // HandleLobbyEntered fires separately and calls ConnectToHost
    }

    public void Disconnect() {
        Debug.Log($"[LobbyManager] Disconnect called. CurrentLobby set: {CurrentLobby.HasValue}");
        CurrentLobby?.Leave();
        CurrentLobby = null;
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        UnsubscribeNGO();
        OnLobbyLeft?.Invoke();
    }

    // --- Steam Callbacks ---

    void HandleLobbyCreated(Result result, Lobby lobby) {
        Debug.Log($"[LobbyManager] HandleLobbyCreated — result={result} lobbyId={lobby.Id}");
        if (result != Result.OK) {
            Debug.LogError($"[LobbyManager] Lobby creation failed with result: {result}");
            NetworkManager.Singleton.Shutdown();
            return;
        }
        CurrentLobby = lobby;
        Debug.Log($"[LobbyManager] CurrentLobby set in HandleLobbyCreated. Firing OnLobbyReady + OnLobbyUpdated.");
        OnLobbyReady?.Invoke(lobby);
        FireLobbyUpdated("HandleLobbyCreated");
    }

    void HandleLobbyEntered(Lobby lobby) {
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        Debug.Log($"[LobbyManager] HandleLobbyEntered — lobbyId={lobby.Id} Owner={lobby.Owner.Name} IsHost={isHost}");
        CurrentLobby = lobby;
        Debug.Log($"[LobbyManager] CurrentLobby set in HandleLobbyEntered.");
        FireLobbyUpdated("HandleLobbyEntered");
        // Steam populates the member cache asynchronously after LobbyEntered_t fires.
        // Fire a second update after a short delay so the member list is actually populated.
        StartCoroutine(DelayedLobbyRefresh());
        if (isHost) {
            Debug.Log("[LobbyManager] Host entered own lobby — skipping ConnectToHost.");
            return;
        }
        Debug.Log($"[LobbyManager] Joiner — connecting to host SteamId={lobby.Owner.Id}");
        ConnectToHost(lobby.Owner.Id);
    }

    System.Collections.IEnumerator DelayedLobbyRefresh() {
        yield return new UnityEngine.WaitForSeconds(0.5f);
        if (CurrentLobby.HasValue) {
            int members = CurrentLobby.Value.MemberCount;
            Debug.Log($"[LobbyManager] DelayedLobbyRefresh — MemberCount={members} Owner={CurrentLobby.Value.Owner.Name}");
            FireLobbyUpdated("DelayedLobbyRefresh");
        } else {
            Debug.LogWarning("[LobbyManager] DelayedLobbyRefresh — CurrentLobby is no longer set, skipping.");
        }
    }

    void HandleJoinRequested(Lobby lobby, SteamId friendId) {
        Debug.Log($"[LobbyManager] HandleJoinRequested — lobby={lobby.Id} friend={friendId}");
        _ = JoinLobby(lobby.Id);
    }

    void HandleMemberJoined(Lobby lobby, Friend friend) {
        Debug.Log($"[LobbyManager] HandleMemberJoined — {friend.Name} (id={friend.Id}) joined lobby {lobby.Id}. Total members: {lobby.MemberCount}");
        FireLobbyUpdated("HandleMemberJoined");
    }

    void HandleMemberLeft(Lobby lobby, Friend friend) {
        Debug.Log($"[LobbyManager] HandleMemberLeft — {friend.Name} left lobby {lobby.Id}. Remaining members: {lobby.MemberCount}");
        FireLobbyUpdated("HandleMemberLeft");
    }

    void FireLobbyUpdated(string source) {
        int listenerCount = OnLobbyUpdated?.GetInvocationList().Length ?? 0;
        Debug.Log($"[LobbyManager] OnLobbyUpdated fired from {source} — {listenerCount} listener(s). CurrentLobby.HasValue={CurrentLobby.HasValue}");
        OnLobbyUpdated?.Invoke();
    }

    // --- Internal ---

    void ConnectToHost(SteamId hostId) {
        Debug.Log($"[LobbyManager] ConnectToHost — targetSteamId={hostId}");
        SubscribeNGO();
        _transport.targetSteamId = hostId;
        bool started = NetworkManager.Singleton.StartClient();
        Debug.Log($"[LobbyManager] StartClient() returned: {started}");
        if (!started) Debug.LogError("[LobbyManager] StartClient() failed.");
    }

    void SubscribeNGO() {
        Debug.Log("[LobbyManager] Subscribing NGO callbacks.");
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

    void OnServerStarted()              => Debug.Log("[LobbyManager] NGO server started.");
    void OnClientConnected(ulong id)    => Debug.Log($"[LobbyManager] NGO client connected — clientId={id}");
    void OnClientDisconnected(ulong id) => Debug.Log($"[LobbyManager] NGO client disconnected — clientId={id}");
}
