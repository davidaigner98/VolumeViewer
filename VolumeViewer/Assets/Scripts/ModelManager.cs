using System.Collections.Generic;
using UnityEngine;

public class ModelManager : MonoBehaviour {
    public static ModelManager Instance { get; private set; }
    private List<GameObject> models = new List<GameObject>();

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
}
