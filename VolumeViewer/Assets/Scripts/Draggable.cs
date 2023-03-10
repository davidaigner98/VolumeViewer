using UnityEngine;

public class Draggable : MonoBehaviour {
    public Transform displayCameraTransform;
    public float rotSpeed = 250.0f;

    // Start is called before the first frame update
    void Start() {
        
    }

    private void OnMouseDrag() {
        float rotX = Input.GetAxis("Mouse X") * Mathf.Deg2Rad * rotSpeed;
        float rotY = Input.GetAxis("Mouse Y") * Mathf.Deg2Rad * rotSpeed;

        transform.Rotate(displayCameraTransform.TransformDirection(Vector3.up), -rotX, Space.World);
        transform.Rotate(displayCameraTransform.TransformDirection(Vector3.right), rotY, Space.World);
    }
}
