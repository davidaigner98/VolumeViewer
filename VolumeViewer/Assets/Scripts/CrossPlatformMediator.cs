using Unity.Netcode;
using UnityEngine;

public class CrossPlatformMediator : NetworkBehaviour{
    public static CrossPlatformMediator Instance { get; private set; }
    public bool isServer;
    public bool isInLobby = true;
    public NetworkVariable<Vector3> xrCameraOffset = new NetworkVariable<Vector3>(Vector3.zero);

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }

        xrCameraOffset.OnValueChanged += SynchronizeCameraPosition;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeAttachmentButtonInteractabilityServerRpc(bool interactable) {
        if (isServer) {
            DisplayCameraCanvasManager.Instance.detachButton.interactable = interactable;
        }
    }

    private void SynchronizeCameraPosition(Vector3 prev, Vector3 curr) {
        if (isServer) {
            DisplayCameraPositioning.Instance.SynchronizeDisplayCameraPosition(curr);
        }
    }
}
