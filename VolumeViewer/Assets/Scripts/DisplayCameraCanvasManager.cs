using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCameraCanvasManager : MonoBehaviour {
    public static DisplayCameraCanvasManager Instance { get; private set; }
    public Button detachButton;
    public TextMeshProUGUI detachButtonText;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        ModelManager.Instance.attached.OnValueChanged += RefreshAttachmentButtonText;
    }

    public void AlignCoronal() {
        GameObject selectedModel = ModelManager.Instance.GetSelectedModel();

        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().AlignCoronal(); }
    }

    public void AlignSagittal() {
        GameObject selectedModel = ModelManager.Instance.GetSelectedModel();

        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().AlignSagittal(); }
    }

    public void AlignAxial() {
        GameObject selectedModel = ModelManager.Instance.GetSelectedModel();

        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().AlignAxial(); }
    }

    public void ToggleAttachmentMode() {
        ModelManager.Instance.attached.Value = !ModelManager.Instance.attached.Value;
    }

    private void RefreshAttachmentButtonText(bool prev, bool current) {
        if (current) {
            detachButtonText.text = "Detach";
        } else {
            detachButtonText.text = "Attach";
        }
    }
}
