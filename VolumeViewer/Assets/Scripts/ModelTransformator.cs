using Leap;
using Leap.Unity;
using UnityEngine;

public class ModelTransformator : MonoBehaviour {
    public Transform displaySize;
    public float releaseDistanceThreshold = 1.0f;
    private bool separatedFromDisplay = false;
    private bool isBeingGrabbed = false;
    private Hand grabbingHand;
    private Vector3 lastPalmPosition;

    // Start is called before the first frame update
    void Start() {
    
    }

    private void Rescale() {
        transform.localScale = Vector3.one * displaySize.localScale.y / 2;
        Debug.Log("scaled him down to " + transform.localScale);
    }

    // Update is called once per frame
    void Update() {
        if (isBeingGrabbed && lastPalmPosition != null) {
            Vector3 delta = grabbingHand.PalmPosition - lastPalmPosition;
            lastPalmPosition = grabbingHand.PalmPosition;

            transform.position += delta;
        }
    }

    public void PalmGrabModelOn(string hand) {
        if (hand.Equals("left")) { grabbingHand = Hands.Left; }
        else if (hand.Equals("right")) { grabbingHand = Hands.Right; }
        lastPalmPosition = grabbingHand.PalmPosition;

        Rescale();
        isBeingGrabbed = true;
        GetComponent<MeshRenderer>().enabled = true;
    }

    public void PalmGrabModelOff() {
        float distance = Vector3.Distance(transform.position, transform.parent.position);
        isBeingGrabbed = false;

        if (distance >= releaseDistanceThreshold) {
            //transform.SetParent(null);
            separatedFromDisplay = true;
        } else {
            GetComponent<MeshRenderer>().enabled = false;
            transform.localPosition = Vector3.zero;
        }
    }
}
