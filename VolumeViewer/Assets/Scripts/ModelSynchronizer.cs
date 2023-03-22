using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ModelSynchronizer : NetworkBehaviour {
    public void Start() {
        ModelManager.Instance.attached.OnValueChanged += ChangeModelAttachment;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle, Space.World);
    }

    public void ChangeModelAttachment(bool prev, bool current) {
        GameObject displayProjection = GameObject.Find("DisplayProjection");
        if (displayProjection != null) {
            GameObject displayCenter = displayProjection.GetComponent<DisplayProfileManager>().GetCurrentDisplayCenter();

            if (current) {
                GameObject modelParent = GameObject.Find("ModelParent");
                transform.SetParent(displayCenter.transform);
                Destroy(modelParent);
            } else {
                GameObject modelParent = new GameObject("ModelParent");
                modelParent.transform.rotation = displayCenter.transform.rotation;
                transform.SetParent(modelParent.transform);
            }
        }
    }
}
