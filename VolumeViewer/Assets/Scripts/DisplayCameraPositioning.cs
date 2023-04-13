using UnityEngine;

public class DisplayCameraPositioning : MonoBehaviour {
    public static DisplayCameraPositioning Instance { get; private set; }
    public bool drawCage;
    public Vector3 cageSize;

    public bool cameraRepositioning;
    public Vector2 viewportSize;
    public Vector3 startPosition;
    public Vector3 focalPoint;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        transform.position = startPosition;
        transform.LookAt(focalPoint);

        if (drawCage) { DrawCage(); }
    }

    public void SynchronizeDisplayCameraPosition(Vector3 cameraOffset) {
        if (!cameraRepositioning) { return; }

        cameraOffset *= viewportSize.x;
        cameraOffset += startPosition;
        cameraOffset -= focalPoint;

        float focalRadius = (startPosition - focalPoint).magnitude;
        cameraOffset = cameraOffset.normalized * focalRadius;
        cameraOffset += focalPoint;

        transform.position = cameraOffset;
        transform.LookAt(focalPoint);

        ModelManager.Instance.RefreshModelScreenOffsets();
    }

    private void DrawCage() {
        GameObject cageGO = new GameObject("Grid");

        DrawLine(startPosition + new Vector3(-cageSize.x / 2, -cageSize.y / 2, -cageSize.z), startPosition + new Vector3(cageSize.x / 2, -cageSize.y / 2, -cageSize.z)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(-cageSize.x / 2, cageSize.y / 2, -cageSize.z), startPosition + new Vector3(cageSize.x / 2, cageSize.y / 2, -cageSize.z)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(-cageSize.x / 2, -cageSize.y / 2, -cageSize.z), startPosition + new Vector3(-cageSize.x / 2, cageSize.y / 2, -cageSize.z)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(cageSize.x / 2, -cageSize.y / 2, -cageSize.z), startPosition + new Vector3(cageSize.x / 2, cageSize.y / 2, -cageSize.z)).transform.SetParent(cageGO.transform);

        DrawLine(startPosition + new Vector3(-cageSize.x / 2, -cageSize.y / 2, -cageSize.z * 2 / 3), startPosition + new Vector3(cageSize.x / 2, -cageSize.y / 2, -cageSize.z * 2 / 3)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(-cageSize.x / 2, cageSize.y / 2, -cageSize.z * 2 / 3), startPosition + new Vector3(cageSize.x / 2, cageSize.y / 2, -cageSize.z * 2 / 3)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(-cageSize.x / 2, -cageSize.y / 2, -cageSize.z * 2 / 3), startPosition + new Vector3(-cageSize.x / 2, cageSize.y / 2, -cageSize.z * 2 / 3)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(cageSize.x / 2, -cageSize.y / 2, -cageSize.z * 2 / 3), startPosition + new Vector3(cageSize.x / 2, cageSize.y / 2, -cageSize.z * 2 / 3 )).transform.SetParent(cageGO.transform);

        DrawLine(startPosition + new Vector3(-cageSize.x / 2, -cageSize.y / 2, -cageSize.z * 1 / 3), startPosition + new Vector3(cageSize.x / 2, -cageSize.y / 2, -cageSize.z * 1 / 3)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(-cageSize.x / 2, cageSize.y / 2, -cageSize.z * 1 / 3), startPosition + new Vector3(cageSize.x / 2, cageSize.y / 2, -cageSize.z * 1 / 3)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(-cageSize.x / 2, -cageSize.y / 2, -cageSize.z * 1 / 3), startPosition + new Vector3(-cageSize.x / 2, cageSize.y / 2, -cageSize.z * 1 / 3)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(cageSize.x / 2, -cageSize.y / 2, -cageSize.z * 1 / 3), startPosition + new Vector3(cageSize.x / 2, cageSize.y / 2, -cageSize.z * 1 / 3)).transform.SetParent(cageGO.transform);

        DrawLine(startPosition + new Vector3(-cageSize.x / 2, -cageSize.y / 2, -cageSize.z), startPosition + new Vector3(-cageSize.x / 2, -cageSize.y / 2, 0)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(-cageSize.x / 2, cageSize.y / 2, -cageSize.z), startPosition + new Vector3(-cageSize.x / 2, cageSize.y / 2, 0)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(cageSize.x / 2, -cageSize.y / 2, -cageSize.z), startPosition + new Vector3(cageSize.x / 2, -cageSize.y / 2, 0)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(cageSize.x / 2, cageSize.y / 2, -cageSize.z), startPosition + new Vector3(cageSize.x / 2, cageSize.y / 2, 0)).transform.SetParent(cageGO.transform);

        DrawLine(startPosition + new Vector3(-cageSize.x / 2, 0, -cageSize.z), startPosition + new Vector3(-cageSize.x / 2, 0, 0)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(cageSize.x / 2, 0, -cageSize.z), startPosition + new Vector3(cageSize.x / 2, 0, 0)).transform.SetParent(cageGO.transform);

        DrawLine(startPosition + new Vector3(-cageSize.x / 6, -cageSize.y / 2, -cageSize.z), startPosition + new Vector3(-cageSize.x / 6, -cageSize.y / 2, 0)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(-cageSize.x / 6, cageSize.y / 2, -cageSize.z), startPosition + new Vector3(-cageSize.x / 6, cageSize.y / 2, 0)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(cageSize.x / 6, -cageSize.y / 2, -cageSize.z), startPosition + new Vector3(cageSize.x / 6, -cageSize.y / 2, 0)).transform.SetParent(cageGO.transform);
        DrawLine(startPosition + new Vector3(cageSize.x / 6, cageSize.y / 2, -cageSize.z), startPosition + new Vector3(cageSize.x / 6, cageSize.y / 2, 0)).transform.SetParent(cageGO.transform);
    }

    private GameObject DrawLine(Vector3 from, Vector3 to) {
        GameObject newLineGO = new GameObject("Line");
        LineRenderer renderer = newLineGO.AddComponent<LineRenderer>();
        renderer.SetPosition(0, from);
        renderer.SetPosition(1, to);
        renderer.startWidth = 0.1f;
        renderer.endWidth = 0.1f;

        Material mat = new Material(Shader.Find("Transparent/Diffuse"));
        mat.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        renderer.material = mat;

        return newLineGO;
    }
}
