using Leap;
using Leap.Unity;
using System.Collections.Generic;
using UnityEngine;

public class DetectorManager : MonoBehaviour {
    public static DetectorManager Instance { get; private set; }
    public ConeTrigger leftConeTrigger, rightConeTrigger;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        if (!CrossPlatformMediator.Instance.isServer) {
            //leftConeTrigger.gameObject.SetActive(true);
            //rightConeTrigger.gameObject.SetActive(true);
        }
    }

    public void PerformPalmGrabOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            ConeTrigger coneTrigger = rightConeTrigger;
            if (hand.Equals("left")) { coneTrigger = leftConeTrigger; }
            else if (hand.Equals("right")) { coneTrigger = rightConeTrigger; }

            ModelInfo grabbedModel = coneTrigger.GetSelectedModel();
            if (grabbedModel != null) {
                ModelManager.Instance.SetSelectedModel(grabbedModel);
                grabbedModel.gameObject.GetComponent<ModelTransformator>().PalmGrabModelOn(hand);
            }
        }
    }

    public void PerformPalmGrabOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
            if (selectedModel != null) {
                selectedModel.GetComponent<ModelTransformator>().PalmGrabModelOff();
            }
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
            GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
            if (selectedModel != null) {
                selectedModel.GetComponent<ModelTransformator>().OneFingerRotationOff();
            }
        }
    }

    private ModelInfo GetModelByRaycast(Ray ray) {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo)) {
            return hitInfo.collider.gameObject.GetComponent<ModelInfo>();
        }

        return null;
    }
}
