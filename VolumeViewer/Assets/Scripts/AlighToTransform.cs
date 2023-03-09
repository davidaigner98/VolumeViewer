using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlighToTransform : MonoBehaviour {
    public Transform alignTo;
    
    // Start is called before the first frame update
    void Start() {
        transform.eulerAngles = new Vector3(0, alignTo.eulerAngles.y, 0);
        transform.position = alignTo.position + alignTo.TransformDirection(new Vector3(0, 0, -2));
    }
}
