using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHandCanvasUIManager : MonoBehaviour {
    private GameObject wrist;

    void Start() {
        wrist = transform.parent.Find("Wrist").gameObject;
    }

    void Update() {
        transform.LookAt(Camera.main.transform);
        Vector3 direction = Vector3.Normalize(Camera.main.transform.position - wrist.transform.position);
        Vector3 wristUp = Vector3.Normalize(wrist.transform.TransformDirection(Vector3.up));

        if (Vector3.Dot(wristUp, direction) > 0.5f) {
            SetChildrenActive(true);
        } else {
            SetChildrenActive(false);
        }
    }

    private void SetChildrenActive(bool active) {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(active);
        }
    }
}
