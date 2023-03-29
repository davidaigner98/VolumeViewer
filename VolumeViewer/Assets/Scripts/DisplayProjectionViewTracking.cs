using UnityEngine;

public class DisplayProjectionViewTracking : MonoBehaviour {
    public GameObject xrCamera;

    void Update() {
        if (!CrossPlatformMediator.Instance.isServer && !CrossPlatformMediator.Instance.isInLobby) {
            Vector3 displayCenterPosition = DisplayProfileManager.Instance.GetCurrentDisplayCenter().transform.position;
            Vector3 cameraOffset = xrCamera.transform.position - displayCenterPosition;

            CrossPlatformMediator.Instance.SynchronizeCameraPositionServerRpc(cameraOffset);
        }
    }
}
