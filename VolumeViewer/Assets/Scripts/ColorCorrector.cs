using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ColorCorrector : NetworkBehaviour {
    private Camera cam;
    private Material[] mats;
    private Mesh mesh;
    private float timer;
    private Color[] originalColors;

    void Start() {
        if (CrossPlatformMediator.Instance.isServer) {
            cam = DisplayCameraPositioning.Instance.GetComponent<Camera>();
        } else {
            cam = GameObject.Find("XRRig/Camera Offset/Main Camera").GetComponent<Camera>();
        }

        GameObject model = transform.Find("Model").gameObject;
        mats = model.GetComponent<Renderer>().materials;
        mesh = model.GetComponent<MeshFilter>().mesh;

        originalColors = new Color[mats.Length];
        for (int i = 0; i < mats.Length; i++) {
            originalColors[i] = mats[i].color;
        }
    }

    void Update() {
        if (CrossPlatformMediator.Instance.isServer) {
            timer += Time.deltaTime;

            if (timer >= 0.05f) {
                Vector3 screenPos = cam.WorldToScreenPoint(transform.position);

                if (screenPos.x >= 0 && screenPos.x < Screen.width && screenPos.y >= 0 && screenPos.y < Screen.height && screenPos.z > 0) {
                    if (GetComponent<ModelTransformator>().screenOffset.Value.z < 0.5f) {
                        Debug.Log("EXECUTING NEW COLOR CORRECTION CALL");
                        Color serverColor = ColorPickByRaycast((int)screenPos.x, (int)screenPos.y);
                        Debug.Log("Server Color: "+serverColor);

                        ColorCorrectionCallClientRpc(serverColor);
                    }
                }

                /*Debug.Log("SubMeshCount: "+mesh.subMeshCount);
                Debug.Log("VertexCount: "+mesh.vertices.Length);
                Debug.Log("TriangleCount: "+mesh.triangles.Count());
                for (int i = 0; i < mats.Count(); i++) {
                    Material mat = mats[i];

                    Vector3 firstVisibleTriangle = GetFirstVisibleTriangle(i);
                    if (!firstVisibleTriangle.Equals(-Vector3.one)) {
                        Debug.Log(i + " " + mat.name + " is there: " + GetFirstVisibleTriangle(i));
                    }

                    //ColorCorrectionCallClientRpc(mat.name);
                }*/

                timer -= 0.05f;
            }
        }
    }

    private Vector3 GetFirstVisibleTriangle(int submesh) {
        int randomTriangle = -1;
        int[] tris = mesh.GetTriangles(submesh);
        int numTris = tris.Length / 3;
        Vector3[] verts = mesh.vertices;

        for (int i = 0; i < numTris; i++) {
            Vector3 v0 = transform.TransformPoint(verts[tris[i * 3 + 0]]);
            Vector3 v1 = transform.TransformPoint(verts[tris[i * 3 + 1]]);
            Vector3 v2 = transform.TransformPoint(verts[tris[i * 3 + 2]]);
            Vector3 triCenter = (v0 + v1 + v2) / 3;

            Vector3 screenPos = cam.WorldToScreenPoint(triCenter);

            if (screenPos.x >= 0 && screenPos.x < Screen.width) {
                if (screenPos.y >= 0 && screenPos.y < Screen.height) {
                    if (screenPos.z >= 0) {
                        return screenPos;
                    }
                }
            }
        }

        return -Vector3.one;
    }

    [ClientRpc]
    private void ColorCorrectionCallClientRpc(Color serverColor) {
        if (!CrossPlatformMediator.Instance.isServer) {
            Debug.Log("COLOR CORRECTION CALLED");
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);

            if (screenPos.x >= 0 && screenPos.x < Screen.width && screenPos.y >= 0 && screenPos.y < Screen.height && screenPos.z > 0) {
                Color clientColor = ColorPickByRaycast((int)screenPos.x, (int)screenPos.y);
                Debug.Log("Client Color: "+clientColor);
                Debug.Log("Server Color: "+serverColor);

                float hS, sS, vS, lS;
                Color.RGBToHSV(serverColor, out hS, out sS, out vS);
                lS = vS * (1 - sS / 2);

                float hC, sC, vC, lC;
                Color.RGBToHSV(clientColor, out hC, out sC, out vC);
                lC = vC * (1 - sC / 2);

                float lDiff= lC - lS;
                Debug.Log("LDiff = "+lDiff);

                for (int i = 0; i < mats.Length; i++) {
                    mats[i].color = ApplyLuminanceDifference(originalColors[i], lDiff);
                }
            }
        }
    }

    private Color ApplyLuminanceDifference(Color color, float luminanceDifference) {
        Color newColor = new Color(color.r, color.g, color.b);

        float h, s, v, l;
        Color.RGBToHSV(newColor, out h, out s, out v);
        l = v * (1 - s / 2);
        
        l += luminanceDifference;

        float ltmp = l;
        if (1.0f - l < l) { ltmp = 1.0f - l; }
        v = l + s * ltmp;
        if (v == 0) { s = 0; }
        else { s = 2 * (1 - l / v); }

        newColor = Color.HSVToRGB(h, s, v);
        Debug.Log("Transformed "+color+" to "+newColor);
        return newColor;
    }

    private Color ColorPickByRaycast(int posX, int posY) {
        Rect viewRect = cam.pixelRect;
        Texture2D tex = new Texture2D((int)viewRect.width, (int)viewRect.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(viewRect, 0, 0, false);
        tex.Apply(false);

        return tex.GetPixel(posX, posY);
    }
}
