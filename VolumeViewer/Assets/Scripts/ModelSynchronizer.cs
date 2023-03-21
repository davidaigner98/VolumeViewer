using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ModelSynchronizer : NetworkBehaviour {
    private ModelTransformator transformator;
    private GameObject displayCenter;
    public NetworkVariable<bool> attached = new NetworkVariable<bool>(true);

    public void Start() {
        transformator = GameObject.Find("ModelManager").GetComponent<ModelTransformator>();
        displayCenter = transform.parent.gameObject;
        attached.OnValueChanged += ChangeModelAttachment;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle, Space.World);
    }

    public void ChangeModelAttachment(bool prev, bool current) {
        ChangeModelAttachment();
    }

    public void ChangeModelAttachment() {
        if (displayCenter != null) {
            if (attached.Value) {
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

    [ServerRpc(RequireOwnership = false)]
    public void ChangeAttachmentButtonInteractabilityServerRpc(bool interactable) {
        GameObject attachmentButtonGO = GameObject.Find("DisplayCamera(Clone)/DisplayCanvas/AttachmentButton");
        Button attachmentButton = attachmentButtonGO.GetComponent<Button>();
        attachmentButton.interactable = interactable;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
