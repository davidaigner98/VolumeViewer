using Leap.Unity;
using System;
using UnityEngine;

public class ClippingBoxGrabbableCorner : MonoBehaviour {
    private ClippingBox clippingBox;
    public Vector3 cornerIndex;

    private void Start() {
        clippingBox = transform.parent.parent.GetComponent<ClippingBox>();
    }

    private void Update() {
        CheckForPositionChange();

        if (clippingBox.IsActive()) { UpdateVisibility(); } 
    }

    private void CheckForPositionChange() {
        if (transform.hasChanged) {
            transform.hasChanged = false;
            clippingBox.UpdateCorners(cornerIndex, transform.position);
        }
    }

    private void UpdateVisibility() {
        float leftDistance = Vector3.Distance(Hands.Left.PalmPosition, transform.position);
        float rightDistance = Vector3.Distance(Hands.Right.PalmPosition, transform.position);

        float distance = rightDistance;
        if (leftDistance < rightDistance) { distance = leftDistance; }

        float visibilityThreshold = 0.5f;
        if (distance < visibilityThreshold) {
            Color oldColor = GetComponent<MeshRenderer>().material.color;
            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, distance / visibilityThreshold);

            GetComponent<MeshRenderer>().material.color = newColor;
        }
    }

    public void UpdatePosition() {
        transform.position = GetSupposedPosition();
        transform.hasChanged = false;
    }

    private Vector3 GetSupposedPosition() {
        Vector3 minBounds = clippingBox.minBounds;
        Vector3 maxBounds = clippingBox.maxBounds;

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
