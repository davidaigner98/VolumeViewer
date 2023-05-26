using Leap;
using Leap.Unity;
using UnityEngine;

public enum CornerType {
    Corner,
    Face
}

public class ClippingBoxGrabbable : MonoBehaviour {
    public CornerType type;
    private ClippingBox clippingBox;
    private bool isBeingGrabbed = false;
    private Hand grabbingHand;

    private void Start() {
        clippingBox = transform.parent.parent.GetComponent<ClippingBox>();
    }

    private void Update() {
        UpdateVisibility();

        if (clippingBox.IsActive() && isBeingGrabbed) {
            PerformGrabMovement();
        }
    }

    // start grab movement for this corner
    public void StartGrabMovement(Hand grabbingHand) {
        isBeingGrabbed = true;
        this.grabbingHand = grabbingHand;
    }

    // perform grab movement for this corner
    private void PerformGrabMovement() {
        Vector3 currPinchPosition = grabbingHand.GetPinchPosition();

        if (type.Equals(CornerType.Corner)) {
            clippingBox.UpdateCorner(gameObject, currPinchPosition);
        } else if (type.Equals(CornerType.Face)) {
            clippingBox.ShiftBoundary(currPinchPosition - transform.localPosition);
        }
    }

    // end grab movement for this corner
    public void EndGrabMovement() {
        isBeingGrabbed = false;
        grabbingHand = null;
    }

    // update the gradual visibility of the corner
    private void UpdateVisibility() {
        if (!clippingBox.IsActive()) {
            // make corner invisible, if clipping box is deactivated
            SetAlpha(0);
            return;
        } else if (Hands.Left == null && Hands.Right == null) {
            // make corner invisible, if neither hand is present
            SetAlpha(0);
            return;
        }

        // calculate the distance between corner and the interacting hands index tip position
        float distance;
        if (Hands.Left == null) {
            distance = Vector3.Distance(Hands.Right.GetIndex().TipPosition, transform.position);
        } else if (Hands.Right == null) {
            distance = Vector3.Distance(Hands.Left.GetIndex().TipPosition, transform.position);
        } else {
            float leftDistance = Vector3.Distance(Hands.Left.GetIndex().TipPosition, transform.position);
            float rightDistance = Vector3.Distance(Hands.Right.GetIndex().TipPosition, transform.position);

            distance = rightDistance;
            if (leftDistance < rightDistance) { distance = leftDistance; }
        }

        // set visibility according to the calculated distance, if in range
        float visibilityThreshold = 0.2f;
        if (distance < visibilityThreshold) {
            SetAlpha(1 - distance / visibilityThreshold);
        } else {
            SetAlpha(0);
        }
    }

    // sets the visibility of this corner, using the alpha value of the material
    private void SetAlpha(float alpha) {
        // assign a new color with the same rgb value but a new alpha value
        Color oldColor = GetComponent<MeshRenderer>().material.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
        GetComponent<MeshRenderer>().material.color = newColor;
    }
}
