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

    // resets the selected model to the center (0, 0, 0)
    public void ResetToCenter() {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();

        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().ResetToCenter(); }
    }

    // aligns the selected model with its front to the camera
    public void AlignCoronal() {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();

        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().AlignCoronal(); }
    }

    // aligns the selected model with its side to the camera
    public void AlignSagittal() {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();

        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().AlignSagittal(); }
    }

    // aligns the selected model with its top to the camera
    public void AlignAxial() {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();

        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().AlignAxial(); }
    }

    // toggles the flag for model attachment in the model manager
    public void ToggleAttachmentMode() {
        ModelManager.Instance.attached.Value = !ModelManager.Instance.attached.Value;
    }

    // updates the attach button text
    private void RefreshAttachmentButtonText(bool prev, bool current) {
        if (current) {
            detachButtonText.text = "Detach";
        } else {
            detachButtonText.text = "Attach";
        }
    }
}
