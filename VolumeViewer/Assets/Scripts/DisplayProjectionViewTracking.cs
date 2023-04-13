using UnityEngine;

public class DisplayProjectionViewTracking : MonoBehaviour {
    public GameObject xrCamera;
    private float time = 0;

    void Update() {
        if (!CrossPlatformMediator.Instance.isServer && !CrossPlatformMediator.Instance.isInLobby) {
            if (time >= 0.02) {
                GameObject displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
                Vector3 cameraOffset = xrCamera.transform.position - displayCenter.transform.position;
                cameraOffset = displayCenter.transform.InverseTransformDirection(cameraOffset);
                cameraOffset /= DisplayProfileManager.Instance.GetCurrentDisplaySize().transform.localScale.x;
                Debug.Log(cameraOffset);

                CrossPlatformMediator.Instance.SynchronizeCameraPositionServerRpc(cameraOffset);
                time = 0;
            }

            time += Time.deltaTime;
        }
    }
}
