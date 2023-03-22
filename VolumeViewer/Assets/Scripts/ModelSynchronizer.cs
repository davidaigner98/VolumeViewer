using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ModelSynchronizer : NetworkBehaviour {
    private ModelTransformator transformator;
    public NetworkVariable<bool> attached = new NetworkVariable<bool>(true);

    public void Start() {
        transformator = GameObject.Find("ModelManager").GetComponent<ModelTransformator>();
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
        GameObject displayProjection = GameObject.Find("DisplayProjection");
        if (displayProjection != null) {
            GameObject displayCenter = displayProjection.GetComponent<DisplayProfileManager>().GetCurrentDisplayCenter();

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

    [ServerRpc(RequireOwnership = false)]
    public Vector2 GetRelativeScreenOffsetServerRpc() {
        return ModelLocalizer.Instance.GetRelativeScreenOffsetOfModel(this.gameObject);
    }
}
