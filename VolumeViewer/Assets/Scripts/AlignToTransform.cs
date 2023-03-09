using UnityEngine;

public class AlignToTransform : MonoBehaviour {
    public Transform alignTo;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start() {
        transform.eulerAngles = new Vector3(0, alignTo.eulerAngles.y, 0);
        transform.position = alignTo.position + alignTo.TransformDirection(offset);
    }
}
