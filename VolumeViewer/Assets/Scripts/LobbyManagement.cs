using UnityEngine;
using Unity.Netcode;

public class LobbyManagement : MonoBehaviour {
    public NetworkManager networkManager;
    public GameObject serviceProvider;
    public GameObject interactionManager;
    public GameObject xrRig;
    public GameObject displayCamera;
    public GameObject model;
    public Vector3 offsetToModelTransform;

    public void StartHost() {
        networkManager.StartHost();
        model.AddComponent<Draggable>();
        ReplaceXRRigWithDisplayCamera();
        Destroy(gameObject);
    }

    public void StartClient() {
        networkManager.StartClient();
        Destroy(gameObject);
    }

    private void ReplaceXRRigWithDisplayCamera() {
        Destroy(interactionManager);
        Destroy(serviceProvider);
        Destroy(xrRig);
        GameObject newCamera = GameObject.Instantiate(displayCamera);
        newCamera.GetComponent<DisplayCameraAlignment>().model = model;
        newCamera.transform.position = model.transform.position + offsetToModelTransform;
        newCamera.transform.LookAt(model.transform);
    }
}
