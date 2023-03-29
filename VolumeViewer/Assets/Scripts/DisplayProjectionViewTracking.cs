using UnityEngine;

public class DisplayProjectionViewTracking : MonoBehaviour {
    public GameObject xrCamera;
    
    void Start() {
        
    }

    void Update() {
        Vector3 cameraOffset = xrCamera.transform.position - transform.position;

        CrossPlatformMediator.Instance.xrCameraOffset.Value = cameraOffset;
    }
}
