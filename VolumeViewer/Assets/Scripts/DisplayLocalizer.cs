using UnityEngine;

public class DisplayLocalizer : MonoBehaviour {
    public static DisplayLocalizer Instance { get; private set; }
    public Camera displayCamera;

    private void Awake() {
        displayCamera = GetComponent<Camera>();

        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    // calculates the relative offset of a given model relative to the screen center
    public Vector2 GetRelativeScreenOffset(GameObject model) {
        Vector3 modelPosition3D = displayCamera.WorldToViewportPoint(model.transform.position);
        return new Vector2(modelPosition3D.x - 0.5f, modelPosition3D.y - 0.5f);
    }

    // returns a model hit by a camera raycast, originating from a given screen position 
    public ModelInfo FindModelByRaycast(Vector2 screenCoordinates) {
        RaycastHit hitInfo;
        if (Physics.Raycast(displayCamera.ScreenPointToRay(screenCoordinates), out hitInfo)) {
            return hitInfo.collider.gameObject.GetComponentInParent<ModelInfo>();
        }

        return null;
    }
}
