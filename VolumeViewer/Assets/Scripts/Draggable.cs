using UnityEngine;
using UnityEngine.InputSystem;

public class Draggable : MonoBehaviour {
    public GameObject displayCamera;
    public float rotSpeed = 0.3f;
    private Mouse mouse;
    private InputAction moveAction;
    private InputAction dragAction;
    private Vector2 startOffset;

    void Start() {
        mouse = Mouse.current;

        PlayerInput playerInput = displayCamera.GetComponent<PlayerInput>();
        InputActionMap map = playerInput.currentActionMap;
        moveAction = map.FindAction("Move");
        dragAction = map.FindAction("Drag");
        moveAction.Enable();
        dragAction.Enable();

        dragAction.started += DragStarted;
        dragAction.canceled += DragCanceled;
    }

    void DragStarted(InputAction.CallbackContext c) {
        moveAction.started += MoveStarted;
        moveAction.performed += MovePerformed;
    }

    void DragCanceled(InputAction.CallbackContext c) {
        moveAction.started -= MoveStarted;
        moveAction.performed -= MovePerformed;
    }

    void MoveStarted(InputAction.CallbackContext c) {
        startOffset = mouse.delta.ReadValue();
    }

    void MovePerformed(InputAction.CallbackContext c) {
        Vector2 rotation = (mouse.delta.ReadValue() - startOffset) * rotSpeed;

        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.up), -rotation.x, Space.World);
        transform.Rotate(displayCamera.transform.TransformDirection(Vector3.right), rotation.y, Space.World);
    }
}
