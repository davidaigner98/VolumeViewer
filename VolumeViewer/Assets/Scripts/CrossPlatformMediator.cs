using Unity.Netcode;
using UnityEngine;

public class CrossPlatformMediator : NetworkBehaviour{
    public static CrossPlatformMediator Instance { get; private set; }
    public bool isServer;
    public bool isInLobby = true;
    public string clientMode;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    // client to server call for changing the attachment button interactability
    [ServerRpc(RequireOwnership = false)]
    public void ChangeAttachmentButtonInteractabilityServerRpc(bool interactable) {
        if (isServer && !isInLobby) {
            DisplayCameraCanvasManager.Instance.detachButton.interactable = interactable;
        }
    }

    // client to server call for updating the position of the display camera
    [ServerRpc(RequireOwnership = false)]
    public void SynchronizeCameraPositionServerRpc(Vector3 cameraOffset) {
        if (isServer && !isInLobby) {
            DisplayCameraPositioning.Instance.SynchronizeDisplayCameraPosition(cameraOffset);
        }
    }
}
