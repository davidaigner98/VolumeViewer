using Leap;
using Leap.Unity;
using System.Collections;
using UnityEngine;

public class ModelTransformator : MonoBehaviour {
    public bool isServer = false;
    public bool isStarted = false;
    public GameObject currentModel;
    public Shader transparentShader;
    public float palmGrabDistance = 1.0f;
    public float oneFingerRotationDistance = 1.0f;
    public float releaseDistanceThreshold = 1.0f;
    public float resetSpeed = 1.0f;
    private ModelSynchronizer synchronizer;
    private GameObject displayCenter;
    private Transform displaySize;
    private bool separatedFromDisplay = false;
    private bool inDisplay = true;
    private Hand interactingHand;
    private bool isBeingGrabbed = false;
    private bool isBeingRotated = false;
    private Vector3 lastPalmPosition;
    private Vector3 lastIndexPosition;

    public void SetupServer() {
        synchronizer = currentModel.GetComponent<ModelSynchronizer>();

        Material[] mats = currentModel.GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            mat.shader = transparentShader;
        }

        currentModel.GetComponent<MeshRenderer>().enabled = true;
    }

    public void SetupClient() {
        synchronizer = currentModel.GetComponent<ModelSynchronizer>();
        DisplayProfileManager profileManager = GameObject.Find("DisplayProjection").GetComponent<DisplayProfileManager>();
        displayCenter = profileManager.GetCurrentDisplayCenter();
        displaySize = profileManager.GetCurrentDisplaySize().transform;
        
        Rescale();
        palmGrabDistance = currentModel.transform.localScale.x * 2;
        oneFingerRotationDistance = currentModel.transform.localScale.x * 3 / 2;
        releaseDistanceThreshold = currentModel.transform.localScale.x * 4 / 5;

        currentModel.transform.SetParent(displayCenter.transform);
        currentModel.transform.localPosition = Vector3.zero;

        Material[] mats = currentModel.GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            mat.shader = transparentShader;
        }

        currentModel.GetComponent<MeshRenderer>().enabled = true;
        SetAlpha(0);
    }

    private void Update() {
        if (inDisplay && !isServer) {
            Vector3 inDisplayPosition = synchronizer.GetRelativeScreenOffsetServerRpc() * displaySize.localScale;
            currentModel.transform.localPosition = inDisplayPosition;
        }

        if (isStarted && !isServer) {
            if (isBeingGrabbed) { PalmGrabMovement(); }
            else if (isBeingRotated) { OneFingerRotation(); }
        }
    }

    private void PalmGrabMovement() {
        Vector3 delta = interactingHand.PalmPosition - lastPalmPosition;
        lastPalmPosition = interactingHand.PalmPosition;

        currentModel.transform.position += delta;

        float distance = Vector3.Distance(currentModel.transform.position, displayCenter.transform.position);
        if (distance < releaseDistanceThreshold) {
            SetAlpha(distance / releaseDistanceThreshold);
        } else {
            SetAlpha(1);
        }
    }

    private void OneFingerRotation() {
        float distance = Vector3.Distance(currentModel.transform.position, interactingHand.GetIndex().TipPosition);

        if (distance <= oneFingerRotationDistance) {
            Vector3 indexPosition = interactingHand.GetIndex().TipPosition - currentModel.transform.position;

            Vector3 axis = Vector3.Cross(indexPosition, lastIndexPosition);
            float angle = -Vector3.Angle(indexPosition, lastIndexPosition);

            synchronizer.RotateModelServerRpc(axis, angle);

            lastIndexPosition = indexPosition;
        }
    }

    public void PalmGrabModelOn(string hand) {
        if (isStarted && !isServer) {
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            float distance = Vector3.Distance(currentModel.transform.position, interactingHand.PalmPosition);
            if (distance <= palmGrabDistance) {
                Rescale();

                inDisplay = false;
                isBeingGrabbed = true;
                lastPalmPosition = interactingHand.PalmPosition;
            }
        }
    }

    public void PalmGrabModelOff() {
        if (isStarted && !isServer) { 
            float distance = Vector3.Distance(currentModel.transform.position, displayCenter.transform.position);
            isBeingGrabbed = false;

            if (distance >= releaseDistanceThreshold) {
                separatedFromDisplay = true;
                synchronizer.ChangeAttachmentButtonInteractabilityServerRpc(true);
            } else {
                StartCoroutine(MoveToOrigin());
            }
        }
    }

    private IEnumerator MoveToOrigin() {
        Vector3 offset = synchronizer.GetRelativeScreenOffsetServerRpc() * displaySize.localScale;
        Vector3 destination = displayCenter.transform.position + offset;
        float distanceToOrigin;

        do {
            Vector3 delta = currentModel.transform.position - destination;
            currentModel.transform.position -= delta.normalized * Time.deltaTime * resetSpeed;
            distanceToOrigin = Vector3.Distance(Vector3.zero, delta);
            SetAlpha(distanceToOrigin / releaseDistanceThreshold);

            yield return null;
        } while (distanceToOrigin > 0.01);

        SetAlpha(0);
        inDisplay = true;
        synchronizer.SetAttachedStateServerRpc(true);
        synchronizer.ChangeModelAttachment();
        synchronizer.ChangeAttachmentButtonInteractabilityServerRpc(false);
        currentModel.transform.localPosition = Vector3.zero;
    }

    public void SetAlpha(float alpha) {
        Material[] mats = currentModel.GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            Color newColor = mat.color;
            newColor.a = alpha;
            mat.color = newColor;
        }
    }

    private void Rescale() {
        currentModel.transform.localScale = Vector3.one * displaySize.localScale.y / 3;
    }
    
    public void OneFingerRotationOn(string hand) {
        if (hand.Equals("left")) { interactingHand = Hands.Left; }
        else if (hand.Equals("right")) { interactingHand = Hands.Right; }

        lastIndexPosition = interactingHand.GetIndex().TipPosition - currentModel.transform.position;
        isBeingRotated = true;
    }

    public void OneFingerRotationOff() {
        isBeingRotated = false;
    }

    public void AlignCoronal() {
        currentModel.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void AlignSagittal() {
        currentModel.transform.rotation = Quaternion.Euler(0, 270, 0);
    }

    public void AlignAxial() {
        currentModel.transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
