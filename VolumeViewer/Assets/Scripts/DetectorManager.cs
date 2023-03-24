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
            GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
            if (selectedModel != null) {
                selectedModel.GetComponent<ModelTransformator>().PalmGrabModelOn(hand);
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
            GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
            if (selectedModel != null) {
                selectedModel.GetComponent<ModelTransformator>().OneFingerRotationOn(hand);
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
}
