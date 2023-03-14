using UnityEngine;

public class DisplayCameraAlignment : MonoBehaviour {
    public GameObject model;

    private void Start()
    {
        AlignCoronal();
    }

    public void AlignCoronal() {
        transform.position = model.transform.position + model.transform.TransformDirection(new Vector3(0, 0, 2));
        transform.LookAt(model.transform);
    }

    public void AlignSagittal() {
        transform.position = model.transform.position + model.transform.TransformDirection(new Vector3(2, 0, 0));
        transform.LookAt(model.transform);
    }

    public void AlignAxial() {
        transform.position = model.transform.position + model.transform.TransformDirection(new Vector3(0, 2, 0));
        transform.LookAt(model.transform);
    }
}
