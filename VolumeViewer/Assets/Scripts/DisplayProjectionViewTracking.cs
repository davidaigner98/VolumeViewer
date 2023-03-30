using UnityEngine;

public class DisplayProjectionViewTracking : MonoBehaviour {
    public GameObject xrCamera;
    private int frame = 0;

    void Update() {
        if (!CrossPlatformMediator.Instance.isServer && !CrossPlatformMediator.Instance.isInLobby) {
            if (frame >= 50) {
                GameObject displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
                Vector3 cameraOffset = xrCamera.transform.position - displayCenter.transform.position;
                cameraOffset = displayCenter.transform.TransformDirection(cameraOffset);

                CrossPlatformMediator.Instance.SynchronizeCameraPositionServerRpc(cameraOffset);
                frame = 0;
            }

            frame++;
        }
    }
}
