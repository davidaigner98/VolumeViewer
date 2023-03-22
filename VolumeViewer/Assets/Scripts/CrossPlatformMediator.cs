using Unity.Netcode;

public class CrossPlatformMediator : NetworkBehaviour{
    public static CrossPlatformMediator Instance { get; private set; }
    public bool isServer;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeAttachmentButtonInteractabilityServerRpc(bool interactable) {
        if (isServer) {
            DisplayCameraCanvasManager.Instance.detachButton.interactable = interactable;
        }
    }
}
