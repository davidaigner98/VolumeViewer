using UnityEngine;

public class ModelInfo : MonoBehaviour {
    // model information properties
    public int modelId;
    public int modelInstanceId;
    public string modelName;
    public string description;
    public Sprite icon;

    // register this model on start
    private void Start() {
        ModelManager.Instance.RegisterModel(this);
    }

    // unregister this model on destroy
    private void OnDestroy() {
        ModelManager.Instance.UnregisterModel(this);
    }
}
