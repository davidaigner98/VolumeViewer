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
    public NetworkVariable<Vector3> screenOffset = new NetworkVariable<Vector3>(new Vector3(0, 0, 1));
    public NetworkVariable<float> scaleOnDisplay = new NetworkVariable<float>(1);
    public float scaleFactor = 0.375f;
    private Hand interactingHand;
    private bool isBeingGrabbed = false;
    private bool isBeingRotated = false;
    private Vector3 lastPalmPosition;
    private Vector3 lastIndexPosition;
    private float zPositionFactor = 0.2f;

    private void Awake() {
        Material[] mats = transform.Find("Model").GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            mat.shader = Shader.Find("Custom/ModelShader");
            mat.SetVector("_ModelOffset", transform.Find("Model").localPosition);
        }
    }

    private void Start() {
        ModelManager.Instance.attached.OnValueChanged += ChangeModelAttachment;

        // triggers setup process
        if (CrossPlatformMediator.Instance.isServer) { SetupServer(); }
        else { SetupClient(); }
    }

    // sets up a new model serverside
    public void SetupServer() {
        UpdateClipScreenParametersServerside();
        screenOffset.OnValueChanged += AdjustPositionServerside;
        AlignCoronal();
    }

    // sets up a new model clientside
    public void SetupClient() {
        displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
        displaySize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform;
        screenOffset.OnValueChanged += AdjustPositionClientside;
        Rescale();

        // configures thresholds and distances
        palmGrabDistance = transform.localScale.x * 1.5f;
        oneFingerRotationDistance = transform.localScale.x * 3 / 2;
        releaseDistanceThreshold = transform.localScale.x * 4 / 5;
        scaleOnDisplay.OnValueChanged += Rescale;

        transform.SetParent(displayCenter.transform);
        AdjustPositionClientside(Vector3.zero, screenOffset.Value);

        
        UpdateClipScreenParametersClientside();
    }

    private void Update() {
        // handle clientside palm grab movement and one finger rotation
        if (!CrossPlatformMediator.Instance.isServer) {
            if (isBeingGrabbed) { PalmGrabMovement(); }
            else if (isBeingRotated) { OneFingerRotation(); }
        }
    }

    // update shader properties regarding the clipping screen mechanic serverside
    private void UpdateClipScreenParametersServerside() {
        DisplayCameraPositioning displayCameraPositioner = DisplayCameraPositioning.Instance;
        Vector2 viewportSize = displayCameraPositioner.viewportSize;

        // calculate four corners of the display projection and its normal vector
        Vector3 screenCorner1 = new Vector3(-viewportSize.x * 10, +viewportSize.y * 10, 0);
        Vector3 screenCorner2 = new Vector3(+viewportSize.x * 10, +viewportSize.y * 10, 0);
        Vector3 screenCorner3 = new Vector3(+viewportSize.x * 10, -viewportSize.y * 10, 0);
        Vector3 screenCorner4 = new Vector3(-viewportSize.x * 10, -viewportSize.y * 10, 0);
        Vector3 screenNormal = Vector3.forward;

        // update all materials
        Material[] mats = transform.Find("Model").GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            mat.SetInt("_InDisplay", 0);

            mat.SetVector("_ScreenCorner1", screenCorner1);
            mat.SetVector("_ScreenCorner2", screenCorner2);
            mat.SetVector("_ScreenCorner3", screenCorner3);
            mat.SetVector("_ScreenCorner4", screenCorner4);
            mat.SetVector("_ScreenNormal", screenNormal);
        }
    }

    // update shader properties regarding the clipping screen mechanic clientside
    private void UpdateClipScreenParametersClientside() {
        GameObject displaySizeGO = DisplayProfileManager.Instance.GetCurrentDisplaySize();
        Vector3 displayCenter = displaySizeGO.transform.position;
        Vector2 displaySize = displaySizeGO.transform.localScale.xy();

        // calculate four corners of the display projection and its normal vector
        Vector3 screenCorner1 = displayCenter + displaySizeGO.transform.TransformDirection(new Vector3(-displaySize.x, +displaySize.y, 0) / 2);
        Vector3 screenCorner2 = displayCenter + displaySizeGO.transform.TransformDirection(new Vector3(+displaySize.x, +displaySize.y, 0) / 2);
        Vector3 screenCorner3 = displayCenter + displaySizeGO.transform.TransformDirection(new Vector3(+displaySize.x, -displaySize.y, 0) / 2);
        Vector3 screenCorner4 = displayCenter + displaySizeGO.transform.TransformDirection(new Vector3(-displaySize.x, -displaySize.y, 0) / 2);
        Vector3 screenNormal = displayCenter + displaySizeGO.transform.TransformDirection(Vector3.back);

        // update all materials
        Material[] mats = transform.Find("Model").GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            mat.SetInt("_InDisplay", 0);

            mat.SetVector("_ScreenCorner1", screenCorner1);
            mat.SetVector("_ScreenCorner2", screenCorner2);
            mat.SetVector("_ScreenCorner3", screenCorner3);
            mat.SetVector("_ScreenCorner4", screenCorner4);
            mat.SetVector("_ScreenNormal", screenNormal);
        }
    }

    // perform palm grab movement
    private void PalmGrabMovement() {
        Vector3 delta = interactingHand.PalmPosition - lastPalmPosition;
        lastPalmPosition = interactingHand.PalmPosition;

        transform.position += delta;
        Transform screenCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter().transform;
        Vector3 screenSize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform.localScale;
        Vector3 newOffset = screenCenter.InverseTransformDirection(transform.position - screenCenter.position) / screenSize.x;
        newOffset = new Vector3(newOffset.x, newOffset.y, newOffset.z / zPositionFactor);
        SetModelScreenOffsetServerRpc(newOffset);
    }

    // clientside call to the server for changing the screen offset of this model
    [ServerRpc(RequireOwnership = false)]
    private void SetModelScreenOffsetServerRpc(Vector3 newOffset) {
        if (CrossPlatformMediator.Instance.isServer) {
            screenOffset.Value = newOffset;
        }
    }

    // adjust serverside model position regarding to screen offset
    private void AdjustPositionServerside(Vector3 oldOffset, Vector3 newOffset) {
        Vector3 viewportSize = DisplayCameraPositioning.Instance.viewportSize;
        transform.position = new Vector3(newOffset.x * viewportSize.x, newOffset.y * viewportSize.x, newOffset.z);
    }

    // adjust clientside model position regarding to screen offset
    private void AdjustPositionClientside(Vector3 oldOffset, Vector3 newOffset) {
        if (isBeingGrabbed) { return; }

        Transform screenCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter().transform;
        Vector3 screenSize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform.localScale;

        newOffset = new Vector3(newOffset.x, newOffset.y, newOffset.z * zPositionFactor);
        transform.position = screenCenter.position + screenCenter.TransformDirection(newOffset * screenSize.x);
    }

    // perform one finger rotation
    private void OneFingerRotation() {
        float distance = Vector3.Distance(transform.position, interactingHand.GetIndex().TipPosition);

        // check if model is in range
        if (distance <= oneFingerRotationDistance) {
            Vector3 indexPosition = interactingHand.GetIndex().TipPosition - transform.position;

            // calculate axis and angle
            Vector3 axis = Vector3.Cross(indexPosition, lastIndexPosition);
            axis = new Vector3(-axis.x, axis.y, -axis.z);
            float angle = -Vector3.Angle(indexPosition, lastIndexPosition);

            // perform rotation
            RotateModelServerRpc(axis, angle);

            lastIndexPosition = indexPosition;
        }
    }

    // clientside call to the server for rotating the model
    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle, Space.World);
    }

    // start palm grab movement
    public void PalmGrabModelOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            // check if model is in range
            float distance = Vector3.Distance(transform.position, interactingHand.PalmPosition);
            if (distance <= palmGrabDistance) {
                Rescale();

                isBeingGrabbed = true;
                lastPalmPosition = interactingHand.PalmPosition;
                UpdateClipScreenParametersClientside();
            }
        }
    }

    // end palm grab movement
    public void PalmGrabModelOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            Vector3 screenOffset = this.screenOffset.Value;
            screenOffset = new Vector3(screenOffset.x * displaySize.localScale.x, screenOffset.y * displaySize.localScale.y, 0);
            float distance = Vector3.Distance(transform.position, displayCenter.transform.position + displayCenter.transform.TransformDirection(screenOffset));
            isBeingGrabbed = false;

            if (distance >= releaseDistanceThreshold) {
                CrossPlatformMediator.Instance.ChangeAttachmentButtonInteractabilityServerRpc(true);
            }

            // if model is too "deep in the screen", reset it to z position 1
            if (this.screenOffset.Value.z > 1) {
                StartCoroutine(MoveToZ1());
            }
        }
    }

    // coroutine for gradiently moving the model to z position 1
    private IEnumerator MoveToZ1() {
        Vector3 screenOffset = this.screenOffset.Value;
        Vector3 destination = new Vector3(screenOffset.x, screenOffset.y, 1);
        float distanceToDestination;

        // while the model is not on z position 1
        do {
            Vector3 delta = destination - screenOffset;
            screenOffset += delta.normalized * Time.deltaTime * resetSpeed;
            SetModelScreenOffsetServerRpc(screenOffset);
            
            // calculate new distance to destination
            distanceToDestination = Vector3.Distance(destination, screenOffset);
            
            yield return null;
        } while (distanceToDestination > 0.1);

        ModelManager.Instance.SetAttachedStateServerRpc(true);
        ChangeModelAttachment(true, true);
        CrossPlatformMediator.Instance.ChangeAttachmentButtonInteractabilityServerRpc(false);
        SetModelScreenOffsetServerRpc(screenOffset);
        UpdateClipScreenParametersClientside();
    }

    // changes the clientside models attachement to the display projection (e.g. if the model is a child of the display projection or not)
    public void ChangeModelAttachment(bool prev, bool current) {
        if (!CrossPlatformMediator.Instance.isServer) {
            GameObject displayProjection = GameObject.Find("DisplayProjection");
            if (displayProjection != null) {
                GameObject displayCenter = displayProjection.GetComponent<DisplayProfileManager>().GetCurrentDisplayCenter();

                if (current) {          // attach to display projection
                    GameObject modelParent = GameObject.Find("ModelParent");
                    transform.SetParent(displayCenter.transform);
                    Destroy(modelParent);
                } else {                // detach from display projection
                    GameObject modelParent = new GameObject("ModelParent");
                    modelParent.transform.rotation = displayCenter.transform.rotation;
                    transform.SetParent(modelParent.transform);
                }
            }
        }
    }

    // rescales the model, respective to the scaleOnDisplay factor
    private void Rescale(float prev = 1.0f, float curr = 1.0f) {
        transform.localScale = Vector3.one * scaleOnDisplay.Value * displaySize.localScale.y * scaleFactor;
    }
    
    // start one finger rotation
    public void OneFingerRotationOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            lastIndexPosition = interactingHand.GetIndex().TipPosition - transform.position;
            isBeingRotated = true;
        }
    }

    // end one finger rotation
    public void OneFingerRotationOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            isBeingRotated = false;
        }
    }

    // reset the models screen offset (and therefore its position) to (0, 0, 0)
    public void ResetToCenter() {
        if (CrossPlatformMediator.Instance.isServer) {
            screenOffset.Value = new Vector3(0, 0, 1);
        }
    }

    // align the model with its front to the camera
    public void AlignCoronal() {
        if (CrossPlatformMediator.Instance.isServer) {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    // align the model with its side to the camera
    public void AlignSagittal() {
        if (CrossPlatformMediator.Instance.isServer) {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }

    // align the model with its top to the camera
    public void AlignAxial() {
        if (CrossPlatformMediator.Instance.isServer) {
            transform.rotation = Quaternion.Euler(90, 180, 0);
        }
    }
}
