using UnityEngine;
using Unity.Netcode;

public class LobbyManagement : MonoBehaviour {
    public NetworkManager networkManager;

    public void StartHost() {
        networkManager.StartHost();
        Destroy(gameObject);
    }

    public void StartClient() {
        networkManager.StartClient();
        Destroy(gameObject);
    }
}
