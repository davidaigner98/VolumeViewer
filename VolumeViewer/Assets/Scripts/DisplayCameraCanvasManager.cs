using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCameraCanvasManager : MonoBehaviour {
    private ModelTransformator modelTransformator;
    public TextMeshProUGUI detachButtonText;

    public void Start() {
        modelTransformator = GameObject.Find("ModelManager").GetComponent<ModelTransformator>();
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
        modelTransformator.ChangeAttachmentMode(attached);

        if (attached) {
            detachButtonText.text = "Detach";
        } else {
            detachButtonText.text = "Attach";
        }
    }
}
