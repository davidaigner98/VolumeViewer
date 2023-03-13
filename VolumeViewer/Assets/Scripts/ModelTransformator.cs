using UnityEngine;

public class ModelTransformator : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void ExtractModelFromDisplay(float distance) {
        if (distance < 0) { distance = 0; }
        else if (distance > 1) { distance = 1; }

        if (distance > 0.05) {
            GetComponent<MeshRenderer>().enabled = true;
        }

        transform.localPosition = new Vector3(0, 0, -distance);
        transform.localScale = new Vector3(1, 1, distance);
    }
}
