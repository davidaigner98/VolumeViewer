using Leap;
using Leap.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DetectorManager : MonoBehaviour {
    public static DetectorManager Instance { get; private set; }
    private bool isGrabbing;
    private bool isPinching;
    private InputAction cPressed;
    private InputAction vPressed;
    private bool debugInput = false;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        InputActionMap map = playerInput.currentActionMap;
        cPressed = map.FindAction("cPressed");
        cPressed.Enable();
        vPressed = map.FindAction("vPressed");
        vPressed.Enable();

        cPressed.started += DebugPerformPalmGrabOnWithLeftHand;
        cPressed.canceled += DebugPerformPalmGrabOff;
        vPressed.started += DebugPerformPalmGrabOnWithLeftHand;
        vPressed.canceled += DebugPerformPalmGrabOff;
    }

    private void DebugPerformPalmGrabOnWithLeftHand(InputAction.CallbackContext c) {
        PerformPalmGrabOn("left");
        debugInput = true;
    }

    private void DebugPerformPalmGrabOnWithRightHand(InputAction.CallbackContext c) {
        PerformPalmGrabOn("right");
        debugInput = true;
    }

    private void DebugPerformPalmGrabOff(InputAction.CallbackContext c) {
        debugInput = false;
        PerformPalmGrabOff();
    }

    public void PerformPalmGrabOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (debugInput) { return; }
            if (isPinching) { return; }
            
            Hand grabbingHand = Hands.Right;
            if (hand.Equals("left")) { grabbingHand = Hands.Left; }
            else if (hand.Equals("right")) { grabbingHand = Hands.Right; }

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

            if (nearestModel != null && shortestDistance <= 1.5f) {
                ModelInfo grabbedModel = nearestModel;
                ModelManager.Instance.SetSelectedModel(grabbedModel);
                grabbedModel.gameObject.GetComponent<ModelTransformator>().PalmGrabModelOn(hand);
                isGrabbing = true;
            }
        }
    }

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

    public void PerformOneFingerRotationOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            Hand interactingHand = Hands.Right;
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            Ray indexRay = new Ray(interactingHand.GetIndex().TipPosition, interactingHand.GetIndex().Direction);
            ModelInfo grabbedModel = GetModelByRaycast(indexRay);

            if (grabbedModel != null) {
                ModelManager.Instance.SetSelectedModel(grabbedModel);
                grabbedModel.gameObject.GetComponent<ModelTransformator>().OneFingerRotationOn(hand);
            }
        }
    }

    public void PerformOneFingerRotationOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
            if (selectedModel != null) {
                selectedModel.GetComponent<ModelTransformator>().OneFingerRotationOff();
            }
        }
    }

    public void PerformPinchOn(string hand) {
        if (isGrabbing) { return; }
        
        Hand currHand = null;
        if (hand.Equals("left")) { currHand = Hands.Left; }
        else if (hand.Equals("right")) { currHand = Hands.Right; }

        if (currHand != null && ClippingBox.Instance != null) {
            ClippingBox.Instance.StartPinchMovement(currHand);
            isPinching = true;
        }
    }

    public void PerformPinchOff() {
        if (ClippingBox.Instance != null) {
            ClippingBox.Instance.EndPinchMovement();
            isPinching = false;
        }
    }

    private ModelInfo GetModelByRaycast(Ray ray) {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo)) {
            return hitInfo.collider.gameObject.GetComponentInParent<ModelInfo>();
        }

        return null;
    }
}
