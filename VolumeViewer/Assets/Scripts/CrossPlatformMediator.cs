using Unity.Netcode;
using UnityEngine;

public class CrossPlatformMediator : NetworkBehaviour{
    public static CrossPlatformMediator Instance { get; private set; }
    public bool isServer;
    public bool isInLobby = true;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeAttachmentButtonInteractabilityServerRpc(bool interactable) {
        if (isServer && !isInLobby) {
            DisplayCameraCanvasManager.Instance.detachButton.interactable = interactable;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SynchronizeCameraPositionServerRpc(Vector3 cameraOffset, Quaternion cameraOrientation) {
        if (isServer && !isInLobby) {
            DisplayCameraPositioning.Instance.SynchronizeDisplayCameraPosition(cameraOffset, cameraOrientation);
        }
    }
}
