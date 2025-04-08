using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CubeInputHandler : MonoBehaviour
{
    [SerializeField] private CubeMovementSettings movementSettings;

    public event Action<Vector3> OnMoveCube;
    public event Action OnLaunchCube;

    private Vector2 touchStartPosition;
    private bool isDragging = false;
    private Vector3 currentCubeStartPosition;
    private GameObject currentCube;
    private Rigidbody currentCubeRigidbody;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetCurrentCube(GameObject cube)
    {
        currentCube = cube;
        if (currentCube != null)
        {
            currentCubeRigidbody = currentCube.GetComponent<Rigidbody>();
            currentCubeStartPosition = currentCube.transform.position;
        }
    }

    private void Update()
    {
        if (currentCube == null || currentCubeRigidbody == null)
            return;

        HandleTouchInput();

        HandleKeyboardInput();
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

            if (!isDragging)
            {
                touchStartPosition = touchPosition;
                currentCubeStartPosition = currentCube.transform.position;
                isDragging = true;
            }
            else
            {
                float horizontalDelta = (touchPosition.x - touchStartPosition.x) / Screen.width * movementSettings.HorizontalMoveSpeed;
                Vector3 newPosition = currentCubeStartPosition + Vector3.right * horizontalDelta;

                newPosition.x = Mathf.Clamp(newPosition.x, -movementSettings.MovementRange, movementSettings.MovementRange);

                newPosition.y = currentCube.transform.position.y;
                newPosition.z = currentCube.transform.position.z;

                currentCube.transform.position = newPosition;

                OnMoveCube?.Invoke(Vector3.zero);
            }
        }
        else if (isDragging)
        {
            OnLaunchCube?.Invoke();
            isDragging = false;
        }
    }

    private void HandleKeyboardInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (!isDragging && Mathf.Abs(horizontalInput) > 0.1f &&
            currentCubeRigidbody.linearVelocity.magnitude < 0.1f)
        {
            Vector3 moveDirection = Vector3.right * horizontalInput *
                movementSettings.HorizontalMoveSpeed * Time.deltaTime;

            Vector3 newPosition = currentCube.transform.position + moveDirection;

            newPosition.x = Mathf.Clamp(newPosition.x, -movementSettings.MovementRange, movementSettings.MovementRange);

            currentCube.transform.position = newPosition;

            OnMoveCube?.Invoke(Vector3.zero);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDragging &&
            currentCubeRigidbody.linearVelocity.magnitude < 0.1f)
        {
            OnLaunchCube?.Invoke();
        }
    }
}
