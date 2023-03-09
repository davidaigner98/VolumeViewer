using UnityEngine;
using Unity.Netcode;

public class LobbyManagement : MonoBehaviour {
    public NetworkManager networkManager;
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
        Destroy(xrRig);
        GameObject newCamera = GameObject.Instantiate(displayCamera);
        newCamera.transform.position = model.transform.position + offsetToModelTransform;
        newCamera.transform.LookAt(model.transform);
    }
}
