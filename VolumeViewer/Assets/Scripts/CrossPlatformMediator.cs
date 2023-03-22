using Unity.Netcode;

public class CrossPlatformMediator : NetworkBehaviour{
    public static CrossPlatformMediator Instance { get; private set; }

    void Start() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeAttachmentButtonInteractabilityServerRpc(bool interactable) {
        DisplayCameraCanvasManager.Instance.detachButton.interactable = interactable;
    }
}
