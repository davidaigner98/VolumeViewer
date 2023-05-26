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
    private List<Vector3> possibleCornerIndices = new List<Vector3>() {
        new Vector3(-1, -1, -1),
        new Vector3(-1, -1, +1),
        new Vector3(-1, +1, -1),
        new Vector3(-1, +1, +1),
        new Vector3(+1, -1, -1),
        new Vector3(+1, -1, +1),
        new Vector3(+1, +1, -1),
        new Vector3(+1, +1, +1)
    };

    private List<ClippingBoxCorner> faces = new List<ClippingBoxCorner>();
    private List<Vector3> possibleFaceIndices = new List<Vector3>() {
        new Vector3(+1, 0, 0),
        new Vector3(0, +1, 0),
        new Vector3(0, 0, +1),
        new Vector3(-1, 0, 0),
        new Vector3(0, -1, 0),
        new Vector3(0, 0, -1),
    };

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    // performs the initial setup
    public void Setup() {
        foreach (Transform cornerGO in transform.Find("Corners")) {
            corners.Add(cornerGO.GetComponent<ClippingBoxCorner>());
        }

        foreach (Transform faceGO in transform.Find("Faces")) {
            faces.Add(faceGO.GetComponent<ClippingBoxCorner>());
        }

        if (drawBox) { DrawBox(); }
        UpdateTrigger();
        UpdateLineVertices();
        UpdateAllCornerPositions();
        UpdateAllFacePositions();
    }

    private void Update() {
        if (active) {
            UpdateModelMaterials();
        }
    }

    // enables or disables the clipping box
    public void SetActive(bool active) {
        this.active = active;
        if (!active) { UpdateModelMaterials(); }

        // enable or disable all lines
        foreach (Transform child in transform.Find("Lines")) {
            child.gameObject.GetComponent<LineRenderer>().enabled = active;
        }

        // enable or disable all corners
        foreach (Transform child in transform.Find("Corners")) {
            child.gameObject.GetComponent<MeshRenderer>().enabled = active;
        }

        // enable or disable all faces
        foreach (Transform child in transform.Find("Faces")) {
            child.gameObject.GetComponent<MeshRenderer>().enabled = active;
        }
    }

    // return if the clipping box is active or not
    public bool IsActive() {
        return active;
    }

    // draws the lines of the box
    private void DrawBox() {
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z));
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));

        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z));
        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));

        DrawLine(transform.position + new Vector3(minBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, minBounds.y, maxBounds.z));
        DrawLine(transform.position + new Vector3(minBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
        DrawLine(transform.position + new Vector3(maxBounds.x, minBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
        DrawLine(transform.position + new Vector3(maxBounds.x, maxBounds.y, minBounds.z), transform.position + new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));
    }

    // draws a singular line between two points
    private GameObject DrawLine(Vector3 from, Vector3 to) {
        // instantiates new line game object
        GameObject newLineGO = new GameObject("Line");
        newLineGO.transform.SetParent(transform.Find("Lines"));
        newLineGO.transform.localPosition = Vector3.zero;
        newLineGO.transform.localRotation = Quaternion.identity;

        // adds new line renderer to the line game object
        LineRenderer renderer = newLineGO.AddComponent<LineRenderer>();
        renderer.SetPosition(0, from);
        renderer.SetPosition(1, to);
        renderer.startWidth = 0.005f;
        renderer.endWidth = 0.005f;
        renderer.numCapVertices = 8;
        renderer.enabled = false;

        // sets up the line material and color
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        renderer.material = mat;

        return newLineGO;
    }

    // updates all clientside models within the clipping box shader properties for clipping
    private void UpdateModelMaterials() {
        Vector4 rotation = new Vector4(-transform.rotation.x, -transform.rotation.y, -transform.rotation.z, transform.rotation.w);

        // remove models from the list, that dont exist anymore
        RemoveNullEntries();

        // update all models, within the clipping box
        foreach (ModelInfo currModel in includedModels) {
            Material[] currMats = currModel.transform.Find("Model").GetComponent<Renderer>().materials;

            // update all materials on this model
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

    // update vertices of the drawn lines
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

    // updates a single vertice of a specific line game object
    private void SetVerticeOfLineRenderer(int childIndex, int pointIndex, Vector3 position) {
        position = transform.TransformDirection(position);
        transform.Find("Lines").GetChild(childIndex).GetComponent<LineRenderer>().SetPosition(pointIndex, transform.position + position);
    }

    // triggers if a model enters the clipping box trigger
    private void OnTriggerEnter(Collider other) {
        ModelInfo modelInfo = other.gameObject.GetComponentInParent<ModelInfo>();
        if (modelInfo) { includedModels.Add(modelInfo); }
    }

    // triggers if a model exits the clipping box trigger
    private void OnTriggerExit(Collider other) {
        ModelInfo modelInfo = other.gameObject.GetComponentInParent<ModelInfo>();
        if (modelInfo) { includedModels.Remove(modelInfo); }

        Material[] currMats = other.gameObject.GetComponent<Renderer>().materials;
        foreach (Material currMat in currMats) {
            currMat.SetVector("_MinBounds", new Vector3(1, 1, 1));
            currMat.SetVector("_MaxBounds", new Vector3(0, 0, 0));
        }
    }
    
    // removes null entries from the model list (models within the clipping box) (e.g. models that have been deleted)
    private void RemoveNullEntries() {
        for (int i = includedModels.Count - 1; i >= 0; i--) {
            if (includedModels[i] == null) {
                includedModels.RemoveAt(i);
            }
        }
    }

    // start pinch movement for corners
    public void StartPinchMovement(Hand grabbingHand) {
        Vector3 pinchPosition = grabbingHand.GetPinchPosition();
        GameObject pinchedGrabbable = null;
        float shortestDistancce = -1;

        // find closest corner
        foreach (Transform corner in transform.Find("Corners")) {
            float currDistance = Vector3.Distance(pinchPosition, corner.position);

            if (pinchedGrabbable == null || currDistance < shortestDistancce) {
                pinchedGrabbable = corner.gameObject;
                shortestDistancce = currDistance;
            }
        }

        // or find closest face
        foreach (Transform face in transform.Find("Faces")) {
            float currDistance = Vector3.Distance(pinchPosition, face.position);

            if (pinchedGrabbable == null || currDistance < shortestDistancce) {
                pinchedGrabbable = face.gameObject;
                shortestDistancce = currDistance;
            }
        }

        // if closest corner is close enough, perform pinch movement
        if (Vector3.Distance(pinchPosition, pinchedGrabbable.transform.position) < 0.2f) {
            pinchedGrabbable.GetComponent<ClippingBoxCorner>().StartGrabMovement(grabbingHand);
        }
    }

    // end pinch movement for corners
    public void EndPinchMovement() {
        foreach (Transform corner in transform.Find("Corners")) {
            corner.GetComponent<ClippingBoxCorner>().EndGrabMovement();
        }

        foreach (Transform face in transform.Find("Faces")) {
            face.GetComponent<ClippingBoxCorner>().EndGrabMovement();
        }

        UpdateAllCornerPositions();
        UpdateAllFacePositions();
    }

    // update minimal and maximal bounds based on the new position of one corner
    public void UpdateCorner(GameObject cornerGO, Vector3 position) {
        // determine which corner was moved
        Vector3 cornerIndex = GetIndexOfCorner(cornerGO);
        position = transform.InverseTransformDirection(position);

        // update minimal and maximal bounds
        UpdateBoundary(cornerIndex.x > 0, 'x', position.x);
        UpdateBoundary(cornerIndex.y > 0, 'y', position.y);
        UpdateBoundary(cornerIndex.z > 0, 'z', position.z);

        // update other corners, the line game objects and the trigger
        UpdateAllCornerPositions();
        UpdateAllFacePositions();
        UpdateLineVertices();
        UpdateTrigger();
    }

    // update the minimal or maximal bounds of the clipping box
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

        // correct x bounds if box is inverse
        if (minBounds.x > maxBounds.x) {
            float tmp = minBounds.x;
            minBounds.x = maxBounds.x;
            maxBounds.x = tmp;
        }

        // correct y bounds if box is inverse
        if (minBounds.y > maxBounds.y) {
            float tmp = minBounds.y;
            minBounds.y = maxBounds.y;
            maxBounds.y = tmp;
        }

        // correct z bounds if box is inverse
        if (minBounds.z > maxBounds.z) {
            float tmp = minBounds.z;
            minBounds.z = maxBounds.z;
            maxBounds.z = tmp;
        }
    }

    // shifts the bounds of the clipping box by a delta vector
    public void ShiftBoundary(Vector3 delta) {
        minBounds += delta;
        maxBounds += delta;

        // update other corners, the line game objects and the trigger
        UpdateAllCornerPositions();
        UpdateAllFacePositions();
        UpdateLineVertices();
        UpdateTrigger();
    }

    // ipdate box trigger center and size
    private void UpdateTrigger() {
        BoxCollider trigger = GetComponent<BoxCollider>();

        Vector3 center = (maxBounds + minBounds) / 2;
        trigger.center = center;

        Vector3 size = maxBounds - minBounds;
        trigger.size = size;
    }

    // update all corner positions respective the minimal and maximal bounds
    private void UpdateAllCornerPositions() {
        // determine the right order of the corners, respective their indices
        List<GameObject> cornerGOs = new List<GameObject>();
        foreach (Vector3 index in possibleCornerIndices) {
            cornerGOs.Add(GetCorner(index));
        }

        // update the corners positions
        for (int i = 0; i < possibleCornerIndices.Count; i++) {
            UpdateCornerPosition(cornerGOs[i], possibleCornerIndices[i]);
        }
    }

    // update all face box positions respective the minimal and maximal bounds
    private void UpdateAllFacePositions() {
        // determine the right order of the corners, respective their indices
        List<GameObject> faceGOs = new List<GameObject>();
        foreach (Vector3 index in possibleFaceIndices) {
            faceGOs.Add(GetFace(index));
        }

        // update the corners positions
        for (int i = 0; i < possibleFaceIndices.Count; i++) {
            UpdateFacePosition(faceGOs[i], possibleFaceIndices[i]);
        }
    }

    // update the position of a specific corner
    private void UpdateCornerPosition(GameObject cornerGO, Vector3 index) {
        float posX = maxBounds.x;
        if (index.x < 0) { posX = minBounds.x; }
        float posY = maxBounds.y;
        if (index.y < 0) { posY = minBounds.y; }
        float posZ = maxBounds.z;
        if (index.z < 0) { posZ = minBounds.z; }

        cornerGO.transform.localPosition = new Vector3(posX, posY, posZ);
    }

    // update the position of a specific face
    private void UpdateFacePosition(GameObject faceGO, Vector3 index) {
        Vector3 center = GetBoxCenter();
        float posX = 0, posY = 0, posZ = 0;

        if (index.x > 0) { posX = maxBounds.x - center.x; }
        else if (index.x < 0) { posX = minBounds.x - center.x; }
        else if (index.y > 0) { posY = maxBounds.y - center.y; }
        else if (index.y < 0) { posY = minBounds.y - center.y; }
        else if (index.z > 0) { posZ = maxBounds.z - center.z; }
        else if (index.z < 0) { posZ = minBounds.z - center.z; }

        faceGO.transform.localPosition = center + new Vector3(posX, posY, posZ);
    }

    // find a corner based on its index
    private GameObject GetCorner(Vector3 index) {
        GameObject resultCorner = null;
        Vector3 boxCenter = GetBoxCenter();
        float positionSum = 0;

        // iterate through all corners
        foreach(ClippingBoxCorner corner in corners) {
            Vector3 currPosition = corner.transform.localPosition - boxCenter;
            float currPositionSum = 0;
            
            // sum up the directional position sum
            currPositionSum += currPosition.x * index.x;
            currPositionSum += currPosition.y * index.y;
            currPositionSum += currPosition.z * index.z;
            
            // if the position of the corner is in the same direction as the index, save it
            if (resultCorner == null || currPositionSum > positionSum) {
                resultCorner = corner.gameObject;
                positionSum = currPositionSum;
            }
        }
        
        return resultCorner;
    }

    // find a face based on its index
    private GameObject GetFace(Vector3 index) {
        GameObject resultFace = null;
        Vector3 boxCenter = GetBoxCenter();
        float positionSum = 0;

        // iterate through all corners
        foreach(ClippingBoxCorner face in faces) {
            Vector3 currPosition = face.transform.localPosition - boxCenter;
            float currPositionSum = 0;
            
            // sum up the directional position sum
            currPositionSum += currPosition.x * index.x;
            currPositionSum += currPosition.y * index.y;
            currPositionSum += currPosition.z * index.z;
            
            // if the position of the corner is in the same direction as the index, save it
            if (resultFace == null || currPositionSum > positionSum) {
                resultFace = face.gameObject;
                positionSum = currPositionSum;
            }
        }
        
        return resultFace;
    }

    // returns the center position of the clipping box, (e.g. average position of all corners)
    private Vector3 GetBoxCenter() {
        Vector3 center = Vector3.zero;

        // sum up all corner positions
        foreach (ClippingBoxCorner corner in corners) {
            center += corner.transform.localPosition;
        }

        // divide to calculate average
        center = center / corners.Count;
        return center;
    }

    // get index of corner based on its relative position
    public Vector3 GetIndexOfCorner(GameObject cornerGO) {
        int indexX, indexY, indexZ;
        Vector3 boxCenter = GetBoxCenter();

        // get position, relative to the box center
        Vector3 relativePos = cornerGO.transform.localPosition - boxCenter;

        // determine index
        if (relativePos.x > 0) { indexX = 1; } else { indexX = -1; }
        if (relativePos.y > 0) { indexY = 1; } else { indexY = -1; }
        if (relativePos.z > 0) { indexZ = 1; } else { indexZ = -1; }

        return new Vector3(indexX, indexY, indexZ);
    }
}
