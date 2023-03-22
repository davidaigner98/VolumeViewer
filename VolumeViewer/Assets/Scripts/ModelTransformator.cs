using Leap;
using Leap.Unity;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModelTransformator : NetworkBehaviour {
    public Shader transparentShader;
    public float palmGrabDistance = 1.0f;
    public float oneFingerRotationDistance = 1.0f;
    public float releaseDistanceThreshold = 1.0f;
    public float resetSpeed = 1.0f;
    private GameObject displayCenter;
    private Transform displaySize;
    private bool inDisplay = true;
    private Hand interactingHand;
    private bool isBeingGrabbed = false;
    private bool isBeingRotated = false;
    private Vector3 lastPalmPosition;
    private Vector3 lastIndexPosition;

    private void Start() {
        ModelManager.Instance.attached.OnValueChanged += ChangeModelAttachment;

        Material[] mats = GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            mat.shader = transparentShader;
        }

        GetComponent<MeshRenderer>().enabled = true;

        if (CrossPlatformMediator.Instance.isServer) { SetupServer(); }
        else { SetupClient(); }
    }

    public void SetupServer() {
        gameObject.AddComponent<Draggable>();
        SetAlpha(1);
    }

    public void SetupClient() {
        displayCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
        displaySize = DisplayProfileManager.Instance.GetCurrentDisplaySize().transform;
        
        Rescale();
        palmGrabDistance = transform.localScale.x * 2;
        oneFingerRotationDistance = transform.localScale.x * 3 / 2;
        releaseDistanceThreshold = transform.localScale.x * 4 / 5;

        transform.SetParent(displayCenter.transform);
        transform.localPosition = Vector3.zero;

        SetAlpha(0);
    }

    private void Update() {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (isBeingGrabbed) { PalmGrabMovement(); }
            else if (isBeingRotated) { OneFingerRotation(); }

            if (inDisplay) {
                transform.localPosition = Vector3.zero;
            }
        }
    }

    private void PalmGrabMovement() {
        Vector3 delta = interactingHand.PalmPosition - lastPalmPosition;
        lastPalmPosition = interactingHand.PalmPosition;

        transform.position += delta;

        float distance = Vector3.Distance(transform.position, displayCenter.transform.position);
        if (distance < releaseDistanceThreshold) {
            SetAlpha(distance / releaseDistanceThreshold);
        } else {
            SetAlpha(1);
        }
    }

    private void OneFingerRotation() {
        float distance = Vector3.Distance(transform.position, interactingHand.GetIndex().TipPosition);

        if (distance <= oneFingerRotationDistance) {
            Vector3 indexPosition = interactingHand.GetIndex().TipPosition - transform.position;

            Vector3 axis = Vector3.Cross(indexPosition, lastIndexPosition);
            float angle = -Vector3.Angle(indexPosition, lastIndexPosition);

            RotateModelServerRpc(axis, angle);

            lastIndexPosition = indexPosition;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RotateModelServerRpc(Vector3 axis, float angle) {
        transform.Rotate(axis, angle, Space.World);
    }

    public void PalmGrabModelOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            float distance = Vector3.Distance(transform.position, interactingHand.PalmPosition);
            if (distance <= palmGrabDistance) {
                Rescale();

                inDisplay = false;
                isBeingGrabbed = true;
                lastPalmPosition = interactingHand.PalmPosition;
            }
        }
    }

    public void PalmGrabModelOff() {
        if (!CrossPlatformMediator.Instance.isServer) { 
            float distance = Vector3.Distance(transform.position, displayCenter.transform.position);
            isBeingGrabbed = false;

            if (distance >= releaseDistanceThreshold) {
                CrossPlatformMediator.Instance.ChangeAttachmentButtonInteractabilityServerRpc(true);
            } else {
                StartCoroutine(MoveToOrigin());
            }
        }
    }

    private IEnumerator MoveToOrigin() {
        Vector3 destination = displayCenter.transform.position;
        float distanceToOrigin;

        do {
            Vector3 delta = transform.position - destination;
            transform.position -= delta.normalized * Time.deltaTime * resetSpeed;
            distanceToOrigin = Vector3.Distance(Vector3.zero, delta);
            SetAlpha(distanceToOrigin / releaseDistanceThreshold);

            yield return null;
        } while (distanceToOrigin > 0.01);

        SetAlpha(0);
        inDisplay = true;
        ModelManager.Instance.SetAttachedStateServerRpc(true);
        ChangeModelAttachment(true, true);
        CrossPlatformMediator.Instance.ChangeAttachmentButtonInteractabilityServerRpc(false);
        transform.position = destination;
    }

    public void ChangeModelAttachment(bool prev, bool current) {
        if (!CrossPlatformMediator.Instance.isServer) {
            GameObject displayProjection = GameObject.Find("DisplayProjection");
            if (displayProjection != null) {
                GameObject displayCenter = displayProjection.GetComponent<DisplayProfileManager>().GetCurrentDisplayCenter();

                if (current) {
                    GameObject modelParent = GameObject.Find("ModelParent");
                    transform.SetParent(displayCenter.transform);
                    Destroy(modelParent);
                } else {
                    GameObject modelParent = new GameObject("ModelParent");
                    modelParent.transform.rotation = displayCenter.transform.rotation;
                    transform.SetParent(modelParent.transform);
                }
            }
        }
    }

    public void SetAlpha(float alpha) {
        Material[] mats = GetComponent<Renderer>().materials;
        foreach (Material mat in mats) {
            Color newColor = mat.color;
            newColor.a = alpha;
            mat.color = newColor;
        }
    }

    private void Rescale() {
        transform.localScale = Vector3.one * displaySize.localScale.y / 3;
    }
    
    public void OneFingerRotationOn(string hand) {
        if (!CrossPlatformMediator.Instance.isServer) {
            if (hand.Equals("left")) { interactingHand = Hands.Left; }
            else if (hand.Equals("right")) { interactingHand = Hands.Right; }

            lastIndexPosition = interactingHand.GetIndex().TipPosition - transform.position;
            isBeingRotated = true;
        }
    }

    public void OneFingerRotationOff() {
        if (!CrossPlatformMediator.Instance.isServer) {
            isBeingRotated = false;
        }
    }

    public void AlignCoronal() {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void AlignSagittal() {
        transform.rotation = Quaternion.Euler(0, 270, 0);
    }

    public void AlignAxial() {
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
