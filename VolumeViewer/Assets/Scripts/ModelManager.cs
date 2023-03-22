using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModelManager : NetworkBehaviour {
    public static ModelManager Instance { get; private set; }
    public List<GameObject> models = new List<GameObject>();
    public NetworkVariable<bool> attached = new NetworkVariable<bool>(true);

    private void Start() {
        if (Instance != null && Instance != this) { Destroy(this); } 
        else { Instance = this; }
    }

    public void SpawnDefaultModel() {
        
    }

    private void SpawnModel(GameObject modelPrefab) {
        GameObject model = GameObject.Instantiate(modelPrefab);
        models.Add(model);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttachedStateServerRpc(bool attached) {
        this.attached.Value = attached;
    }
}
