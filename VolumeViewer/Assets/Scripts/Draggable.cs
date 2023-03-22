using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class Draggable : MonoBehaviour {
    public float rotSpeed = 0.25f;
    public float moveSpeed = 0.006f;
    private InputAction mouseMoveAction;
    private InputAction mouseDragAction;
    private InputAction touchMoveAction;
    private InputAction touchDragAction;

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
        
        transform.Rotate(Vector3.up, -rotation.x, Space.World);
        transform.Rotate(Vector3.right, -rotation.y, Space.World);
    }

    private void TouchMovePerformed(InputAction.CallbackContext c) {
        int touchCount = GetNumbersOfTouches();

        if (touchCount == 1) {
            OneFingerGesture();
        } else if (touchCount >= 2 && touchCount < 6) {
            MultipleFingerGesture(touchCount);
        }
    }

    private void OneFingerGesture() {
        Vector2 rotation = Touchscreen.current.delta.ReadValue() * rotSpeed;

        transform.Rotate(Vector3.up, -rotation.x, Space.World);
        transform.Rotate(Vector3.right, -rotation.y, Space.World);
    }

    private void MultipleFingerGesture(int touchCount) {
        Vector2 totalDelta = Vector3.zero;

        for (int i = 0; i < touchCount; i++) {
            TouchControl currTouch = Touchscreen.current.touches[i];
            totalDelta += currTouch.delta.ReadValue();
        }

        totalDelta /= touchCount;
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
