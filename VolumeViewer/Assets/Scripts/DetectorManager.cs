using Leap;
using Leap.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DetectorManager : MonoBehaviour {
    public static DetectorManager Instance { get; private set; }

    // state flags
    private bool isGrabbing;
    private bool isPointing;
    private bool isPinching;

    // debug input actions
    private InputAction ePressed;
    private InputAction rPressed;
    private InputAction dPressed;
    private InputAction fPressed;
    private InputAction cPressed;
    private InputAction vPressed;

    // debug input flag
    private bool debugInput = false;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        // initialize and enable input actions
        PlayerInput playerInput = GetComponent<PlayerInput>();
        InputActionMap map = playerInput.currentActionMap;
        ePressed = map.FindAction("ePressed");
        ePressed.Enable();
        rPressed = map.FindAction("rPressed");
        rPressed.Enable();
        dPressed = map.FindAction("dPressed");
        dPressed.Enable();
        fPressed = map.FindAction("fPressed");
        fPressed.Enable();
        cPressed = map.FindAction("cPressed");
        cPressed.Enable();
        vPressed = map.FindAction("vPressed");
        vPressed.Enable();

        // map methods to input action events
        // debug palm grabbing
        ePressed.started += DebugPerformPalmGrabOnWithLeftHand;
        ePressed.canceled += DebugPerformPalmGrabOff;
        rPressed.started += DebugPerformPalmGrabOnWithRightHand;
        rPressed.canceled += DebugPerformPalmGrabOff;

        // debug one finger rotation
        dPressed.started += DebugPerformOneFingerRotationOnWithLeftHand;
        dPressed.canceled += DebugPerformOneFingerRotationOff;
        fPressed.started += DebugPerformOneFingerRotationOnWithRightHand;
        fPressed.canceled += DebugPerformOneFingerRotationOff;

        // debug clipping box corner pinching
        cPressed.started += DebugPerformPinchOnWithLeftHand;
        cPressed.canceled += DebugPerformPinchOff;
        vPressed.started += DebugPerformPinchOnWithRightHand;
        vPressed.canceled += DebugPerformPinchOff;
    }

    // method for triggering a debug palm grab with the left hand
    private void DebugPerformPalmGrabOnWithLeftHand(InputAction.CallbackContext c) {
        if (Hands.Left == null) { return; }

        if (isGrabbing) { PerformPalmGrabOff(); }

        PerformPalmGrabOn("left");
        debugInput = true;
    }

    // method for triggering a debug palm grab with the right hand
    private void DebugPerformPalmGrabOnWithRightHand(InputAction.CallbackContext c) {
        if (Hands.Right == null) { return; }

        if (isGrabbing) { PerformPalmGrabOff(); }

        PerformPalmGrabOn("right");
        debugInput = true;
    }

    // ends the debug palm grab
    private void DebugPerformPalmGrabOff(InputAction.CallbackContext c) {
        debugInput = false;
        PerformPalmGrabOff();
    }

    // starts the palm grab
    public void PerformPalmGrabOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (debugInput) { return; }
            if (isPinching) { return; }
            
            // get the interacting hand
            Hand grabbingHand = Hands.Right;
            if (hand.Equals("left")) { grabbingHand = Hands.Left; }
            else if (hand.Equals("right")) { grabbingHand = Hands.Right; }

            // find the closest model to the interacting hand
            ModelInfo nearestModel = null;
            float shortestDistance = -1;
            List<ModelInfo> allModels = ModelManager.Instance.modelInfos;
            foreach(ModelInfo currModel in allModels) {
                float currDistance = Vector3.Distance(grabbingHand.PalmPosition, currModel.transform.position);
                
                if (nearestModel == null || currDistance < shortestDistance) {
                    nearestModel = currModel;
                    shortestDistance = currDistance;
                }
            }

            // if in range, perform palm grab
            if (nearestModel != null && shortestDistance <= 1.5f) {
                ModelInfo grabbedModel = nearestModel;
                ModelManager.Instance.SetSelectedModel(grabbedModel);
                grabbedModel.gameObject.GetComponent<ModelTransformator>().PalmGrabModelOn(hand);
                isGrabbing = true;
            }
        }
    }

    // ends the palm grab
    public void PerformPalmGrabOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (debugInput) { return; }
            
            ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
            if (selectedModel != null) {
                selectedModel.GetComponent<ModelTransformator>().PalmGrabModelOff();
            }

            isGrabbing = false;
        }
    }

    // triggers a debug one finger rotation with the left hand
    private void DebugPerformOneFingerRotationOnWithLeftHand(InputAction.CallbackContext c) {
        if (Hands.Left == null) { return; }

        if (isPointing) { PerformOneFingerRotationOff(); }

        PerformOneFingerRotationOn("left");
        debugInput = true;
    }

    // triggers a debug one finger rotation with the right hand
    private void DebugPerformOneFingerRotationOnWithRightHand(InputAction.CallbackContext c) {
        if (Hands.Right == null) { return; }

        if (isPointing) { PerformOneFingerRotationOff(); }

        PerformOneFingerRotationOn("right");
        debugInput = true;
    }

    // ends the debug one finger rotation
    private void DebugPerformOneFingerRotationOff(InputAction.CallbackContext c) {
        debugInput = false;
        PerformOneFingerRotationOff();
    }

    // starts the one finger rotation
    public void PerformOneFingerRotationOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (debugInput) { return; }

            // get interacting hand
            Hand interactingHand = Hands.Right;
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            // try to find the model, the index finger points to
            Ray indexRay = new Ray(interactingHand.GetIndex().TipPosition, interactingHand.GetIndex().Direction);
            ModelInfo grabbedModel = GetModelByRaycast(indexRay);

            // start one finger rotation
            if (grabbedModel != null) {
                ModelManager.Instance.SetSelectedModel(grabbedModel);
                grabbedModel.gameObject.GetComponent<ModelTransformator>().OneFingerRotationOn(hand);
                isPointing = true;
            }
        }
    }

    // ends the one finger rotation
    public void PerformOneFingerRotationOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (debugInput) { return; }

            ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
            if (selectedModel != null) {
                selectedModel.GetComponent<ModelTransformator>().OneFingerRotationOff();
                isPointing = false;
            }
        }
    }

    // triggers the corner pinching with the left hand
    private void DebugPerformPinchOnWithLeftHand(InputAction.CallbackContext c) {
        if (Hands.Left == null) { return; }

        if (isPinching) { PerformPinchOff(); }

        PerformPinchOn("left");
        debugInput = true;
    }

    // triggers the corner pinching with the right hand
    private void DebugPerformPinchOnWithRightHand(InputAction.CallbackContext c) {
        if (Hands.Right == null) { return; }

        if (isPinching) { PerformPinchOff(); }

        PerformPinchOn("right");
        debugInput = true;
    }

    // ends the debug corner pinching
    private void DebugPerformPinchOff(InputAction.CallbackContext c) {
        debugInput = false;
        PerformPinchOff();
    }

    // starts the corner pinching
    public void PerformPinchOn(string hand) {
        if (debugInput) { return; }
        if (isGrabbing) { return; }
        
        Hand currHand = null;
        if (hand.Equals("left")) { currHand = Hands.Left; }
        else if (hand.Equals("right")) { currHand = Hands.Right; }

        if (currHand != null && ClippingBox.Instance != null) {
            ClippingBox.Instance.StartPinchMovement(currHand);
            isPinching = true;
        }
    }

    // ends the corner pinching
    public void PerformPinchOff() {
        if (debugInput) { return; }

        if (ClippingBox.Instance != null) {
            ClippingBox.Instance.EndPinchMovement();
            isPinching = false;
        }
    }

    // returns the model hit by a specific raycast
    private ModelInfo GetModelByRaycast(Ray ray) {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo)) {
            return hitInfo.collider.gameObject.GetComponentInParent<ModelInfo>();
        }

        return null;
    }
}
