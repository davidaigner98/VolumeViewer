using UnityEngine;

public class DisplayCameraPositioning : MonoBehaviour {
    public static DisplayCameraPositioning Instance { get; private set; }
    private Vector3 startPosition;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }

        startPosition = transform.position;
    }

    public void SynchronizeDisplayCameraPosition(Vector3 offset) {
        transform.position = startPosition + offset;
    }
}
