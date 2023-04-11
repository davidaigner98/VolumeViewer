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

    private List<ClippingBoxCorner> corners = new List<ClippingBoxCorner>();
    private List<Vector3> possibleIndices = new List<Vector3>() {
        new Vector3(-1, -1, -1),
        new Vector3(-1, -1, +1),
        new Vector3(-1, +1, -1),
        new Vector3(-1, +1, +1),
        new Vector3(+1, -1, -1),
        new Vector3(+1, -1, +1),
        new Vector3(+1, +1, -1),
        new Vector3(+1, +1, +1)
    };

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    public void Setup() {
        foreach (Transform cornerGO in transform.Find("Corners")) {
            corners.Add(cornerGO.GetComponent<ClippingBoxCorner>());
        }

        if (drawBox) { DrawBox(); }
        UpdateTrigger();
        UpdateLineVertices();
        UpdateAllCornerPositions();
    }

    private void Update() {
        if (active) {
            UpdateModelMaterials();
        } else {
            SetActive(true);
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
        Vector4 rotation = new Vector4(-transform.rotation.x, -transform.rotation.y, -transform.rotation.z, transform.rotation.w);

        RemoveNullEntries();

        foreach (ModelInfo currModel in includedModels) {
            Material[] currMats = currModel.transform.Find("Model").GetComponent<Renderer>().materials;

            foreach (Material currMat in currMats) {
                if (active) {
                    currMat.SetVector("_MinBounds", minBounds);
                    currMat.SetVector("_MaxBounds", maxBounds);
                    currMat.SetVector("_Rotation", rotation);
                } else {
                    currMat.SetVector("_MinBounds", new Vector3(1, 1, 1));
                    currMat.SetVector("_MaxBounds", new Vector3(0, 0, 0));
                }
            }
        }
    }

    private void UpdateLineVertices() {
        if (transform.Find("Lines").childCount != 12) { return; }

        SetVerticeOfLineRenderer(0, 0, new Vector3(minBounds.x, minBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(0, 1, new Vector3(maxBounds.x, minBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(1, 0, new Vector3(minBounds.x, minBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(1, 1, new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(2, 0, new Vector3(minBounds.x, maxBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(2, 1, new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(3, 0, new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(3, 1, new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));

        SetVerticeOfLineRenderer(4, 0, new Vector3(minBounds.x, minBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(4, 1, new Vector3(minBounds.x, maxBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(5, 0, new Vector3(minBounds.x, minBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(5, 1, new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(6, 0, new Vector3(maxBounds.x, minBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(6, 1, new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(7, 0, new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(7, 1, new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));

        SetVerticeOfLineRenderer(8, 0, new Vector3(minBounds.x, minBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(8, 1, new Vector3(minBounds.x, minBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(9, 0, new Vector3(minBounds.x, maxBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(9, 1, new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(10, 0, new Vector3(maxBounds.x, minBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(10, 1, new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
        SetVerticeOfLineRenderer(11, 0, new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
        SetVerticeOfLineRenderer(11, 1, new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));
    }

    private void SetVerticeOfLineRenderer(int childIndex, int pointIndex, Vector3 position) {
        position = transform.TransformDirection(position);
        transform.Find("Lines").GetChild(childIndex).GetComponent<LineRenderer>().SetPosition(pointIndex, transform.position + position);
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

        if (Vector3.Distance(pinchPosition, pinchedCorner.transform.position) < 0.2f) {
            ClippingBoxCorner cornerScript = pinchedCorner.GetComponent<ClippingBoxCorner>();
            cornerScript.StartGrabMovement(grabbingHand);
        }
    }

    public void EndPinchMovement() {
        foreach (Transform corner in transform.Find("Corners")) {
            ClippingBoxCorner cornerScript = corner.GetComponent<ClippingBoxCorner>();
            cornerScript.EndGrabMovement();
        }
    }

    public void UpdateCorner(GameObject cornerGO, Vector3 newPosition) {
        Vector3 cornerIndex = GetIndexOfCorner(cornerGO);
        newPosition = newPosition - transform.position;

        UpdateBoundary(cornerIndex.x == 1, 'x', newPosition.x);
        UpdateBoundary(cornerIndex.y == 1, 'y', newPosition.y);
        UpdateBoundary(cornerIndex.z == 1, 'z', newPosition.z);
        
        UpdateAllCornerPositions();
        UpdateTrigger();
        UpdateLineVertices();
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

    private void UpdateTrigger() {
        BoxCollider trigger = GetComponent<BoxCollider>();

        Vector3 center = (maxBounds + minBounds) / 2;
        trigger.center = center;

        Vector3 size = maxBounds - minBounds;
        trigger.size = size;
    }

    private void UpdateAllCornerPositions() {
        List<GameObject> cornerGOs = new List<GameObject>();

        foreach (Vector3 index in possibleIndices) {
            cornerGOs.Add(GetCorner(index));
        }

        for (int i = 0; i < possibleIndices.Count; i++) {
            UpdateCornerPosition(cornerGOs[i], possibleIndices[i]);
        }
    }

    private void UpdateCornerPosition(GameObject cornerGO, Vector3 index) {
        float posX = maxBounds.x;
        if (index.x == -1) { posX = minBounds.x; }
        float posY = maxBounds.y;
        if (index.y == -1) { posY = minBounds.y; }
        float posZ = maxBounds.z;
        if (index.z == -1) { posZ = minBounds.z; }

        cornerGO.transform.localPosition = new Vector3(posX, posY, posZ);
    }

    private GameObject GetCorner(Vector3 index) {
        GameObject minimumCorner = null;
        Vector3 boxCenter = GetBoxCenter();
        float positionSum = 0;

        foreach(ClippingBoxCorner corner in corners) {
            Vector3 currPosition = corner.transform.position - boxCenter;
            float currPositionSum = 0;
            currPositionSum += currPosition.x * index.x;
            currPositionSum += currPosition.y * index.y;
            currPositionSum += currPosition.z * index.z;
            
            if (minimumCorner == null || currPositionSum < positionSum) {
                minimumCorner = corner.gameObject;
                positionSum = currPositionSum;
            }
        }
        
        return minimumCorner;
    }

    private Vector3 GetBoxCenter() {
        Vector3 center = Vector3.zero;

        foreach (ClippingBoxCorner corner in corners) {
            center += corner.transform.position;
        }

        center = center / corners.Count;
        return center;
    }

    private Vector3 GetIndexOfCorner(GameObject cornerGO) {
        foreach (Vector3 currIndex in possibleIndices) {
            GameObject currCornerGO = GetCorner(currIndex);

            if (ReferenceEquals(cornerGO, currCornerGO)) {
                return currIndex;
            }
        }

        return Vector3.zero;
    }
}
