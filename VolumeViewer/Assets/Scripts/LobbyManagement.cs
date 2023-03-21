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
    public ModelTransformator modelTransformator;
    public Vector3 offsetToModelTransform;
    public TextMeshProUGUI errorLabel;
    private bool connected = false;

    public void StartServer() {
        networkManager.StartServer();
        GameObject newCamera = ReplaceXRRigWithDisplayCamera();
        modelTransformator.currentModel.AddComponent<Draggable>().displayCamera = newCamera;        
        modelTransformator.SetAlpha(1);
        modelTransformator.currentModel.transform.SetParent(null);
        modelTransformator.currentModel.transform.rotation = Quaternion.identity;
        modelTransformator.isStarted = true;
        modelTransformator.isServer = true;
        modelTransformator.SetupServer();

        Destroy(displayProjection);
        Destroy(modelTransformator);
        Destroy(gameObject);
    }

    public void StartClient() {
        errorLabel.enabled = false;
        networkManager.OnClientConnectedCallback += ClientConnectionSuccess;
        networkManager.OnClientDisconnectCallback += ClientConnectionFailure;
        modelTransformator.SetupClient();

        networkManager.StartClient();
    }

    private void ClientConnectionSuccess(ulong clientId) {
        connected = true;
        modelTransformator.isStarted = true;
        Destroy(gameObject);
    }

    private void ClientConnectionFailure(ulong clientId) {
        if (!connected) {
            errorLabel.enabled = true;
        }
    }

    private GameObject ReplaceXRRigWithDisplayCamera() {
        Destroy(interactionManager);
        Destroy(serviceProvider);
        Destroy(xrRig);
        GameObject newCamera = GameObject.Instantiate(displayCamera);
        newCamera.transform.position = modelTransformator.currentModel.transform.position + offsetToModelTransform;
        newCamera.transform.LookAt(modelTransformator.currentModel.transform);
        lightsource.transform.SetParent(newCamera.transform);

        modelTransformator.currentModel.GetComponent<MeshRenderer>().enabled = true;
        return newCamera;
    }
}
