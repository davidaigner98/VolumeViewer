using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayLocalizer : MonoBehaviour {
    public static DisplayLocalizer Instance { get; private set; }
    public Camera displayCamera;

    private void Awake() {
        displayCamera = GetComponent<Camera>();

        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    public Vector2 GetRelativeScreenOffset(GameObject model) {
        Vector3 modelPosition3D = displayCamera.WorldToViewportPoint(model.transform.position);
        Vector2 modelPosition2D = new Vector2(-modelPosition3D.x, modelPosition3D.y);
        return modelPosition2D - Vector2.one / 2;
    }
}
