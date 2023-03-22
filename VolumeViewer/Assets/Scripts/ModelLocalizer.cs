using UnityEngine;

public class ModelLocalizer : MonoBehaviour {
    public static ModelLocalizer Instance { get; private set; }
    private Camera displayCamera;

    void Start() {
        if (Instance != null && Instance != this) { Destroy(this); } 
        else { Instance = this; }

        displayCamera = GetComponent<Camera>();
    }

    public Vector2 GetRelativeScreenOffsetOfModel(GameObject model) {
        Vector3 modelPosition3D = displayCamera.WorldToViewportPoint(model.transform.position);
        Vector2 modelPosition2D = new Vector2(modelPosition3D.x, modelPosition3D.y);
        return modelPosition2D - Vector2.one / 2;
    }
}
