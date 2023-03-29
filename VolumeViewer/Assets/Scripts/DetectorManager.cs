using Leap;
using Leap.Unity;
using System.Collections.Generic;
using UnityEngine;

public class DetectorManager : MonoBehaviour {
    public static DetectorManager Instance { get; private set; }
    public List<PalmDirectionDetector> dependentPalmDetectors = new List<PalmDirectionDetector>();
    public List<FingerDirectionDetector> dependentFingerDetectors = new List<FingerDirectionDetector>();

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    private void Start() {
        ModelManager.Instance.OnSelectionChanged += UpdateDetectors;
    }

    public void UpdateDetectors() {
        if (!CrossPlatformMediator.Instance.isServer) {
            GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
            if (selectedModel != null) {
                foreach (PalmDirectionDetector detector in dependentPalmDetectors) {
                    detector.gameObject.SetActive(true);
                    detector.TargetObject = selectedModel.transform;
                }

                foreach (FingerDirectionDetector detector in dependentFingerDetectors) {
                    detector.gameObject.SetActive(true);
                    detector.TargetObject = selectedModel.transform;
                }
            } else {
                foreach (PalmDirectionDetector detector in dependentPalmDetectors) {
                    detector.gameObject.SetActive(false);
                }

                foreach (FingerDirectionDetector detector in dependentFingerDetectors) {
                    detector.gameObject.SetActive(false);
                }
            }
        }
    }

    public void PerformPalmGrabOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            Hand interactingHand = Hands.Right;
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            Ray palmRay = new Ray(interactingHand.PalmPosition, interactingHand.PalmarAxis());
            ModelInfo grabbedModel = GetModelByRaycast(palmRay);

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
