using Leap;
using Leap.Unity;
using System.Collections;
using UnityEngine;

public class ModelTransformator : MonoBehaviour {
    public GameObject currentModel;
    public Shader transparentShader;
    public bool isConnected = false;
    public Transform displaySize;
    public float releaseDistanceThreshold = 1.0f;
    public float resetSpeed = 1.0f;
    private ModelSynchronizer synchronizer;
    private bool separatedFromDisplay = false;
    private Hand interactingHand;
    private bool isBeingGrabbed = false;
    private bool isBeingRotated = false;
    private Vector3 lastPalmPosition;
    private Vector3 lastIndexPosition;

    private void Start() {
        synchronizer = currentModel.GetComponent<ModelSynchronizer>();

        Material[] mats = currentModel.GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            mat.shader = transparentShader;
        }

        SetAlpha(0);
    }

    private void Update() {
        if (isConnected) {
            if (isBeingGrabbed) { PalmGrabMovement(); }
            else if (isBeingRotated) { OneFingerRotation(); }
        }
    }

    private void PalmGrabMovement() {
        Vector3 delta = interactingHand.PalmPosition - lastPalmPosition;
        lastPalmPosition = interactingHand.PalmPosition;

        currentModel.transform.position += delta;

        float distance = Vector3.Distance(currentModel.transform.position, currentModel.transform.parent.position);
        if (distance < releaseDistanceThreshold) {
            SetAlpha(distance / releaseDistanceThreshold);
        } else {
            SetAlpha(1);
        }
    }

    private void OneFingerRotation() {
        Vector3 indexPosition = interactingHand.GetIndex().TipPosition - currentModel.transform.position;

        Vector3 axis = Vector3.Cross(indexPosition, lastIndexPosition);
        float angle = -Vector3.Angle(indexPosition, lastIndexPosition);
        
        synchronizer.RotateModelServerRpc(axis, angle);
        
        lastIndexPosition = indexPosition;
    }

    public void PalmGrabModelOn(string hand) {
        if (isConnected) {
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }
            lastPalmPosition = interactingHand.PalmPosition;

            Rescale();
            isBeingGrabbed = true;
        }
    }

    public void PalmGrabModelOff() {
        if (isConnected) { 
            float distance = Vector3.Distance(currentModel.transform.position, currentModel.transform.parent.position);
            isBeingGrabbed = false;

            if (distance >= releaseDistanceThreshold) {
                //currentModel.transform.SetParent(null);
                separatedFromDisplay = true;
            } else {
                StartCoroutine(MoveToOrigin());
            }
        }
    }

    private IEnumerator MoveToOrigin() {
        float distanceToOrigin;

        do {
            Vector3 delta = currentModel.transform.localPosition;
            distanceToOrigin = Vector3.Distance(Vector3.zero, delta);
            currentModel.transform.localPosition -= delta.normalized * Time.deltaTime * resetSpeed;
            SetAlpha(distanceToOrigin / releaseDistanceThreshold);

            yield return null;
        } while (distanceToOrigin > 0.01);

        SetAlpha(0);
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
        currentModel.transform.localScale = Vector3.one * displaySize.localScale.y / 2;
    }
    
    public void OneFingerRotationOn(string hand) {
        if (hand.Equals("left")) { interactingHand = Hands.Left; }
        else if (hand.Equals("right")) { interactingHand = Hands.Right; }

        lastIndexPosition = interactingHand.GetIndex().TipPosition - transform.position;
        isBeingRotated = true;
    }

    public void OneFingerRotationOff() {
        isBeingRotated = false;
    }
}
