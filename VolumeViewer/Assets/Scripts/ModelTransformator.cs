using Leap;
using Leap.Unity;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class ModelTransformator : MonoBehaviour {
    public bool isConnected = false;
    public Transform displaySize;
    public float releaseDistanceThreshold = 1.0f;
    public float resetSpeed = 1.0f;
    private bool separatedFromDisplay = false;
    private Hand interactingHand;
    private bool isBeingGrabbed = false;
    private bool isBeingRotated = false;
    private Vector3 lastPalmPosition;
    private Vector3 lastIndexPosition;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (isConnected) {
            if (isBeingGrabbed) { PalmGrabMovement(); }
            else if (isBeingRotated) { OneFingerRotation(); }
        }
    }

    private void PalmGrabMovement() {
        Vector3 delta = interactingHand.PalmPosition - lastPalmPosition;
        lastPalmPosition = interactingHand.PalmPosition;

        transform.position += delta;

        float distance = Vector3.Distance(transform.position, transform.parent.position);
        if (distance < releaseDistanceThreshold) {
            SetAlpha(distance / releaseDistanceThreshold);
        } else {
            SetAlpha(1);
        }
    }

    private void OneFingerRotation() {
        Vector3 indexPosition = interactingHand.GetIndex().TipPosition - transform.position;

        float angle = Vector3.Angle(indexPosition, lastIndexPosition);
        Vector3 axis = Vector3.Cross(indexPosition, lastIndexPosition);
        transform.Rotate(axis, angle);

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
            float distance = Vector3.Distance(transform.position, transform.parent.position);
            isBeingGrabbed = false;

            if (distance >= releaseDistanceThreshold) {
                //transform.SetParent(null);
                separatedFromDisplay = true;
            } else {
                StartCoroutine(MoveToOrigin());
            }
        }
    }

    private IEnumerator MoveToOrigin() {
        float distanceToOrigin;

        do {
            Vector3 delta = transform.localPosition;
            distanceToOrigin = Vector3.Distance(Vector3.zero, delta);
            transform.localPosition -= delta.normalized * Time.deltaTime * resetSpeed;
            SetAlpha(distanceToOrigin / releaseDistanceThreshold);

            yield return null;
        } while (distanceToOrigin > 0.01);

        SetAlpha(0);
        transform.localPosition = Vector3.zero;
    }

    public void SetAlpha(float alpha) {
        Color newColor = GetComponent<Renderer>().material.color;
        newColor.a = alpha;
        GetComponent<Renderer>().material.color = newColor;
    }

    private void Rescale() {
        transform.localScale = Vector3.one * displaySize.localScale.y / 2;
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
