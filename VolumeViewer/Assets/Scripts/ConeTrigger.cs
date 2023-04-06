using Leap.Unity;
using System.Collections.Generic;
using UnityEngine;

public class ConeTrigger : MonoBehaviour {
    public HandSide side;
    public enum HandSide { Left, Right};
    private List<ModelInfo> includedModels = new List<ModelInfo>();

    private void Update() {
        if (side == HandSide.Left && Hands.Left != null) {
            transform.position = Hands.Left.PalmPosition;
            transform.LookAt(Hands.Left.PalmPosition + Hands.Left.PalmarAxis());
        } else if (side == HandSide.Right && Hands.Right != null) {
            transform.position = Hands.Right.PalmPosition;
            transform.LookAt(Hands.Right.PalmPosition + Hands.Right.PalmarAxis());
        }
    }

    private void OnTriggerEnter(Collider collider) {
        ModelInfo model = collider.gameObject.GetComponentInParent<ModelInfo>();
        if (model) { includedModels.Add(model); }
    }

    private void OnTriggerExit(Collider collider) {
        ModelInfo model = collider.gameObject.GetComponentInParent<ModelInfo>();
        if (model && includedModels.Contains(model)) { includedModels.Remove(model); }
    }

    public ModelInfo GetSelectedModel() {
        if (includedModels.Count == 0) { return null; }
        RemoveNullEntries();

        ModelInfo closestModel = includedModels[0];
        float smallestDistance = Vector3.Distance(closestModel.transform.position, transform.position);

        foreach (ModelInfo currModel in includedModels) {
            float currDistance = Vector3.Distance(currModel.transform.position, transform.position);

            if (currDistance < smallestDistance) {
                closestModel = currModel;
                smallestDistance = currDistance;
            }
        }

        return closestModel;
    }

    private void RemoveNullEntries() {
        for (int i = includedModels.Count - 1; i >= 0; i--) {
            if (includedModels[i] == null) {
                includedModels.RemoveAt(i);
            }
        }
    }
}
