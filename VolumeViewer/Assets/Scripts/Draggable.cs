using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class Draggable : MonoBehaviour {
    public GameObject displayCamera;
    public float rotSpeed = 0.25f;
    public float moveSpeed = 0.01f;
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
        int touchCount = GetNumbersOfTouches();

        if (touchCount == 1) {
            OneFingerGesture();
        } else if (touchCount == 5) {
            TwoFingerGesture();
        }
    }

    private void OneFingerGesture() {
        Vector2 rotation = Touchscreen.current.delta.ReadValue() * rotSpeed;

        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.up), -rotation.x, Space.World);
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.right), rotation.y, Space.World);
    }

    private void TwoFingerGesture() {
        TouchControl touch0 = Touchscreen.current.touches[0];
        TouchControl touch1 = Touchscreen.current.touches[1];
        TouchControl touch2 = Touchscreen.current.touches[1];
        TouchControl touch3 = Touchscreen.current.touches[1];
        TouchControl touch4 = Touchscreen.current.touches[1];

        Vector2 totalDelta = Vector3.zero;
        totalDelta += touch0.delta.ReadValue();
        totalDelta += touch1.delta.ReadValue();
        totalDelta += touch2.delta.ReadValue();
        totalDelta += touch3.delta.ReadValue();
        totalDelta += touch4.delta.ReadValue();
        totalDelta /= 5;
        transform.position += new Vector3(-totalDelta.x, totalDelta.y, 0) * moveSpeed;
    }

    private int GetNumbersOfTouches() {
        int touchCount = 0;

        for (int i = 0; i < Touchscreen.current.touches.Count; i++) {
            TouchState currTouch = Touchscreen.current.touches[i].ReadValue();

            if (currTouch.phase != 0 && currTouch.phase != TouchPhase.Canceled && currTouch.phase != TouchPhase.Ended) {
                touchCount++;
            }
        }

        return touchCount;
    }
}
