using Leap;
using Leap.Unity;
using UnityEngine;

public class ClippingBoxCorner : MonoBehaviour {
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

    public void StartGrabMovement(Hand grabbingHand) {
        isBeingGrabbed = true;
        this.grabbingHand = grabbingHand;
    }
    private void PerformGrabMovement() {
        clippingBox.UpdateCorner(gameObject, grabbingHand.GetPinchPosition());
    }

    public void EndGrabMovement() {
        isBeingGrabbed = false;
        grabbingHand = null;
    }

    private void UpdateVisibility() {
        if (!clippingBox.IsActive()) {
            SetAlpha(0);
            return;
        } else if (Hands.Left == null && Hands.Right == null) {
            SetAlpha(0);
            return;
        }

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

        float visibilityThreshold = 0.2f;
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
}
