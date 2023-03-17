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
        if (displayCenter != null) {
            if (attached) {
                transform.eulerAngles = transform.InverseTransformDirection(transform.eulerAngles);
                transform.SetParent(displayCenter.transform);
            } else {
                transform.SetParent(null);
                transform.eulerAngles = transform.TransformDirection(transform.eulerAngles);
            }
        }
    }
}
