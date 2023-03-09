using UnityEngine;

public class DisplayCameraAlignment : MonoBehaviour {
    public GameObject model;

    public void AlignCoronal() {
        transform.position = model.transform.position + model.transform.TransformDirection(new Vector3(0, 0, 1));
        transform.LookAt(model.transform);
    }

    public void AlignSagittal() {
        transform.position = model.transform.position + model.transform.TransformDirection(new Vector3(1, 0, 0));
        transform.LookAt(model.transform);
    }

    public void AlignAxial() {
        transform.position = model.transform.position + model.transform.TransformDirection(new Vector3(0, 1, 0));
        transform.LookAt(model.transform);
    }
}
