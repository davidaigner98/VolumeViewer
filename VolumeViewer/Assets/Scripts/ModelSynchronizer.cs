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
        if (this.attached.Value != attached) {
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

    /*[ServerRpc(RequireOwnership = false)]
    public void ChangeAttachmentLabelServerRpc(bool attached) {
        GameObject attachmentButtonTextGO = GameObject.Find("DisplayCamera(Clone)/DisplayCanvas/AttachmentButton/Text (TMP)");
        TextMeshProUGUI attachmentButtonText = attachmentButtonTextGO.GetComponent<TextMeshProUGUI>();
        if (attached) {
            attachmentButtonText.text = "Detach";
        } else {
            attachmentButtonText.text = "Attach";
        }
    }*/

    [ServerRpc(RequireOwnership = false)]
    public void ChangeAttachmentButtonInteractabilityServerRpc(bool interactable) {
        GameObject attachmentButtonGO = GameObject.Find("DisplayCamera(Clone)/DisplayCanvas/AttachmentButton");
        Button attachmentButton = attachmentButtonGO.GetComponent<Button>();
        attachmentButton.interactable = interactable;
    }
}
