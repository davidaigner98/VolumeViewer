using System.Collections.Generic;
using UnityEngine;

public class ClippingBox : MonoBehaviour {
    public static ClippingBox Instance;
    public Vector3 minBounds, maxBounds;
    public List<ModelInfo> includedModels = new List<ModelInfo>();
    public bool drawBox;
    private bool active = false;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        if (drawBox) { DrawBox(); }
    }

    private void Update() {
        if (active) {
            UpdateModelMaterials();

            if (drawBox) { UpdateBoxPositions(); }
            UpdateTriggerBounds();
        }
    }

    public void SetActive(bool active) {
        this.active = active;

        foreach (Transform child in transform) {
            child.gameObject.GetComponent<LineRenderer>().enabled = active;
        }
    }

    private void DrawBox() {
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform);

        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform);

        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z)).transform.SetParent(transform);
        DrawLine(transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform);
    }

    private GameObject DrawLine(Vector3 from, Vector3 to) {
        GameObject newLineGO = new GameObject("Line");
        LineRenderer renderer = newLineGO.AddComponent<LineRenderer>();
        renderer.SetPosition(0, from);
        renderer.SetPosition(1, to);
        renderer.startWidth = 0.01f;
        renderer.endWidth = 0.01f;
        renderer.numCapVertices = 8;

        Material mat = new Material(Shader.Find("Transparent/Diffuse"));
        mat.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        renderer.material = mat;

        return newLineGO;
    }

    private void UpdateModelMaterials() {
        Vector3 minBounds = transform.position + this.minBounds;
        Vector3 maxBounds = transform.position + this.maxBounds;

        RemoveNullEntries();

        foreach (ModelInfo currModel in includedModels) {
            Material[] currMats = currModel.gameObject.GetComponent<Renderer>().materials;

            foreach (Material currMat in currMats) {
                currMat.SetVector("_MinBounds", minBounds);
                currMat.SetVector("_MaxBounds", maxBounds);
            }
        }
    }

    private void UpdateBoxPositions() {
        if (transform.childCount != 12) { return; }

        SetPositionOfLineRenderer(0, 0, new Vector3(minBounds.x, minBounds.y, minBounds.z));
        SetPositionOfLineRenderer(0, 1, new Vector3(maxBounds.x, minBounds.y, minBounds.z));
        SetPositionOfLineRenderer(1, 0, new Vector3(minBounds.x, minBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(1, 1, new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(2, 0, new Vector3(minBounds.x, maxBounds.y, minBounds.z));
        SetPositionOfLineRenderer(2, 1, new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
        SetPositionOfLineRenderer(3, 0, new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(3, 1, new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));

        SetPositionOfLineRenderer(4, 0, new Vector3(minBounds.x, minBounds.y, minBounds.z));
        SetPositionOfLineRenderer(4, 1, new Vector3(minBounds.x, maxBounds.y, minBounds.z));
        SetPositionOfLineRenderer(5, 0, new Vector3(minBounds.x, minBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(5, 1, new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(6, 0, new Vector3(maxBounds.x, minBounds.y, minBounds.z));
        SetPositionOfLineRenderer(6, 1, new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
        SetPositionOfLineRenderer(7, 0, new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(7, 1, new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));

        SetPositionOfLineRenderer(8, 0, new Vector3(minBounds.x, minBounds.y, minBounds.z));
        SetPositionOfLineRenderer(8, 1, new Vector3(minBounds.x, minBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(9, 0, new Vector3(minBounds.x, maxBounds.y, minBounds.z));
        SetPositionOfLineRenderer(9, 1, new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(10, 0, new Vector3(maxBounds.x, minBounds.y, minBounds.z));
        SetPositionOfLineRenderer(10, 1, new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
        SetPositionOfLineRenderer(11, 0, new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
        SetPositionOfLineRenderer(11, 1, new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));
    }

    private void SetPositionOfLineRenderer(int childIndex, int pointIndex, Vector3 position) {
        transform.GetChild(childIndex).GetComponent<LineRenderer>().SetPosition(pointIndex, transform.position + position);
    }

    private void UpdateTriggerBounds() {
        BoxCollider trigger = GetComponent<BoxCollider>();

        Vector3 center = (maxBounds + minBounds) / 2;
        trigger.center = center;

        Vector3 size = maxBounds - minBounds;
        trigger.size = size;
    }

    private void OnTriggerEnter(Collider other) {
        ModelInfo modelInfo = other.gameObject.GetComponent<ModelInfo>();
        if (modelInfo) { includedModels.Add(modelInfo); }
    }

    private void OnTriggerExit(Collider other) {
        ModelInfo modelInfo = other.gameObject.GetComponent<ModelInfo>();
        if (modelInfo) { includedModels.Remove(modelInfo); }
    }

    private void RemoveNullEntries() {
        for (int i = includedModels.Count - 1; i >= 0; i--) {
            if (includedModels[i] == null) {
                includedModels.RemoveAt(i);
            }
        }
    }
}
