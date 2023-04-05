using Leap;
using Leap.Unity;
using UnityEngine;

public class ClippingBoxGrabbableCorner : MonoBehaviour {
    private ClippingBox clippingBox;
    public Vector3 cornerIndex;
    private bool isBeingGrabbed = false;
    private Hand grabbingHand;

    private void Start() {
        clippingBox = transform.parent.parent.GetComponent<ClippingBox>();
        UpdatePosition();
    }

    private void Update() {
        if (clippingBox.IsActive()) {
            UpdateVisibility();
            if (isBeingGrabbed) { PerformGrabMovement(); }
        }
    }

    public void StartGrabMovement(Hand grabbingHand) {
        isBeingGrabbed = true;
        this.grabbingHand = grabbingHand;
    }
    private void PerformGrabMovement() {
        transform.position = grabbingHand.GetPinchPosition();
        clippingBox.UpdateCorners(cornerIndex, transform.position);
    }

    public void EndGrabMovement() {
        isBeingGrabbed = false;
        grabbingHand = null;
    }

    private void UpdateVisibility() {
        float distance;
        
        if (Hands.Left == null && Hands.Right == null) {
            SetAlpha(0);
            return;
        }

        if (Hands.Left == null) {
            distance = Vector3.Distance(Hands.Right.PalmPosition, transform.position);
        } else if (Hands.Right == null) {
            distance = Vector3.Distance(Hands.Left.PalmPosition, transform.position);
        } else {
            float leftDistance = Vector3.Distance(Hands.Left.PalmPosition, transform.position);
            float rightDistance = Vector3.Distance(Hands.Right.PalmPosition, transform.position);

            distance = rightDistance;
            if (leftDistance < rightDistance) { distance = leftDistance; }
        }

        float visibilityThreshold = 0.5f;
        if (distance < visibilityThreshold) {
            SetAlpha(1 - distance / visibilityThreshold);
        } else {
            SetAlpha(0);
        }
    }

    private void SetAlpha(float alpha) {
        Color oldColor = GetComponent<MeshRenderer>().material.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
        GetComponent<MeshRenderer>().material.color = newColor;
    }

    public void UpdatePosition() {
        transform.position = GetSupposedPosition();
    }

    private Vector3 GetSupposedPosition() {
        float x = GetValueOfSupposedPosition('x');
        float y = GetValueOfSupposedPosition('y');
        float z = GetValueOfSupposedPosition('z');

        return new Vector3(x, y, z);
    }

    private float GetValueOfSupposedPosition(char coordinate) {
        if (coordinate.Equals('x')) {
            if (cornerIndex.x == 1) { return clippingBox.maxBounds.x; }
            else { return clippingBox.minBounds.x; }
        } else if (coordinate.Equals('y')) {
            if (cornerIndex.y == 1) { return clippingBox.maxBounds.y; }
            else { return clippingBox.minBounds.y; }
        } else if (coordinate.Equals('z')) {
            if (cornerIndex.z == 1) { return clippingBox.maxBounds.z; }
            else { return clippingBox.minBounds.z; }
        }

        return 0;
    }
}
