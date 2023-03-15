using Unity.Netcode;
using UnityEngine;

public class ModelSynchronizer : MonoBehaviour {
    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle);
    }
}
