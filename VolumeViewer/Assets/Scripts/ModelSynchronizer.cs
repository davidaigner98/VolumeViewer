using Unity.Netcode;
using UnityEngine;

public class ModelSynchronizer : NetworkBehaviour {
    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle, Space.World);
    }
}
