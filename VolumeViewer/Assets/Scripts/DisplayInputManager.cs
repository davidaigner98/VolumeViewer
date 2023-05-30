using UnityEngine;
using UnityEngine.EventSystems;
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
    private InputAction mouseLeftDragAction;
    private InputAction mouseRightDragAction;
    private InputAction mouseScrollAction;
    private InputAction touchMoveAction;
    private InputAction touchDragAction;
    private bool leftMouseButton;
    private Vector2 oldRotatingFingerDifference = Vector2.zero;
    private float initialScaleDistance = -1;
    private Vector3 initialScale = Vector3.one;
    private bool first1FingerCall, first2FingerCall, first3To5FingerCall;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); } 
        else { Instance = this; }
    }

    private void Start() {
        // initialize and enable input actions
        PlayerInput playerInput = GameObject.Find("DisplayCamera(Clone)").GetComponent<PlayerInput>();
        InputActionMap map = playerInput.currentActionMap;
        mouseMoveAction = map.FindAction("MouseMove");
        mouseMoveAction.Enable();
        mouseLeftDragAction = map.FindAction("MouseLeftDrag");
        mouseLeftDragAction.Enable();
        mouseRightDragAction = map.FindAction("MouseRightDrag");
        mouseRightDragAction.Enable();
        mouseScrollAction = map.FindAction("MouseScroll");
        mouseScrollAction.Enable();
        touchMoveAction = map.FindAction("TouchMove");
        touchMoveAction.Enable();
        touchDragAction = map.FindAction("TouchDrag");
        touchDragAction.Enable();

        // bind methods to input actions
        mouseLeftDragAction.started += MouseLeftDragStarted;
        mouseLeftDragAction.canceled += MouseLeftDragCanceled;
        mouseRightDragAction.started += MouseRightDragStarted;
        mouseRightDragAction.canceled += MouseRightDragCanceled;
        mouseScrollAction.performed += MouseScrollPerformed;
        touchDragAction.started += TouchDragStarted;
        touchDragAction.canceled += TouchDragCanceled;
    }

    private void OnDestroy() {
        // remove bindings on destroy
        mouseLeftDragAction.started -= MouseLeftDragStarted;
        mouseLeftDragAction.canceled -= MouseLeftDragCanceled;
        mouseRightDragAction.started -= MouseRightDragStarted;
        mouseRightDragAction.canceled -= MouseRightDragCanceled;
        mouseScrollAction.performed -= MouseScrollPerformed;
        mouseMoveAction.performed -= MouseMovePerformed;
        touchDragAction.started -= TouchDragStarted;
        touchDragAction.canceled -= TouchDragCanceled;
        touchMoveAction.performed -= TouchMovePerformed;
    }

    private void MouseLeftDragStarted(InputAction.CallbackContext c) {
        if (EventSystem.current.IsPointerOverGameObject()) { return; }

        // try to select model to be transformed
        TrySelectModel(Mouse.current.position.ReadValue());

        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().highlighted.Value = true; }

        leftMouseButton = true;
        mouseMoveAction.performed += MouseMovePerformed;
    }

    private void MouseLeftDragCanceled(InputAction.CallbackContext c) {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().highlighted.Value = false; }

        mouseMoveAction.performed -= MouseMovePerformed;
    }

    private void MouseRightDragStarted(InputAction.CallbackContext c) {
        if (EventSystem.current.IsPointerOverGameObject()) { return; }

        // try to select model to be transformed
        TrySelectModel(Mouse.current.position.ReadValue());

        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().highlighted.Value = true; }

        leftMouseButton = false;
        mouseMoveAction.performed += MouseMovePerformed;
    }

    private void MouseRightDragCanceled(InputAction.CallbackContext c) {
        mouseMoveAction.performed -= MouseMovePerformed;

        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().highlighted.Value = false; }
    }

    private void MouseScrollPerformed(InputAction.CallbackContext c) {
        // try to select model to be transformed
        TrySelectModel(Mouse.current.position.ReadValue());

        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }

        float zPos = selectedModel.transform.position.z;
        float oldZBoundary = selectedModel.transform.Find("Model").GetComponent<BoxCollider>().bounds.min.z - zPos;
        float sizeChange = mouseScrollAction.ReadValue<Vector2>().y / 1200;

        // scale model
        float currScale = selectedModel.transform.localScale.x;
        selectedModel.transform.localScale = Vector3.one * currScale * (1.0f + sizeChange);
        selectedModel.GetComponent<ModelTransformator>().scaleOnDisplay.Value = selectedModel.transform.localScale.x;

        // move model back on z axis
        float newZBoundary = oldZBoundary * (1.0f + sizeChange);
        float deltaZBoundary = oldZBoundary - newZBoundary;
        selectedModel.GetComponent<ModelTransformator>().screenOffset.Value += Vector3.forward * deltaZBoundary;
    }

    private void TouchDragStarted(InputAction.CallbackContext c) {
        initialScaleDistance = -1;
        oldRotatingFingerDifference = Vector2.zero;
        first1FingerCall = true;
        first2FingerCall = true;
        first3To5FingerCall = true;

        touchMoveAction.performed += TouchMovePerformed;
        mouseLeftDragAction.started -= MouseLeftDragStarted;
    }

    private void TouchDragCanceled(InputAction.CallbackContext c) {
        touchMoveAction.performed -= TouchMovePerformed;
        mouseLeftDragAction.started += MouseLeftDragStarted;

        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().highlighted.Value = false; }
    }

    private void MouseMovePerformed(InputAction.CallbackContext c) {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }

        if (leftMouseButton) {
            MousePositioning(Mouse.current.position.ReadValue());
        } else {
            Vector2 rotation = Mouse.current.delta.ReadValue() * ofRotSpeed;

            ModelTransformator selectedTransformator = selectedModel.GetComponent<ModelTransformator>();
            selectedTransformator.modelRotation.Value *= Quaternion.Euler(-rotation.x * selectedModel.transform.InverseTransformDirection(Vector3.up));
            selectedTransformator.modelRotation.Value *= Quaternion.Euler(rotation.y * selectedModel.transform.InverseTransformDirection(Vector3.right));
        }
    }

    private void TouchMovePerformed(InputAction.CallbackContext c) {
        int touchCount = GetNumbersOfTouches();

        // different kinds of touch interactions

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

        if (touchCount >= 2 && touchCount <= 5) {
            if (first3To5FingerCall) {
                TrySelectModel(GetPalmPosition());
                first3To5FingerCall = false;
            }

            MultipleFingerPositioning(touchCount);
        }

        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel != null) { selectedModel.GetComponent<ModelTransformator>().highlighted.Value = true; }
    }

    // one finger rotation on touch
    private void OneFingerGesture() {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }
        
        // get angle of rotation
        Vector2 rotation = Touchscreen.current.delta.ReadValue() * ofRotSpeed;

        // rotate model by two axes
        ModelTransformator selectedTransformator = selectedModel.GetComponent<ModelTransformator>();
        selectedTransformator.modelRotation.Value *= Quaternion.Euler(-rotation.x * selectedModel.transform.InverseTransformDirection(Vector3.up));
        selectedTransformator.modelRotation.Value *= Quaternion.Euler(rotation.y * selectedModel.transform.InverseTransformDirection(Vector3.right));
    }

    // multiple finger positioning on touch
    private void MultipleFingerPositioning(int touchCount) {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }

        Vector2 palmPosition = GetPalmPosition();
        if (palmPosition.Equals(new Vector2(-1, -1))) { return; }

        // calculate new screen offset based on camera position and current z position
        Camera displayCamera = DisplayLocalizer.Instance.displayCamera;
        float distance = selectedModel.transform.position.z - displayCamera.transform.position.z;
        Vector3 newOffset = displayCamera.ScreenToWorldPoint(new Vector3(palmPosition.x, palmPosition.y, distance));
        Vector3 viewportSize = DisplayCameraPositioning.Instance.viewportSize;
        newOffset = new Vector3(newOffset.x / viewportSize.x, newOffset.y / viewportSize.x, newOffset.z);
        
        // set screen offset, therefore reposition model
        selectedModel.GetComponent<ModelTransformator>().screenOffset.Value = newOffset;
    }

    // positioning via left mouse button
    private void MousePositioning(Vector2 mouseScreenPosition) {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }

        // calculate new screen offset based on camera position and current z position
        Camera displayCamera = DisplayLocalizer.Instance.displayCamera;
        float distance = selectedModel.transform.position.z - displayCamera.transform.position.z;
        Vector3 newOffset = displayCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, distance));
        Vector3 viewportSize = DisplayCameraPositioning.Instance.viewportSize;
        newOffset = new Vector3(newOffset.x / viewportSize.x, newOffset.y / viewportSize.x, newOffset.z);
        
        // set screen offset, therefore reposition model
        selectedModel.GetComponent<ModelTransformator>().screenOffset.Value = newOffset;
    }

    // mutliple finger rotating on touch
    private void MultipleFingerRotating(int touchCount) {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }
        if (touchCount != 2) { return; }
        
        TouchState touch0 = Touchscreen.current.touches[0].ReadValue();
        TouchState touch1 = Touchscreen.current.touches[1].ReadValue();
        Vector2 newPosition0 = touch0.position;
        Vector2 newPosition1 = touch1.position;
        Vector2 newVector = newPosition0 - newPosition1;

        // if this is not the first rotation
        if (!oldRotatingFingerDifference.Equals(Vector2.zero)) {
            // calculate rotation angle
            float angle = -Vector2.SignedAngle(newVector, oldRotatingFingerDifference) * mfRotSpeed;
            selectedModel.GetComponent<ModelTransformator>().modelRotation.Value *= Quaternion.Euler(angle * Vector3.forward);
        }

        oldRotatingFingerDifference = newVector;
    }

    // multiple finger scaling on touch
    private void MultipleFingerScaling(int touchCount) {
        ModelInfo selectedModel = ModelManager.Instance.GetSelectedModel();
        if (selectedModel == null) { return; }
        if (touchCount != 2) { return; }
        
        TouchControl touch0 = Touchscreen.current.touches[0];
        TouchControl touch1 = Touchscreen.current.touches[1];

        // calculate new distance between touching fingers
        Vector2 newPosition0 = touch0.position.ReadValue();
        Vector2 newPosition1 = touch1.position.ReadValue();
        float newDistance = Vector2.Distance(newPosition0, newPosition1);

        // calculate old distance between touching fingers, if this is not the first touch
        if (initialScaleDistance < 0) {
            Vector2 oldPosition0 = newPosition0 - touch0.delta.ReadValue();
            Vector2 oldPosition1 = newPosition1 - touch1.delta.ReadValue();
            initialScale = selectedModel.transform.localScale;
            initialScaleDistance = Vector2.Distance(oldPosition0, oldPosition1);
        }

        // scale model
        float sizeChange = newDistance / initialScaleDistance;
        selectedModel.transform.localScale = initialScale * sizeChange;
        selectedModel.GetComponent<ModelTransformator>().scaleOnDisplay.Value = selectedModel.transform.localScale.x;
    }

    // get number of fingers currently touching the screen
    private int GetNumbersOfTouches() {
        int touchCount = 0;
        
        for (int i = 0; i < Touchscreen.current.touches.Count; i++) {
            TouchPhase currPhase = Touchscreen.current.touches[i].phase.ReadValue();

            // check if touch is in a valid touch phase
            if (currPhase != 0 && currPhase != TouchPhase.Canceled && currPhase != TouchPhase.Ended) {
                touchCount++;
            }
        }

        return touchCount;
    }

    // get medium screen position of all touching fingers
    private Vector2 GetPalmPosition() {
        Vector2 palmPosition = Vector3.zero;
        int touchCount = GetNumbersOfTouches();
        if (touchCount <= 0) { return new Vector2(-1, -1); }

        // iterate through all touches and sum up all screen positions
        for (int i = 0; i < touchCount; i++) {
            TouchControl currTouch = Touchscreen.current.touches[i];
            palmPosition += currTouch.position.ReadValue();
        }

        // divide by touch count to get average position ("palm position")
        palmPosition /= touchCount;
        return palmPosition;
    }

    // try to select a model, based on touch interaction on screen
    private void TrySelectModel(Vector2 screenCoordinates) {
        if (screenCoordinates.Equals(new Vector2(-1, -1))) { return; }
        
        // try to hit model
        ModelInfo hitModel = DisplayLocalizer.Instance.FindModelByRaycast(screenCoordinates);
        if (hitModel != null) {
            // if hit, then select
            ModelManager.Instance.SetSelectedModel(hitModel);
        }
    }
}
