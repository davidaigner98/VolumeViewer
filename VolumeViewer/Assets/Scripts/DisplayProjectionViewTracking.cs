using UnityEngine;

public class DisplayProjectionViewTracking : MonoBehaviour {
    public GameObject xrCamera;

    void Update() {
        if (!CrossPlatformMediator.Instance.isServer && !CrossPlatformMediator.Instance.isInLobby) {
            GameObject displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
            Vector3 cameraOffset = xrCamera.transform.position - displayCenter.transform.position;
            cameraOffset = displayCenter.transform.TransformDirection(cameraOffset);

            CrossPlatformMediator.Instance.SynchronizeCameraPositionServerRpc(cameraOffset);
        }
    }
}
