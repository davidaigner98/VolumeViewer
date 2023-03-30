using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class DisplayInputManager : MonoBehaviour {
    public static DisplayInputManager Instance { get; private set; }
    public float ofRotSpeed = 0.25f;
    public float mfRotSpeed = 0.7f;
    public float moveSpeed = 0.006f;
    private InputAction mouseMoveAction;
    private InputAction mouseDragAction;
    private InputAction touchMoveAction;
    private InputAction touchDragAction;
    private Vector2 oldRotatingFingerDifference = Vector2.zero;
    private float initialScaleDistance = -1;
    private Vector3 initialScale = Vector3.one;
    private bool first1FingerCall, first2FingerCall, first3To5FingerCall;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); } 
        else { Instance = this; }
    }

    private void Start() {
        PlayerInput playerInput = GameObject.Find("DisplayCamera(Clone)").GetComponent<PlayerInput>();
        InputActionMap map = playerInput.currentActionMap;
        mouseMoveAction = map.FindAction("MouseMove");
        mouseMoveAction.Enable();
        mouseDragAction = map.FindAction("MouseDrag");
        mouseDragAction.Enable();
        touchMoveAction = map.FindAction("TouchMove");
        touchMoveAction.Enable();
        touchDragAction = map.FindAction("TouchDrag");
        touchDragAction.Enable();

        mouseDragAction.started += MouseDragStarted;
        mouseDragAction.canceled += MouseDragCanceled;
        touchDragAction.started += TouchDragStarted;
        touchDragAction.canceled += TouchDragCanceled;
    }

    private void OnDestroy() {
        mouseDragAction.started -= MouseDragStarted;
        mouseDragAction.canceled -= MouseDragCanceled;
        mouseMoveAction.performed -= MouseMovePerformed;
        touchDragAction.started -= TouchDragStarted;
        touchDragAction.canceled -= TouchDragCanceled;
        touchMoveAction.performed -= TouchMovePerformed;
    }

    private void MouseDragStarted(InputAction.CallbackContext c) {
        TrySelectModel(Mouse.current.position.ReadValue());

        mouseMoveAction.performed += MouseMovePerformed;
    }

    private void MouseDragCanceled(InputAction.CallbackContext c) {
        mouseMoveAction.performed -= MouseMovePerformed;
    }

    private void TouchDragStarted(InputAction.CallbackContext c) {
        initialScaleDistance = -1;
        oldRotatingFingerDifference = Vector2.zero;
        first1FingerCall = true;
        first2FingerCall = true;
        first3To5FingerCall = true;

        touchMoveAction.performed += TouchMovePerformed;
        mouseDragAction.started -= MouseDragStarted;
    }

    private void TouchDragCanceled(InputAction.CallbackContext c) {
        touchMoveAction.performed -= TouchMovePerformed;
        mouseDragAction.started += MouseDragStarted;
    }

    private void MouseMovePerformed(InputAction.CallbackContext c) {
        GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }
        
        Vector2 rotation = Mouse.current.delta.ReadValue() * ofRotSpeed;
        
        selectedModel.transform.Rotate(Vector3.up, -rotation.x, Space.World);
        selectedModel.transform.Rotate(Vector3.right, -rotation.y, Space.World);
    }

    private void TouchMovePerformed(InputAction.CallbackContext c) {
        int touchCount = GetNumbersOfTouches();

        if (touchCount == 1) {
            if (first1FingerCall) {
                TrySelectModel(GetPalmPosition());

                first1FingerCall = false;
            }

            OneFingerGesture();
        }

        if (touchCount == 2) {
            if (first2FingerCall) {
                TrySelectModel(GetPalmPosition());
                first2FingerCall = false;
            }

            MultipleFingerRotating(touchCount);
            MultipleFingerScaling(touchCount);
        }

        if (touchCount >= 3 && touchCount <= 5) {
            if (first3To5FingerCall) {
                TrySelectModel(GetPalmPosition());
                first3To5FingerCall = false;
            }

            MultipleFingerPositioning(touchCount);
        }
    }

    private void OneFingerGesture() {
        GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }
        
        Vector2 rotation = Touchscreen.current.delta.ReadValue() * ofRotSpeed;

        selectedModel.transform.Rotate(Vector3.up, -rotation.x, Space.World);
        selectedModel.transform.Rotate(Vector3.right, -rotation.y, Space.World);
    }

    private void MultipleFingerPositioning(int touchCount) {
        GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }

        Vector2 palmPosition = GetPalmPosition();
        if (palmPosition.Equals(new Vector2(-1, -1))) { return; }

        Camera displayCamera = DisplayLocalizer.Instance.displayCamera;
        Vector3 palmPosition3D = new Vector3(palmPosition.x, palmPosition.y, displayCamera.transform.position.z);
        selectedModel.transform.position = displayCamera.ScreenToWorldPoint(palmPosition3D);

        Vector2 screenOffset = DisplayLocalizer.Instance.GetRelativeScreenOffset(selectedModel);
        selectedModel.GetComponent<ModelTransformator>().screenOffset.Value = screenOffset;
    }

    private void MultipleFingerRotating(int touchCount) {
        GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }
        if (touchCount != 2) { return; }
        
        TouchState touch0 = Touchscreen.current.touches[0].ReadValue();
        TouchState touch1 = Touchscreen.current.touches[1].ReadValue();
        Vector2 newPosition0 = touch0.position;
        Vector2 newPosition1 = touch1.position;
        Vector2 newVector = newPosition0 - newPosition1;

        if (!oldRotatingFingerDifference.Equals(Vector2.zero)) {
            float angle = Vector2.SignedAngle(newVector, oldRotatingFingerDifference) * mfRotSpeed;
            selectedModel.transform.Rotate(Vector3.forward, angle, Space.World);
        }

        oldRotatingFingerDifference = newVector;
    }

    private void MultipleFingerScaling(int touchCount) {
        GameObject selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }
        if (touchCount != 2) { return; }
        
        TouchControl touch0 = Touchscreen.current.touches[0];
        TouchControl touch1 = Touchscreen.current.touches[1];

        Vector2 newPosition0 = touch0.position.ReadValue();
        Vector2 newPosition1 = touch1.position.ReadValue();
        float newDistance = Vector2.Distance(newPosition0, newPosition1);

        if (initialScaleDistance < 0) {
            Vector2 oldPosition0 = newPosition0 - touch0.delta.ReadValue();
            Vector2 oldPosition1 = newPosition1 - touch1.delta.ReadValue();
            initialScale = selectedModel.transform.localScale;
            initialScaleDistance = Vector2.Distance(oldPosition0, oldPosition1);
        }

        selectedModel.transform.localScale = initialScale * newDistance / initialScaleDistance;
        selectedModel.GetComponent<ModelTransformator>().scaleOnDisplay.Value = selectedModel.transform.localScale.x;
    }

    private int GetNumbersOfTouches() {
        int touchCount = 0;
        
        for (int i = 0; i < Touchscreen.current.touches.Count; i++) {
            TouchPhase currPhase = Touchscreen.current.touches[i].phase.ReadValue();

            if (currPhase != 0 && currPhase != TouchPhase.Canceled && currPhase != TouchPhase.Ended) {
                touchCount++;
            }
        }

        return touchCount;
    }

    private Vector2 GetPalmPosition() {
        Vector2 palmPosition = Vector3.zero;
        int touchCount = GetNumbersOfTouches();
        if (touchCount <= 0) { return new Vector2(-1, -1); }

        for (int i = 0; i < touchCount; i++) {
            TouchControl currTouch = Touchscreen.current.touches[i];
            palmPosition += currTouch.position.ReadValue();
        }

        palmPosition /= touchCount;
        return palmPosition;
    }

    private void TrySelectModel(Vector2 screenCoordinates) {
        if (screenCoordinates.Equals(new Vector2(-1, -1))) { return; }
        
        ModelInfo hitModel = DisplayLocalizer.Instance.FindModelByRaycast(screenCoordinates);
        if (hitModel != null) {
            ModelManager.Instance.SetSelectedModel(hitModel);
        } else {
            ModelManager.Instance.SetSelectedModel(null);
        }
    }
}