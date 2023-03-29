using UnityEngine;

public class DisplayProjectionViewTracking : MonoBehaviour {
    public GameObject xrCamera;

    void Update() {
        if (!CrossPlatformMediator.Instance.isServer && !CrossPlatformMediator.Instance.isInLobby) {
            Vector3 cameraOffset = xrCamera.transform.position - transform.position;

            CrossPlatformMediator.Instance.xrCameraOffset.Value = cameraOffset;
        }
    }
}
