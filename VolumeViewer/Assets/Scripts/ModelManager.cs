using System.Collections.Generic;
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

    public void SpawnModel(GameObject modelPrefab) {
        GameObject model = Instantiate(modelPrefab);
        model.transform.position = Vector3.forward;
        model.GetComponent<NetworkObject>().Spawn();
    }

    public void RegisterModel(ModelInfo info) {
        info.modelInstanceId = modelCount;
        info.gameObject.name = info.modelInstanceId + "_" + info.modelName;
        modelInfos.Add(info);
        modelCount++;

        if (CrossPlatformMediator.Instance.isServer) {
            if (selectedModel == null) { SetSelectedModel(info); }
            ModelListUIManager.Instance.AddEntry(info);
        }
    }

    public void UnregisterModel(ModelInfo info) {
        if (selectedModel != null && selectedModel.modelInstanceId == info.modelInstanceId) { SetSelectedModel(null); }
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

    public void RefreshModelScreenOffsets() {
        foreach (ModelInfo info in modelInfos) {
            ModelTransformator transformator = info.gameObject.GetComponent<ModelTransformator>();
            Vector2 screenOffset = DisplayLocalizer.Instance.GetRelativeScreenOffset(info.gameObject);
            transformator.screenOffset.Value = screenOffset;
        }
    }

    public ModelInfo GetSelectedModel() {
        return selectedModel;
    }

    public void SetSelectedModel(ModelInfo newSelectedModel) {
        SetModelSelectionInShader(selectedModel, 0);

        selectedModel = newSelectedModel;

        if (CrossPlatformMediator.Instance.isServer && selectedModel != null) {
            ModelListUIManager.Instance.ChangeSelectedText(selectedModel.name);
        }

        SetModelSelectionInShader(selectedModel, 1);

        if (OnSelectionChanged != null) {
            OnSelectionChanged();
        }
    }

    private void SetModelSelectionInShader(ModelInfo model, int selectionStatus) {
        if (CrossPlatformMediator.Instance.isServer &&  model != null) {
            Material[] mats = model.transform.Find("Model").GetComponent<Renderer>().materials;
            foreach (Material mat in mats) {
                mat.SetInt("_IsSelected", selectionStatus);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
