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

    // spawns a given model
    public void SpawnModel(GameObject modelPrefab) {
        GameObject model = Instantiate(modelPrefab);
        model.transform.position = Vector3.forward;
        model.GetComponent<NetworkObject>().Spawn();
    }

    // registers a model after spawning (triggered by model itself, see ModelInfo class)
    public void RegisterModel(ModelInfo info) {
        info.modelInstanceId = modelCount;
        info.gameObject.name = info.modelInstanceId + "_" + info.modelName;
        modelInfos.Add(info);
        modelCount++;

        // add new entry to model list
        if (CrossPlatformMediator.Instance.isServer) {
            if (selectedModel == null) { SetSelectedModel(info); }
            ModelListUIManager.Instance.AddEntry(info);
        }
    }

    // unregister model on deletion/destroying (triggered by model itself, see ModelInfo class)
    public void UnregisterModel(ModelInfo info) {
        if (selectedModel != null && selectedModel.modelInstanceId == info.modelInstanceId) { SetSelectedModel(null); }
        modelInfos.Remove(info);
    }

    // deletes a model, triggered by the button in the model list
    public void DeleteModel(int instanceId) {
        foreach (ModelInfo info in modelInfos) {
            if (info.modelInstanceId == instanceId) {
                modelInfos.Remove(info);
                Destroy(info.gameObject);

                return;
            }
        }
    }

    // returns currently selected model
    public ModelInfo GetSelectedModel() {
        return selectedModel;
    }

    // sets the selected model
    public void SetSelectedModel(ModelInfo newSelectedModel) {
        // deactivates the selection outline shader on newly selected model
        SetModelSelectionInShader(selectedModel, 0);

        selectedModel = newSelectedModel;

        // changes text color in model list
        if (CrossPlatformMediator.Instance.isServer && selectedModel != null) {
            ModelListUIManager.Instance.ChangeSelectedText(selectedModel.name);
        }

        // activates the selection outline shader on newly selected model
        SetModelSelectionInShader(selectedModel, 1);

        // fire OnSelectionChanged event
        if (OnSelectionChanged != null) {
            OnSelectionChanged();
        }
    }

    // changes shader properties for selection outline shader pass
    private void SetModelSelectionInShader(ModelInfo model, int selectionStatus) {
        if (CrossPlatformMediator.Instance.isServer &&  model != null) {
            Material[] mats = model.transform.Find("Model").GetComponent<Renderer>().materials;
            foreach (Material mat in mats) {
                mat.SetInt("_IsSelected", selectionStatus);
            }
        }
    }

    // sets the state of the attached button
    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
