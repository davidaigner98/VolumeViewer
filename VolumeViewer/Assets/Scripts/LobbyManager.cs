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
    public TextMeshProUGUI errorLabel;
    public bool manualServerStart;
    public bool manualClientStart;

    private void Update() {
        // options for debug starting of server or client
        if (manualServerStart) {
            manualServerStart = false;
            StartServer();
        } else if (manualClientStart) {
            manualClientStart = false;
            StartClient();
        }
    }

    // starts this instance as a server
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

    // starts this instance as a client
    public void StartClient() {
        errorLabel.enabled = false;
        CrossPlatformMediator.Instance.isServer = false;
        networkManager.OnClientConnectedCallback += ClientConnectionSuccess;
        networkManager.OnClientDisconnectCallback += ClientConnectionFailure;
        GameObject.Find("DirectionalLight").transform.eulerAngles = new Vector3(90, 0, 0);

        networkManager.StartClient();
        Destroy(displayInputManager);
    }

    // on successful client connection
    private void ClientConnectionSuccess(ulong clientId) {
        networkManager.OnClientDisconnectCallback -= ClientConnectionFailure;
        SpawnClippingBox();
        CrossPlatformMediator.Instance.isInLobby = false;
        Destroy(gameObject);
    }

    // on failed client connection
    private void ClientConnectionFailure(ulong clientId) {
        errorLabel.enabled = true;
    }

    // replaces the initial xr rig with a serverside display camera
    private GameObject ReplaceXRRigWithDisplayCamera() {
        Destroy(interactionManager);
        Destroy(serviceProvider);
        Destroy(xrRig);
        return Instantiate(displayCamera);
    }

    // spawns the clipping box
    private void SpawnClippingBox() {
        if (ClippingBox.Instance != null) { return; }

        // setup clipping box
        GameObject clippingBox = Instantiate(clippingBoxPrefab);
        GameObject displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
        clippingBox.name = "ClippingBox";
        clippingBox.transform.SetParent(displayCenter.transform);
        clippingBox.transform.position = Vector3.zero;
        clippingBox.transform.localRotation = Quaternion.identity;

        // reposition and resize clipping box
        Vector3 displaySize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform.localScale;
        Vector3 boxPosition = -clippingBox.transform.localPosition + displayCenter.transform.TransformDirection(new Vector3(-1, 0, 0.5f) * displaySize.x / 2);
        Vector3 boxSize = Vector3.one * displaySize.x / 4;
        clippingBox.GetComponent<ClippingBox>().minBounds = boxPosition - boxSize / 2;
        clippingBox.GetComponent<ClippingBox>().maxBounds = boxPosition + boxSize / 2;
        ClippingBox.Instance.Setup();
    }
}
