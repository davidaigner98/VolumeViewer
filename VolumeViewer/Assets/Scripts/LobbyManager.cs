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
    public GameObject displayCamera;
    public GameObject clippingBoxPrefab;
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
        ModelManager.Instance.SpawnDefaultModel();
        CrossPlatformMediator.Instance.isServer = true;
        CrossPlatformMediator.Instance.isInLobby = false;

        displayInputManager.SetActive(true);
        Destroy(DetectorManager.Instance.gameObject);
        Destroy(displayProjection);
        Destroy(gameObject);
    }

    public void StartClient() {
        errorLabel.enabled = false;
        CrossPlatformMediator.Instance.isServer = false;
        networkManager.OnClientConnectedCallback += ClientConnectionSuccess;
        networkManager.OnClientDisconnectCallback += ClientConnectionFailure;

        networkManager.StartClient();
        Destroy(displayInputManager);
    }

    private void ClientConnectionSuccess(ulong clientId) {
        networkManager.OnClientDisconnectCallback -= ClientConnectionFailure;
        SpawnClippingBox();
        CrossPlatformMediator.Instance.isInLobby = false;
        Destroy(gameObject);
    }

    private void ClientConnectionFailure(ulong clientId) {
        errorLabel.enabled = true;
    }

    private GameObject ReplaceXRRigWithDisplayCamera() {
        Destroy(interactionManager);
        Destroy(serviceProvider);
        Destroy(xrRig);
        GameObject newCamera = Instantiate(displayCamera);
        newCamera.transform.position = displayCameraPosition;
        newCamera.transform.LookAt(Vector3.zero);

        return newCamera;
    }

    private void SpawnClippingBox() {
        GameObject clippingBox = Instantiate(clippingBoxPrefab);
        GameObject displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
        Vector3 displaySize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform.localScale;
        Vector3 boxPosition = new Vector3(-1, 0, 1) * displaySize.x / 2;
        Vector3 boxSize = Vector3.one * displaySize.x / 4;

        clippingBox.name = "ClippingBox";
        clippingBox.transform.SetParent(displayCenter.transform);
        clippingBox.transform.position = Vector3.zero;
        clippingBox.transform.localRotation = Quaternion.identity;
        clippingBox.transform.SetParent(null);
        clippingBox.GetComponent<ClippingBox>().minBounds = boxPosition - boxSize / 2;
        clippingBox.GetComponent<ClippingBox>().maxBounds = boxPosition + boxSize / 2;
        ClippingBox.Instance.Setup();
    }
}
