using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ModelManager : NetworkBehaviour {
    public static ModelManager Instance { get; private set; }
    public List<GameObject> modelPrefabs = new List<GameObject>();
    public NetworkVariable<string[]> models = new NetworkVariable<string[]>();
    public NetworkVariable<bool> attached = new NetworkVariable<bool>(true);

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
        ModelTransformator transformator = model.GetComponent<ModelTransformator>();
        model.GetComponent<NetworkObject>().Spawn();

        ModelInfo info = model.GetComponent<ModelInfo>();
        info.modelInstanceId = models.Value.Length;
        model.name = info.modelInstanceId + "_" + info.modelName;

        List<string> modelList = new List<string>(models.Value);
        modelList.Add(model.name);
        models.Value = modelList.ToArray();
    }

    public GameObject GetSelectedModel() {
        return GameObject.Find(models.Value[0]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
