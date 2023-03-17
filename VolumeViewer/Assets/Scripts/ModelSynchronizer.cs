using Unity.Netcode;
using UnityEngine;

public class ModelSynchronizer : NetworkBehaviour {
    private GameObject displayCenter;

    public void Start() {
        displayCenter = transform.parent.gameObject;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle, Space.World);
    }

    [ClientRpc]
    public void ChangeAttachmentModeClientRpc(bool attached) {
        if (attached && displayCenter != null) {
            transform.SetParent(displayCenter.transform, false);
        } else {
            transform.SetParent(null, false);
        }
    }
}
