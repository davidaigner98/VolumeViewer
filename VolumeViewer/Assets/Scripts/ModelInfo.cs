using UnityEngine;

public class ModelInfo : MonoBehaviour {
    public int modelId;
    public int modelInstanceId;
    public string modelName;
    public string description;

    private void Start() {
        ModelManager.Instance.RegisterModel(this);
    }

    private void OnDestroy() {
        ModelManager.Instance.UnregisterModel(this);
    }
}
