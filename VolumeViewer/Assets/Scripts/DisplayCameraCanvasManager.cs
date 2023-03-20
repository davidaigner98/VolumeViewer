using TMPro;
using UnityEngine;

public class DisplayCameraCanvasManager : MonoBehaviour {
    private ModelTransformator modelTransformator;
    private ModelSynchronizer modelSynchronizer;
    public TextMeshProUGUI detachButtonText;

    public void Start() {
        modelTransformator = GameObject.Find("ModelManager").GetComponent<ModelTransformator>();
        modelSynchronizer = modelTransformator.currentModel.GetComponent<ModelSynchronizer>();
        AlignCoronal();
    }

    public void AlignCoronal() {
        modelTransformator.AlignCoronal();
    }

    public void AlignSagittal() {
        modelTransformator.AlignSagittal();
    }

    public void AlignAxial() {
        modelTransformator.AlignAxial();
    }

    public void ToggleAttachmentMode() {
        bool attached = !modelTransformator.attached;
        modelTransformator.SetAttachedState(attached);
        modelSynchronizer.ChangeAttachmentModeClientRpc(attached);

        if (attached) {
            detachButtonText.text = "Detach";
        } else {
            detachButtonText.text = "Attach";
        }
    }
}
