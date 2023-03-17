using UnityEngine;
using Unity.Netcode;

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

    public void StartHost() {
        networkManager.StartHost();
        GameObject newCamera = ReplaceXRRigWithDisplayCamera();
        modelTransformator.currentModel.AddComponent<Draggable>().displayCamera = newCamera;        
        modelTransformator.SetAlpha(1);
        modelTransformator.currentModel.transform.SetParent(null);
        modelTransformator.currentModel.transform.rotation = Quaternion.identity;
        modelTransformator.isConnected = true;

        Destroy(displayProjection);
        Destroy(modelTransformator);
        Destroy(gameObject);
    }

    public void StartClient() {
        networkManager.StartClient();
        modelTransformator.isConnected = true;
        Destroy(gameObject);
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
