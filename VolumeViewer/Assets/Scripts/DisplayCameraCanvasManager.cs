using TMPro;
using UnityEngine;

public class DisplayCameraCanvasManager : MonoBehaviour {
    private ModelTransformator transformator;
    private ModelSynchronizer synchronizer;
    public TextMeshProUGUI detachButtonText;

    public void Start() {
        transformator = GameObject.Find("ModelManager").GetComponent<ModelTransformator>();
        synchronizer = transformator.currentModel.GetComponent<ModelSynchronizer>();
        synchronizer.attached.OnValueChanged += RefreshAttachmentButtonText;
        AlignCoronal();
    }

    public void AlignCoronal() {
        transformator.AlignCoronal();
    }

    public void AlignSagittal() {
        transformator.AlignSagittal();
    }

    public void AlignAxial() {
        transformator.AlignAxial();
    }

    public void ToggleAttachmentMode() {
        bool attached = !synchronizer.attached.Value;
        synchronizer.SetAttachedStateServerRpc(attached);
    }

    private void RefreshAttachmentButtonText(bool prev, bool current) {
        if (synchronizer.attached.Value) {
            detachButtonText.text = "Detach";
        } else {
            detachButtonText.text = "Attach";
        }
    }
}
