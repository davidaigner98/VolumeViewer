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
        return new Vector2(-modelPosition3D.x + 0.5f, modelPosition3D.y - 0.5f);
    }

    public ModelInfo FindModelByRaycast(Vector2 screenCoordinates) {
        RaycastHit hitInfo;
        if (Physics.Raycast(displayCamera.ScreenPointToRay(screenCoordinates), out hitInfo)) {
            return hitInfo.collider.gameObject.GetComponent<ModelInfo>();
        }

        return null;
    }
}
