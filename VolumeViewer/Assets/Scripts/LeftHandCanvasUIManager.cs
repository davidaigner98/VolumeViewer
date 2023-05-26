using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeftHandCanvasUIManager : MonoBehaviour {
    private GameObject wrist;
    public GameObject instructionPanel;
    private bool instructionsVisible = false;
    private bool clippingBoxEnabled = false;
    public Image iButton;
    public Image cButton;
    public Image tButton;

    void Start() {
        wrist = transform.parent.gameObject;
    }

    void Update() {
        Vector3 direction = Vector3.Normalize(Camera.main.transform.position - wrist.transform.position);
        Vector3 wristUp = Vector3.Normalize(wrist.transform.TransformDirection(Vector3.up));

        UpdateButtonColors();
        if(Vector3.Dot(wristUp, direction) > 0.5f) {
            SetChildrenActive(true);
        } else {
            instructionsVisible = false;
            SetChildrenActive(false);
        }
    }

    private void SetChildrenActive(bool active) {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(active);
        }

        instructionPanel.SetActive(instructionsVisible);
    }

    public void ToggleInstructionPanel() {
        instructionsVisible = !instructionsVisible;
        instructionPanel.SetActive(instructionsVisible);
    }

    // toggle the clipping box via checkbox
    public void ToggleClippingBox() {
        clippingBoxEnabled = !clippingBoxEnabled;
        ClippingBox.Instance.SetActive(clippingBoxEnabled);
    }

    private void UpdateButtonColors() {
        if (instructionsVisible) {
            iButton.color = new Color(100f/255, 100f/255, 200f/255, 1);
        } else {
            iButton.color = new Color(150f/255, 150f/255, 150f/255, 1);
        }

        if (clippingBoxEnabled) {
            cButton.color = new Color(100f/255, 100f/255, 200f/255, 1);
        } else {
            cButton.color = new Color(150f/255, 150f/255, 150f/255, 1);
        }

        if (CrossPlatformMediator.Instance.clientMode.Equals("VR")) {
            tButton.color = new Color(100f/255, 100f/255, 200f/255, 1);
        } else if (CrossPlatformMediator.Instance.clientMode.Equals("AR")) {
            tButton.color = new Color(150f/255, 150f/255, 150f/255, 1);
        }
    }
}
