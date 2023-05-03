using System.Collections;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ColorCorrector : NetworkBehaviour {
    private Camera cam;
    private Material[] mats;
    private float timer;
    private Color[] originalColors;
    private Color trackedColor;

    // color correction matrix
    private float[,] correctionMatrix = new float[,] {
        { 1, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 1 }
    };

    void Start() {
        // assign the correct camera for either serverside or clientside
        if (CrossPlatformMediator.Instance.isServer) {
            cam = DisplayCameraPositioning.Instance.GetComponent<Camera>();
        } else {
            cam = GameObject.Find("XRRig/Camera Offset/Main Camera").GetComponent<Camera>();
        }

        // assign the model, its materials and copy their colors
        GameObject model = transform.Find("Model").gameObject;
        mats = model.GetComponent<Renderer>().materials;
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

    // makes a screenshot of the current clientside camera view and reads the color value of one pixel
    // continues to write the read pixel into the correction matrix
    private IEnumerator ColorPickByRaycast(Vector3 screenPosition, char quad) {
        // check if pixel position is valid
        if (screenPosition.x < 0 || screenPosition.x >= Screen.width) {
            if (screenPosition.y < 0 || screenPosition.y >= Screen.height) {
                if (screenPosition.z <= 0) {
                    yield return new WaitForEndOfFrame();

                    // make screenshot
                    Rect viewRect = cam.pixelRect;
                    Texture2D tex = new Texture2D((int)viewRect.width, (int)viewRect.height, TextureFormat.ARGB32, false);
                    tex.ReadPixels(viewRect, 0, 0, false);
                    tex.Apply(false);

                    // read color value of given pixel
                    trackedColor = tex.GetPixel((int)screenPosition.x, (int)screenPosition.y);

                    // write scanned color into color correction matrix
                    if (quad == 'r') {
                        Debug.Log("Scanned Red: " + trackedColor);
                        correctionMatrix[0, 0] = trackedColor.r;
                        //correctionMatrix[0, 1] = trackedColor.g;
                        //correctionMatrix[0, 2] = trackedColor.b;
                    } else if (quad == 'g') {
                        Debug.Log("Scanned Green: " + trackedColor);
                        //correctionMatrix[1, 0] = trackedColor.r;
                        correctionMatrix[1, 1] = trackedColor.g;
                        //correctionMatrix[1, 2] = trackedColor.b;
                    } else if (quad == 'b') {
                        Debug.Log("Scanned Blue: " + trackedColor);
                        //correctionMatrix[2, 0] = trackedColor.r;
                        //correctionMatrix[2, 1] = trackedColor.g;
                        correctionMatrix[2, 2] = trackedColor.b;
                    } else {
                        ReplaceColors(trackedColor);
                    }
                }
            }
        }
    }

    // spawns three debug balls serverside where the color quads should be
    private void SpawnDebugBalls() {
        if (CrossPlatformMediator.Instance.isServer) {
            GameObject s1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject s2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject s3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            s1.transform.position = cam.ScreenToWorldPoint(new Vector3(1920 * 0.975f, 1080 * 0.05f, 3));
            s2.transform.position = cam.ScreenToWorldPoint(new Vector3(1920 * 0.925f, 1080 * 0.05f, 3));
            s3.transform.position = cam.ScreenToWorldPoint(new Vector3(1920 * 0.875f, 1080 * 0.05f, 3));

            s1.transform.localScale = Vector3.one / 10;
            s2.transform.localScale = Vector3.one / 10;
            s3.transform.localScale = Vector3.one / 10;
        }
    }

    // tries to clientside scan the three color quads on serverside display
    private void ScanCorrectionMatrix() {
        if (!CrossPlatformMediator.Instance.isServer) {
            GameObject screenCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
            Vector3 screenSize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform.localScale;

            // calculate rough quad positions
            Vector3 relativeRedPosition = new Vector3(0.475f * screenSize.x, -0.45f * screenSize.y, 0);
            Vector3 relativeGreenPosition = new Vector3(0.425f * screenSize.x, -0.45f * screenSize.y, 0);
            Vector3 relativeBluePosition = new Vector3(0.375f * screenSize.x, -0.45f * screenSize.y, 0);

            // transform quad positions relative to display projection
            Vector3 redPosition = screenCenter.transform.position + screenCenter.transform.TransformDirection(relativeRedPosition);
            Vector3 greenPosition = screenCenter.transform.position + screenCenter.transform.TransformDirection(relativeGreenPosition);
            Vector3 bluePosition = screenCenter.transform.position + screenCenter.transform.TransformDirection(relativeBluePosition);

            // transform quad positions from world coordinate to screen point
            redPosition = cam.WorldToScreenPoint(redPosition);
            greenPosition = cam.WorldToScreenPoint(greenPosition);
            bluePosition = cam.WorldToScreenPoint(bluePosition);

            // start coroutines for reading pixel values
            StartCoroutine(ColorPickByRaycast(redPosition, 'r'));
            StartCoroutine(ColorPickByRaycast(greenPosition, 'g'));
            StartCoroutine(ColorPickByRaycast(bluePosition, 'b'));
        }
    }

    // corrects a given color value with the correction matrix
    private Color CorrectColor(Color oldColor) {
        Color newColor = new Color();

        // matrix multiplication
        newColor.r = oldColor.r * correctionMatrix[0,0] + oldColor.g * correctionMatrix[0, 1] + oldColor.b * correctionMatrix[0, 2];
        newColor.g = oldColor.r * correctionMatrix[1,0] + oldColor.g * correctionMatrix[1, 1] + oldColor.b * correctionMatrix[1, 2];
        newColor.b = oldColor.r * correctionMatrix[2,0] + oldColor.g * correctionMatrix[2, 1] + oldColor.b * correctionMatrix[2, 2];

        return newColor;
    }

    // replaces the color values in all materials
    private void ReplaceColors(Color newColor) {
        foreach (Material mat in mats) {
            mat.color = newColor;
        }
    }
}
