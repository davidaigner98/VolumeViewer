using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class Draggable : MonoBehaviour {
    public GameObject displayCamera;
    public float rotSpeed = 0.25f;
    private InputAction mouseMoveAction;
    private InputAction mouseDragAction;
    private InputAction touchMoveAction;
    private InputAction touchDragAction;

    void Start() {
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

    void MouseDragStarted(InputAction.CallbackContext c) {
        mouseMoveAction.performed += MouseMovePerformed;
        Debug.Log("Mouse Drag started!");
    }

    void MouseDragCanceled(InputAction.CallbackContext c) {
        mouseMoveAction.performed -= MouseMovePerformed;
    }

    void TouchDragStarted(InputAction.CallbackContext c) {
        touchMoveAction.performed += TouchMovePerformed;
        mouseDragAction.started -= MouseDragStarted;
        Debug.Log("Touch Drag started!");
    }

    void TouchDragCanceled(InputAction.CallbackContext c) {
        touchMoveAction.performed -= TouchMovePerformed;
        mouseDragAction.started += MouseDragStarted;
    }

    void MouseMovePerformed(InputAction.CallbackContext c) {
        Vector2 rotation = Mouse.current.delta.ReadValue() * rotSpeed;
        
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.up), -rotation.x, Space.World);
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.right), rotation.y, Space.World);
    }

    void TouchMovePerformed(InputAction.CallbackContext c) {
        Vector2 rotation = Touchscreen.current.delta.ReadValue() * rotSpeed;
        
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.up), -rotation.x, Space.World);
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.right), rotation.y, Space.World);
    }
}
