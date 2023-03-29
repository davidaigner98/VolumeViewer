using UnityEngine;

public class DisplayProjectionViewTracking : MonoBehaviour {
    public GameObject xrCamera;
    
    void Start() {
        
    }

    void Update() {
        if (!CrossPlatformMediator.Instance.isServer) {
            Vector3 cameraOffset = xrCamera.transform.position - transform.position;

            CrossPlatformMediator.Instance.SynchronizeCameraPositionServerRpc(cameraOffset);
        }
    }
}
