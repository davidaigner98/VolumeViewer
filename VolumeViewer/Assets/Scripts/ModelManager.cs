using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModelManager : NetworkBehaviour {
    public static ModelManager Instance { get; private set; }
    public List<GameObject> modelPrefabs = new List<GameObject>();
    public NetworkVariable<GameObject[]> models = new NetworkVariable<GameObject[]>();
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

        List<GameObject> modelList = new List<GameObject>(models.Value);
        modelList.Add(model);
        models.Value = modelList.ToArray();
    }

    public GameObject GetSelectedModel() {
        return models.Value[0];
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
