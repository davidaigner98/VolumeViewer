using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCameraCanvasManager : MonoBehaviour {
    public static DisplayCameraCanvasManager Instance { get; private set; }
    private ModelManager modelManager;
    public Button detachButton;
    public TextMeshProUGUI detachButtonText;

    public void Start() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }

        modelManager = ModelManager.Instance;
        //synchronizer.attached.OnValueChanged += RefreshAttachmentButtonText;
        AlignCoronal();
    }

    public void AlignCoronal() {
        //transformator.AlignCoronal();
    }

    public void AlignSagittal() {
        //transformator.AlignSagittal();
    }

    public void AlignAxial() {
        //transformator.AlignAxial();
    }

    public void ToggleAttachmentMode() {
        //synchronizer.attached.Value = !synchronizer.attached.Value;
    }

    private void RefreshAttachmentButtonText(bool prev, bool current) {
        //if (synchronizer.attached.Value) {
        //    detachButtonText.text = "Detach";
        //} else {
        //    detachButtonText.text = "Attach";
        //}
    }
}
