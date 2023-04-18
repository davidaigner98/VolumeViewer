using UnityEngine;

public class DisplayCameraPositioning : MonoBehaviour {
    public static DisplayCameraPositioning Instance { get; private set; }

    public bool cameraRepositioning;
    public Vector2 viewportSize;
    public float focalRadius = 3;

    public bool drawCage;
    public Vector3 cageSize;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        transform.position = new Vector3(0, 0, -focalRadius);
        transform.LookAt(Vector3.zero);

        if (drawCage) { DrawCage(); }
    }

    public void SynchronizeDisplayCameraPosition(Vector3 cameraOffset) {
        if (!cameraRepositioning) { return; }

        cameraOffset = cameraOffset.normalized * focalRadius;
        transform.position = cameraOffset;
        transform.LookAt(Vector3.zero);

        ModelManager.Instance.RefreshModelScreenOffsets();
    }

    private void DrawCage() {
        GameObject cageGO = new GameObject("Grid");
        Vector3 startPosition = new Vector3(0, 0, -focalRadius);

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
