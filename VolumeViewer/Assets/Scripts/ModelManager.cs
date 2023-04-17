using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModelManager : NetworkBehaviour {
    public static ModelManager Instance { get; private set; }
    public List<GameObject> modelPrefabs = new List<GameObject>();
    public List<ModelInfo> modelInfos = new List<ModelInfo>();
    private static int modelCount = 0;
    private NetworkVariable<int> selectedModelInstanceId = new NetworkVariable<int>(-1);
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
        model.GetComponent<NetworkObject>().Spawn();
    }

    public void RegisterModel(ModelInfo info) {
        info.modelInstanceId = modelCount;
        info.gameObject.name = info.modelInstanceId + "_" + info.modelName;
        modelInfos.Add(info);
        modelCount++;

        if (CrossPlatformMediator.Instance.isServer) { ModelListUIManager.Instance.AddEntry(info.modelInstanceId, info.gameObject.name); }
    }

    public void UnregisterModel(ModelInfo info) {
        if (selectedModelInstanceId.Value == info.modelInstanceId) { SetSelectedModelClientside(null); }
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

    public GameObject GetSelectedModel() {
        if (selectedModelInstanceId.Value > -1) {
            foreach (ModelInfo currInfo in modelInfos) {
                if (currInfo.modelInstanceId == selectedModelInstanceId.Value) {
                    return currInfo.gameObject;
                }
            }
        }

        return null;
    }

    public void SetSelectedModelServerside(ModelInfo newSelectedModel) {
        GameObject currSelectedModel = GetSelectedModel();
        if (currSelectedModel != null) {
            Material[] mats = currSelectedModel.transform.Find("Model").GetComponent<Renderer>().materials;
            foreach (Material mat in mats) {
                mat.SetInt("_IsSelected", 0);
            }
        }

        if (newSelectedModel == null) {
            selectedModelInstanceId.Value = -1;
        } else {
            selectedModelInstanceId.Value = newSelectedModel.modelInstanceId;

            currSelectedModel = newSelectedModel.gameObject;
            if (currSelectedModel != null) {
                Material[] mats = currSelectedModel.transform.Find("Model").GetComponent<Renderer>().materials;
                foreach (Material mat in mats) {
                    mat.SetInt("_IsSelected", 1);
                }
            }
        }

        if (OnSelectionChanged != null) { OnSelectionChanged(); }
    }

    public void SetSelectedModelClientside(ModelInfo newSelectedModel) {
        GameObject currSelectedModel = GetSelectedModel();
        if (currSelectedModel != null) {
            Material[] mats = currSelectedModel.transform.Find("Model").GetComponent<Renderer>().materials;
            foreach (Material mat in mats) {
                mat.SetInt("_IsSelected", 0);
            }
        }

        if (newSelectedModel == null) {
            SetSelectedModelServerRpc(-1);
        } else {
            SetSelectedModelServerRpc(newSelectedModel.modelInstanceId);

            currSelectedModel = newSelectedModel.gameObject;
            if (currSelectedModel != null) {
                Material[] mats = currSelectedModel.transform.Find("Model").GetComponent<Renderer>().materials;
                foreach (Material mat in mats) {
                    mat.SetInt("_IsSelected", 1);
                }
            }
        }

        if (OnSelectionChanged != null) { OnSelectionChanged(); }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSelectedModelServerRpc(int newModelInstanceId) {
        if (newModelInstanceId == -1) { SetSelectedModelServerside(null); }
        
        foreach(ModelInfo currInfo in modelInfos) {
            if (currInfo.modelInstanceId == newModelInstanceId) {
                SetSelectedModelServerside(currInfo);
                return;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
