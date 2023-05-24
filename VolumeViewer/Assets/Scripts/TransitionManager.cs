using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour {
    public static TransitionManager Instance { get; private set; }
    public GameObject vrEnvironment;
    public bool transitionTrigger;
    private Coroutine transitionCoroutine;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    void Update() {
        if (transitionTrigger) {
            transitionTrigger = false;

            StartTransition();
        }
    }

    private void StartTransition() {
        if (transitionCoroutine == null) {
            if (CrossPlatformMediator.Instance.clientMode.Equals("VR")) {
                transitionCoroutine = StartCoroutine(TransitionToAR());
            } else if (CrossPlatformMediator.Instance.clientMode.Equals("AR")) {
                transitionCoroutine = StartCoroutine(TransitionToVR());
            }
        }
    }

    public IEnumerator TransitionToAR() {
        if (CrossPlatformMediator.Instance.isServer) { yield break; }

        Camera cam = GetComponent<Camera>();
        Color currCamColor = cam.backgroundColor;
        float currAlpha = 1;

        while (currAlpha > 0) {
            currAlpha -= Time.deltaTime;

            cam.backgroundColor = new Color(currCamColor.r, currCamColor.g, currCamColor.b, currAlpha);

            foreach (Transform child in vrEnvironment.transform) {
                Color currColor = child.GetComponent<Renderer>().material.color;
                child.GetComponent<Renderer>().material.color = new Color(currColor.r, currColor.g, currColor.b, currAlpha);
            }

            yield return null;
        }

        CrossPlatformMediator.Instance.clientMode = "AR";
        cam.backgroundColor = new Color(currCamColor.r, currCamColor.g, currCamColor.b, 0);

        foreach (Transform child in vrEnvironment.transform) {
            Color currColor = child.GetComponent<Renderer>().material.color;
            child.GetComponent<Renderer>().material.color = new Color(currColor.r, currColor.g, currColor.b, 0);
        }

        transitionCoroutine = null;
    }

    public IEnumerator TransitionToVR() {
        if (CrossPlatformMediator.Instance.isServer) { yield break; }

        Camera cam = GetComponent<Camera>();
        Color currCamColor = cam.backgroundColor;
        float currAlpha = 0;

        while (currAlpha < 1) {
            currAlpha += Time.deltaTime;

            cam.backgroundColor = new Color(currCamColor.r, currCamColor.g, currCamColor.b, currAlpha);

            foreach (Transform child in vrEnvironment.transform) {
                Color currColor = child.GetComponent<Renderer>().material.color;
                child.GetComponent<Renderer>().material.color = new Color(currColor.r, currColor.g, currColor.b, currAlpha);
            }

            yield return null;
        }

        CrossPlatformMediator.Instance.clientMode = "VR";
        cam.backgroundColor = new Color(currCamColor.r, currCamColor.g, currCamColor.b, 1);

        foreach (Transform child in vrEnvironment.transform) {
            Color currColor = child.GetComponent<Renderer>().material.color;
            child.GetComponent<Renderer>().material.color = new Color(currColor.r, currColor.g, currColor.b, 1);
        }

        transitionCoroutine = null;
    }
}
