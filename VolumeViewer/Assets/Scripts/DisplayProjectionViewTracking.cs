using UnityEngine;

public class DisplayProjectionViewTracking : MonoBehaviour {
    public GameObject xrCamera;
    private float time = 0;

    // tracking of the clientside HMD
    void Update() {
        if (!CrossPlatformMediator.Instance.isServer && !CrossPlatformMediator.Instance.isInLobby) {
            // track every 20ms, 50 times per second
            if (time >= 0.02) {
                // calculate offset between HMD and display center
                GameObject displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
                Vector3 cameraOffset = xrCamera.transform.position - displayCenter.transform.position;
                cameraOffset = displayCenter.transform.InverseTransformDirection(cameraOffset);
                cameraOffset /= DisplayProfileManager.Instance.GetCurrentDisplaySize().transform.localScale.x;
                
                // trigger synchronization call
                CrossPlatformMediator.Instance.SynchronizeCameraPositionServerRpc(cameraOffset);
                time = 0;
            }

            time += Time.deltaTime;
        }
    }
}
