using UnityEngine;
using UnityEngine.InputSystem;

public class Draggable : MonoBehaviour {
    public GameObject displayCamera;
    public float rotSpeed = 0.25f;
    private InputAction mouseMoveAction;
    private InputAction mouseDragAction;
    private InputAction touchMoveAction;
    private InputAction touchDragAction;

    private void Start() {
        PlayerInput playerInput = displayCamera.GetComponent<PlayerInput>();
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

    private void MouseDragStarted(InputAction.CallbackContext c) {
        mouseMoveAction.performed += MouseMovePerformed;
    }

    private void MouseDragCanceled(InputAction.CallbackContext c) {
        mouseMoveAction.performed -= MouseMovePerformed;
    }

    private void TouchDragStarted(InputAction.CallbackContext c) {
        touchMoveAction.performed += TouchMovePerformed;
        mouseDragAction.started -= MouseDragStarted;
    }

    private void TouchDragCanceled(InputAction.CallbackContext c) {
        touchMoveAction.performed -= TouchMovePerformed;
        mouseDragAction.started += MouseDragStarted;
    }

    private void MouseMovePerformed(InputAction.CallbackContext c) {
        Vector2 rotation = Mouse.current.delta.ReadValue() * rotSpeed;
        
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.up), -rotation.x, Space.World);
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.right), rotation.y, Space.World);
    }

    private void TouchMovePerformed(InputAction.CallbackContext c) {
        int touchCount = Touchscreen.current.touches.Count;

        if (touchCount == 1) {
            OneFingerGesture();
        } else if (touchCount == 2) {
            TwoFingerGesture();
        }
    }

    private void OneFingerGesture() {
        Vector2 rotation = Touchscreen.current.delta.ReadValue() * rotSpeed;

        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.up), -rotation.x, Space.World);
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.right), rotation.y, Space.World);
    }

    private void TwoFingerGesture() {
    
    }
}
