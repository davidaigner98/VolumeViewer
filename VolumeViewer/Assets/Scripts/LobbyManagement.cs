using UnityEngine;
using Unity.Netcode;
using TMPro;

public class LobbyManagement : MonoBehaviour {
    public NetworkManager networkManager;
    public GameObject serviceProvider;
    public GameObject interactionManager;
    public GameObject xrRig;
    public GameObject displayProjection;
    public GameObject lightsource;
    public GameObject displayCamera;
    public Vector3 displayCameraPosition;
    public TextMeshProUGUI errorLabel;

    public void StartServer() {
        networkManager.StartServer();
        GameObject newCamera = ReplaceXRRigWithDisplayCamera();
        CrossPlatformMediator.Instance.isServer = true;
        ModelManager.Instance.SpawnDefaultModel();

        Destroy(displayProjection);
        Destroy(gameObject);
    }

    public void StartClient() {
        errorLabel.enabled = false;
        CrossPlatformMediator.Instance.isServer = false;
        networkManager.OnClientConnectedCallback += ClientConnectionSuccess;
        networkManager.OnClientDisconnectCallback += ClientConnectionFailure;

        networkManager.StartClient();
    }

    private void ClientConnectionSuccess(ulong clientId) {
        ModelManager.Instance.SpawnDefaultModel();
        DetectorManager.Instance.UpdateDetectors();
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
        lightsource.transform.SetParent(newCamera.transform);

        return newCamera;
    }
}
