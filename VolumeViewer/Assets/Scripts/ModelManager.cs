using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ModelManager : NetworkBehaviour {
    public static ModelManager Instance { get; private set; }
    public List<GameObject> modelPrefabs = new List<GameObject>();
    public List<ModelInfo> modelInfos = new List<ModelInfo>();
    private static int modelCount = 0;
    private ModelInfo selectedModel;
    public NetworkVariable<bool> attached = new NetworkVariable<bool>(true);

    public delegate void SelectionChangeAction();
    public event SelectionChangeAction OnSelectionChanged;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); } 
        else { Instance = this; }
    }

    public void SpawnDefaultModel() {
        if (modelPrefabs.Count > 0) {
            SpawnModel(modelPrefabs[0]);
        }
    }

    private void SpawnModel(GameObject modelPrefab) {
        GameObject model = Instantiate(modelPrefab);
        model.GetComponent<NetworkObject>().Spawn();
    }

    public void RegisterModel(ModelInfo info) {
        info.modelInstanceId = modelCount;
        info.gameObject.name = info.modelInstanceId + "_" + info.modelName;
        modelInfos.Add(info);
        modelCount++;

        if (CrossPlatformMediator.Instance.isServer) { ModelListUIManager.Instance.AddEntry(info.modelInstanceId, info.gameObject.name); }

        if (selectedModel == null) { SetSelectedModel(info); }
    }

    public void UnregisterModel(ModelInfo info) {
        if (selectedModel.modelInstanceId == info.modelInstanceId) { SetSelectedModel(null); }
        modelInfos.Remove(info);
    }

    public void DeleteModel(int instanceId) {
        foreach (ModelInfo info in modelInfos) {
            if (info.modelInstanceId == instanceId) {
                modelInfos.Remove(info);
                Destroy(info.gameObject);

                return;
            }
        }
    }

    public GameObject GetSelectedModel() {
        if (selectedModel != null) {
            return selectedModel.gameObject;
        } else {
            return null;
        }
    }

    private void SetSelectedModel(ModelInfo newSelectedModel) {
        selectedModel = newSelectedModel;
        OnSelectionChanged();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
