using UnityEngine;
using Unity.Netcode;
using TMPro;

public class LobbyManager : MonoBehaviour {
    public NetworkManager networkManager;
    public GameObject serviceProvider;
    public GameObject interactionManager;
    public GameObject displayInputManager;
    public GameObject xrRig;
    public GameObject displayProjection;
    public GameObject lightsource;
    public GameObject displayCamera;
    public Vector3 displayCameraPosition;
    public TextMeshProUGUI errorLabel;
    public bool manualServerStart;
    public bool manualClientStart;

    private void Update() {
        if (manualServerStart) {
            manualServerStart = false;
            StartServer();
        } else if (manualClientStart) {
            manualClientStart = false;
            StartClient();
        }
    }

    public void StartServer() {
        networkManager.StartServer();
        GameObject newCamera = ReplaceXRRigWithDisplayCamera();
        CrossPlatformMediator.Instance.isServer = true;
        CrossPlatformMediator.Instance.isInLobby = false;
        ModelManager.Instance.SpawnDefaultModel();
        
        displayInputManager.SetActive(true);
        Destroy(DetectorManager.Instance.gameObject);
        Destroy(displayProjection);
        Destroy(gameObject);
    }

    public void StartClient() {
        errorLabel.enabled = false;
        CrossPlatformMediator.Instance.isServer = false;
        CrossPlatformMediator.Instance.isInLobby = false;
        networkManager.OnClientConnectedCallback += ClientConnectionSuccess;
        networkManager.OnClientDisconnectCallback += ClientConnectionFailure;

        networkManager.StartClient();

        Destroy(displayInputManager);
    }

    private void ClientConnectionSuccess(ulong clientId) {
        networkManager.OnClientDisconnectCallback -= ClientConnectionFailure;
        Destroy(gameObject);
    }

    private void ClientConnectionFailure(ulong clientId) {
        errorLabel.enabled = true;
    }

    private GameObject ReplaceXRRigWithDisplayCamera() {
        Destroy(interactionManager);
        Destroy(serviceProvider);
        Destroy(xrRig);
        GameObject newCamera = GameObject.Instantiate(displayCamera);
        newCamera.transform.position = displayCameraPosition;
        newCamera.transform.LookAt(Vector3.zero);
        lightsource.transform.SetParent(newCamera.transform);

        return newCamera;
    }
}
