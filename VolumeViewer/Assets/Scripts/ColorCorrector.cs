using System.Collections;
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
    private Color trackedColor;

    private float[,] correctionMatrix = new float[3,3];

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
        if (!CrossPlatformMediator.Instance.isServer) {
            timer += Time.deltaTime;

            if (timer >= 0.05f) {
                Vector3 screenPos = cam.WorldToScreenPoint(transform.position);

                if (GetComponent<ModelTransformator>().screenOffset.Value.z > 0.5f) {
                    StartCoroutine(ColorPickByRaycast(screenPos, ' '));
                }

                //ScanCorrectionMatrix();

                timer -= 0.05f;
            }
        }
    }

    private IEnumerator ColorPickByRaycast(Vector3 screenPosition, char quad) {
        if (screenPosition.x < 0 || screenPosition.x >= Screen.width) {
            if (screenPosition.y < 0 || screenPosition.y >= Screen.height) {
                if (screenPosition.z <= 0) {
                    yield return new WaitForEndOfFrame();

                    Rect viewRect = cam.pixelRect;
                    Texture2D tex = new Texture2D((int)viewRect.width, (int)viewRect.height, TextureFormat.ARGB32, false);
                    tex.ReadPixels(viewRect, 0, 0, false);
                    tex.Apply(false);

                    trackedColor = tex.GetPixel((int)screenPosition.x, (int)screenPosition.y);

                    if (quad == 'r') {
                        Debug.Log("Scanned Red: " + trackedColor);
                        correctionMatrix[0, 0] = trackedColor.r;
                        correctionMatrix[0, 1] = trackedColor.g;
                        correctionMatrix[0, 2] = trackedColor.b;
                    } else if (quad == 'g') {
                        Debug.Log("Scanned Green: " + trackedColor);
                        correctionMatrix[1, 0] = trackedColor.r;
                        correctionMatrix[1, 1] = trackedColor.g;
                        correctionMatrix[1, 2] = trackedColor.b;
                    } else if (quad == 'b') {
                        Debug.Log("Scanned Blue: " + trackedColor);
                        correctionMatrix[2, 0] = trackedColor.r;
                        correctionMatrix[2, 1] = trackedColor.g;
                        correctionMatrix[2, 2] = trackedColor.b;
                    } else {
                        ReplaceColors(trackedColor);
                    }
                }
            }
        }
    }

    private void ScanCorrectionMatrix() {
        if (!CrossPlatformMediator.Instance.isServer) {
            GameObject screenCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
            Vector3 screenSize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform.localScale;

            Vector3 relativeRedPosition = new Vector3(0.475f * screenSize.x, -0.45f * screenSize.y, 0);
            Vector3 relativeGreenPosition = new Vector3(0.425f * screenSize.x, -0.45f * screenSize.y, 0);
            Vector3 relativeBluePosition = new Vector3(0.375f * screenSize.x, -0.45f * screenSize.y, 0);

            Vector3 redPosition = screenCenter.transform.position + screenCenter.transform.TransformDirection(relativeRedPosition);
            Vector3 greenPosition = screenCenter.transform.position + screenCenter.transform.TransformDirection(relativeGreenPosition);
            Vector3 bluePosition = screenCenter.transform.position + screenCenter.transform.TransformDirection(relativeBluePosition);

            redPosition = cam.WorldToScreenPoint(redPosition);
            greenPosition = cam.WorldToScreenPoint(greenPosition);
            bluePosition = cam.WorldToScreenPoint(bluePosition);

            StartCoroutine(ColorPickByRaycast(redPosition, 'r'));
            StartCoroutine(ColorPickByRaycast(greenPosition, 'g'));
            StartCoroutine(ColorPickByRaycast(bluePosition, 'b'));
        }
    }

    private Color CorrectColor(Color oldColor) {
        Color newColor = new Color();

        newColor.r = oldColor.r * correctionMatrix[0,0] + oldColor.g * correctionMatrix[0, 1] + oldColor.b * correctionMatrix[0, 2];
        newColor.g = oldColor.r * correctionMatrix[1,0] + oldColor.g * correctionMatrix[1, 1] + oldColor.b * correctionMatrix[1, 2];
        newColor.b = oldColor.r * correctionMatrix[2,0] + oldColor.g * correctionMatrix[2, 1] + oldColor.b * correctionMatrix[2, 2];

        return newColor;
    }

    private void ReplaceColors(Color newColor) {
        foreach (Material mat in mats) {
            mat.color = newColor;
        }
    }
}
