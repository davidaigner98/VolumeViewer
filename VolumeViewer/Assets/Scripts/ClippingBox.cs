using Leap;
using Leap.Unity;
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
        if (!active) { UpdateModelMaterials(); }

        foreach (Transform child in transform.Find("Lines")) {
            child.gameObject.GetComponent<LineRenderer>().enabled = active;
        }

        foreach (Transform child in transform.Find("Corners")) {
            child.gameObject.GetComponent<MeshRenderer>().enabled = active;
        }
    }

    public bool IsActive() {
        return active;
    }

    private void DrawBox() {
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform.Find("Lines"));

        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform.Find("Lines"));

        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z)).transform.SetParent(transform.Find("Lines"));
        DrawLine(transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z)).transform.SetParent(transform.Find("Lines"));
    }

    private GameObject DrawLine(Vector3 from, Vector3 to) {
        GameObject newLineGO = new GameObject("Line");
        LineRenderer renderer = newLineGO.AddComponent<LineRenderer>();
        renderer.SetPosition(0, from);
        renderer.SetPosition(1, to);
        renderer.startWidth = 0.01f;
        renderer.endWidth = 0.01f;
        renderer.numCapVertices = 8;
        renderer.enabled = false;

        Material mat = new Material(Shader.Find("Standard"));
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
                if (active) {
                    currMat.SetVector("_MinBounds", minBounds);
                    currMat.SetVector("_MaxBounds", maxBounds);
                } else {
                    currMat.SetVector("_MinBounds", new Vector3(1, 1, 1));
                    currMat.SetVector("_MaxBounds", new Vector3(0, 0, 0));
                }
            }
        }
    }

    private void UpdateBoxPositions() {
        if (transform.Find("Lines").childCount != 12) { return; }

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
        transform.Find("Lines").GetChild(childIndex).GetComponent<LineRenderer>().SetPosition(pointIndex, transform.position + position);
    }

    private void UpdateTriggerBounds() {
        BoxCollider trigger = GetComponent<BoxCollider>();

        Vector3 center = (maxBounds + minBounds) / 2;
        trigger.center = center;

        Vector3 size = maxBounds - minBounds;
        trigger.size = size;
    }

    private void OnTriggerEnter(Collider other) {
        ModelInfo modelInfo = other.gameObject.GetComponentInParent<ModelInfo>();
        if (modelInfo) { includedModels.Add(modelInfo); }
    }

    private void OnTriggerExit(Collider other) {
        ModelInfo modelInfo = other.gameObject.GetComponentInParent<ModelInfo>();
        if (modelInfo) { includedModels.Remove(modelInfo); }

        Material[] currMats = other.gameObject.GetComponent<Renderer>().materials;
        foreach (Material currMat in currMats) {
            currMat.SetVector("_MinBounds", new Vector3(1, 1, 1));
            currMat.SetVector("_MaxBounds", new Vector3(0, 0, 0));
        }
    }
    
    private void RemoveNullEntries() {
        for (int i = includedModels.Count - 1; i >= 0; i--) {
            if (includedModels[i] == null) {
                includedModels.RemoveAt(i);
            }
        }
    }

    public void UpdateCorners(Vector3 corner, Vector3 position) {
        UpdateBoundary(corner.x == 1, 'x', position.x);
        UpdateBoundary(corner.y == 1, 'y', position.y);
        UpdateBoundary(corner.z == 1, 'z', position.z);

        foreach (Transform child in transform.Find("Corners")) {
            ClippingBoxGrabbableCorner cornerScript = child.GetComponent<ClippingBoxGrabbableCorner>();

            if (!cornerScript.cornerIndex.Equals(corner)) {
                cornerScript.UpdatePosition();
            }
        }
    }

    private void UpdateBoundary(bool max, char coordinate, float value) {
        if (max) {
            if (coordinate.Equals('x')) { maxBounds.Set(value, maxBounds.y, maxBounds.z); }
            else if (coordinate.Equals('y')) { maxBounds.Set(maxBounds.x, value, maxBounds.z); }
            else if (coordinate.Equals('z')) { maxBounds.Set(maxBounds.x, maxBounds.y, value); }
        } else {
            if (coordinate.Equals('x')) { minBounds.Set(value, minBounds.y, minBounds.z); }
            else if (coordinate.Equals('y')) { minBounds.Set(minBounds.x, value, minBounds.z); }
            else if (coordinate.Equals('z')) { minBounds.Set(minBounds.x, minBounds.y, value); }
        }
    }

    public void StartPinchMovement(Hand grabbingHand) {
        Vector3 pinchPosition = grabbingHand.GetPinchPosition();
        GameObject pinchedCorner = null;
        float shortestDistancce = -1;

        foreach (Transform corner in transform.Find("Corners")) {
            float currDistance = Vector3.Distance(pinchPosition, corner.position);

            if (pinchedCorner == null || currDistance < shortestDistancce) {
                pinchedCorner = corner.gameObject;
                shortestDistancce = currDistance;
            }
        }

        if (Vector3.Distance(pinchPosition, pinchedCorner.transform.position) < 0.5f) {
            ClippingBoxGrabbableCorner cornerScript = pinchedCorner.GetComponent<ClippingBoxGrabbableCorner>();
            cornerScript.StartGrabMovement(grabbingHand);
        }
    }

    public void EndPinchMovement() {
        foreach (Transform corner in transform.Find("Corners")) {
            ClippingBoxGrabbableCorner cornerScript = corner.GetComponent<ClippingBoxGrabbableCorner>();
            cornerScript.EndGrabMovement();
        }
    }
}
