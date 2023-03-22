using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModelManager : NetworkBehaviour {
    public static ModelManager Instance { get; private set; }
    public List<GameObject> modelPrefabs = new List<GameObject>();
    public List<GameObject> models = new List<GameObject>();
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

        models.Add(model);
    }

    public GameObject GetSelectedModel() {
        return models[0];
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
