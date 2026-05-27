using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

// Handles Unity Relay allocation and wires it into NetworkManager.
// Add this component to the MANAGERS prefab alongside NetworkManager + UnityTransport.
public class LobbyManager : MonoBehaviour {

    public static LobbyManager me;

    public string JoinCode { get; private set; }

    void Awake() {
        me = this;
    }

    async Task InitServices() {
        if (UnityServices.State == ServicesInitializationState.Initialized) return;
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Call from MainMenu when the player wants to host.
    // Returns the 6-character join code to display on screen.
    public async Task<string> StartHost() {
        await InitServices();

        var allocation = await RelayService.Instance.CreateAllocationAsync(1);
        JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

        NetworkManager.Singleton.StartHost();
        Debug.Log($"[LobbyManager] Hosting. Join code: {JoinCode}");
        return JoinCode;
    }

    // Call from MainMenu when the player enters a code and wants to join.
    public async Task StartClient(string code) {
        await InitServices();

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(code.Trim().ToUpper());

        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

        NetworkManager.Singleton.StartClient();
        Debug.Log($"[LobbyManager] Joined with code: {code}");
    }
}
