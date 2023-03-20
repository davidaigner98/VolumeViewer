using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ModelSynchronizer : NetworkBehaviour {
    private ModelTransformator transformator;
    private GameObject displayCenter;

    public void Start() {
        transformator = GameObject.Find("ModelManager").GetComponent<ModelTransformator>();
        displayCenter = transform.parent.gameObject;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle, Space.World);
    }

    [ClientRpc]
    public void ChangeAttachmentModeClientRpc(bool attached) {
        ChangeModelAttachment(attached);
    }

    public void ChangeModelAttachment(bool attached) {
        if (transformator.attached != attached) {
            transformator.SetAttachedState(attached);

            if (attached) {
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
    public void ChangeAttachmentLabelServerRpc(bool attached) {
        transformator.SetAttachedState(attached);

        GameObject attachmentButtonTextGO = GameObject.Find("DisplayCamera(Clone)/DisplayCanvas/AttachmentButton/Text (TMP)");
        attachmentButtonTextGO.GetComponent<TextMeshProUGUI>();
        if (attached) {
            attachmentButtonText.text = "Detach";
        } else {
            attachmentButtonText.text = "Attach";
        }
    }
}
