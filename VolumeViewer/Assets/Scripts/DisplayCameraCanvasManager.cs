using UnityEngine;

public class DisplayCameraCanvasManager : MonoBehaviour {
    private ModelTransformator modelTransformator;

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
        modelTransformator.ToggleAttachmentMode();
    }
}
