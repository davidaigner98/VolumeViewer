using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCameraCanvasManager : MonoBehaviour {
    public static DisplayCameraCanvasManager Instance { get; private set; }
    public Button detachButton;
    public TextMeshProUGUI detachButtonText;
    public Camera displayCamera;

    private void Awake() {
        displayCamera = GetComponent<Camera>();

        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        ModelManager.Instance.attached.OnValueChanged += RefreshAttachmentButtonText;
    }

    public void AlignCoronal() {
        ModelManager.Instance.GetSelectedModel().GetComponent<ModelTransformator>().AlignCoronal();
    }

    public void AlignSagittal() {
        ModelManager.Instance.GetSelectedModel().GetComponent<ModelTransformator>().AlignSagittal();
    }

    public void AlignAxial() {
        ModelManager.Instance.GetSelectedModel().GetComponent<ModelTransformator>().AlignAxial();
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
