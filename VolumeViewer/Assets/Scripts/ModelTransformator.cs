using Leap;
using Leap.Unity;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModelTransformator : NetworkBehaviour {
    public float palmGrabDistance = 1.0f;
    public float oneFingerRotationDistance = 1.0f;
    public float releaseDistanceThreshold = 1.0f;
    public float resetSpeed = 1.0f;
    private GameObject displayCenter;
    private Transform displaySize;
    public NetworkVariable<Vector2> screenOffset = new NetworkVariable<Vector2>(Vector2.zero);
    public NetworkVariable<float> scaleOnDisplay = new NetworkVariable<float>(1);
    public float scaleFactor = 0.085f;
    private bool inDisplay = true;
    private Hand interactingHand;
    private bool isBeingGrabbed = false;
    private bool isBeingRotated = false;
    private Vector3 lastPalmPosition;
    private Vector3 lastIndexPosition;
    private Collider collider;

    private void Start() {
        ModelManager.Instance.attached.OnValueChanged += ChangeModelAttachment;

        Material[] mats = transform.Find("Model").GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            mat.shader = Shader.Find("Custom/ModelShader");
        }

        collider = transform.Find("Model").GetComponent<Collider>();
        if (CrossPlatformMediator.Instance.isServer) { SetupServer(); }
        else { SetupClient(); }
    }

    public void SetupServer() {
        //SetAlpha(1);
        AlignCoronal();
    }

    public void SetupClient() {
        displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
        displaySize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform;
        
        Rescale();
        palmGrabDistance = transform.localScale.x * 2;
        oneFingerRotationDistance = transform.localScale.x * 3 / 2;
        releaseDistanceThreshold = transform.localScale.x * 4 / 5;
        scaleOnDisplay.OnValueChanged += Rescale;

        transform.SetParent(displayCenter.transform);
        transform.localPosition = Vector3.zero;

        //SetAlpha(0);

        UpdateClipScreenParameters();
    }

    private void Update() {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (isBeingGrabbed) { PalmGrabMovement(); }
            else if (isBeingRotated) { OneFingerRotation(); }

            if (inDisplay) {
                float zOffset = collider.bounds.extents.x;
                if (collider.bounds.extents.y > zOffset) { zOffset = collider.bounds.extents.y; }
                else if (collider.bounds.extents.z > zOffset) { zOffset = collider.bounds.extents.z; }

                Vector3 screenOffset = this.screenOffset.Value;
                screenOffset = new Vector3(screenOffset.x * displaySize.localScale.x, screenOffset.y * displaySize.localScale.y, zOffset * 1.01f);
                transform.position = displayCenter.transform.position + displayCenter.transform.TransformDirection(screenOffset);
            }
        }
    }

    private void UpdateClipScreenParameters() {
        GameObject displaySizeGO = DisplayProfileManager.Instance.GetCurrentDisplaySize();
        Vector3 displayCenter = displaySizeGO.transform.position;
        Vector2 displaySize = displaySizeGO.transform.localScale.xy();

        Vector3 screenCorner1 = displayCenter + displaySizeGO.transform.TransformDirection(new Vector3(+displaySize.x, +displaySize.y, 0) / 2);
        Vector3 screenCorner2 = displayCenter + displaySizeGO.transform.TransformDirection(new Vector3(-displaySize.x, +displaySize.y, 0) / 2);
        Vector3 screenCorner3 = displayCenter + displaySizeGO.transform.TransformDirection(new Vector3(-displaySize.x, -displaySize.y, 0) / 2);
        Vector3 screenCorner4 = displayCenter + displaySizeGO.transform.TransformDirection(new Vector3(+displaySize.x, -displaySize.y, 0) / 2);
        Vector3 screenNormal = displayCenter + displaySizeGO.transform.TransformDirection(Vector3.back);

        Material[] mats = transform.Find("Model").GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            if (inDisplay) { mat.SetInt("_InDisplay", 1); }
            else { mat.SetInt("_InDisplay", 0); }

            mat.SetVector("_ScreenCorner1", screenCorner1);
            mat.SetVector("_ScreenCorner2", screenCorner2);
            mat.SetVector("_ScreenCorner3", screenCorner3);
            mat.SetVector("_ScreenCorner4", screenCorner4);
            mat.SetVector("_ScreenNormal", screenNormal);
        }
    }

    private void PalmGrabMovement() {
        Vector3 delta = interactingHand.PalmPosition - lastPalmPosition;
        lastPalmPosition = interactingHand.PalmPosition;

        transform.position += delta;

        /*Vector3 screenOffset = this.screenOffset.Value;
        screenOffset = new Vector3(screenOffset.x * displaySize.localScale.x, screenOffset.y * displaySize.localScale.y, 0);
        float distance = Vector3.Distance(transform.position, displayCenter.transform.position + displayCenter.transform.TransformDirection(screenOffset));
        if (distance < releaseDistanceThreshold) {
            //SetAlpha(distance / releaseDistanceThreshold);
        } else {
            //SetAlpha(1);
        }*/
    }

    private void OneFingerRotation() {
        float distance = Vector3.Distance(transform.position, interactingHand.GetIndex().TipPosition);

        if (distance <= oneFingerRotationDistance) {
            Vector3 indexPosition = interactingHand.GetIndex().TipPosition - transform.position;

            Vector3 axis = Vector3.Cross(indexPosition, lastIndexPosition);
            float angle = -Vector3.Angle(indexPosition, lastIndexPosition);

            RotateModelServerRpc(axis, angle);

            lastIndexPosition = indexPosition;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle, Space.World);
    }

    public void PalmGrabModelOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            float distance = Vector3.Distance(transform.position, interactingHand.PalmPosition);
            if (distance <= palmGrabDistance) {
                Rescale();

                inDisplay = false;
                isBeingGrabbed = true;
                lastPalmPosition = interactingHand.PalmPosition;
                UpdateClipScreenParameters();
            }
        }
    }

    public void PalmGrabModelOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            Vector3 screenOffset = this.screenOffset.Value;
            screenOffset = new Vector3(screenOffset.x * displaySize.localScale.x, screenOffset.y * displaySize.localScale.y, 0);
            float distance = Vector3.Distance(transform.position, displayCenter.transform.position + displayCenter.transform.TransformDirection(screenOffset));
            isBeingGrabbed = false;

            if (distance >= releaseDistanceThreshold) {
                CrossPlatformMediator.Instance.ChangeAttachmentButtonInteractabilityServerRpc(true);
            } else {
                StartCoroutine(MoveToOrigin());
            }
        }
    }

    private IEnumerator MoveToOrigin() {
        float zOffset = collider.bounds.extents.x;
        if (collider.bounds.extents.y > zOffset) { zOffset = collider.bounds.extents.y; }
        else if (collider.bounds.extents.z > zOffset) { zOffset = collider.bounds.extents.z; }

        Vector3 screenOffset = this.screenOffset.Value;
        screenOffset = new Vector3(screenOffset.x * displaySize.localScale.x, screenOffset.y * displaySize.localScale.y, zOffset * 1.01f);
        Vector3 destination = displayCenter.transform.position + displayCenter.transform.TransformDirection(screenOffset);
        float distanceToOrigin;

        do {
            Vector3 delta = transform.position - destination;
            transform.position -= delta.normalized * Time.deltaTime * resetSpeed;
            distanceToOrigin = Vector3.Distance(Vector3.zero, delta);
            //SetAlpha(distanceToOrigin / releaseDistanceThreshold);

            yield return null;
        } while (distanceToOrigin > 0.01);

        //SetAlpha(0);
        inDisplay = true;
        ModelManager.Instance.SetAttachedStateServerRpc(true);
        ChangeModelAttachment(true, true);
        CrossPlatformMediator.Instance.ChangeAttachmentButtonInteractabilityServerRpc(false);
        transform.position = destination;
        UpdateClipScreenParameters();
    }

    public void ChangeModelAttachment(bool prev, bool current) {
        if (!CrossPlatformMediator.Instance.isServer) {
            GameObject displayProjection = GameObject.Find("DisplayProjection");
            if (displayProjection != null) {
                GameObject displayCenter = displayProjection.GetComponent<DisplayProfileManager>().GetCurrentDisplayCenter();

                if (current) {
                    GameObject modelParent = GameObject.Find("ModelParent");
                    transform.SetParent(displayCenter.transform);
                    Destroy(modelParent);
                } else {
                    GameObject modelParent = new GameObject("ModelParent");
                    modelParent.transform.rotation = displayCenter.transform.rotation;
                    transform.SetParent(modelParent.transform);
                }
            }
        }
    }

    /*public void SetAlpha(float alpha) {
        Material[] mats = transform.Find("Model").GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            Color newColor = mat.color;
            newColor.a = alpha;
            mat.color = newColor;
        }
    }*/

    private void Rescale(float prev = 1.0f, float curr = 1.0f) {
        transform.localScale = Vector3.one * scaleOnDisplay.Value * displaySize.localScale.y * scaleFactor;
    }
    
    public void OneFingerRotationOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            lastIndexPosition = interactingHand.GetIndex().TipPosition - transform.position;
            isBeingRotated = true;
        }
    }

    public void OneFingerRotationOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            isBeingRotated = false;
        }
    }

    public void AlignCoronal() {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void AlignSagittal() {
        transform.rotation = Quaternion.Euler(0, 270, 0);
    }

    public void AlignAxial() {
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
