using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Відповідає за обробку вводу користувача для керування кубами
/// </summary>
public class CubeInputHandler : MonoBehaviour
{
    [SerializeField] private CubeMovementSettings movementSettings;

    // Події для комунікації з CubeController
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
        // Перевіряємо, чи існує куб для керування
        if (currentCube == null || currentCubeRigidbody == null)
            return;

        // Обробка сенсорних дотиків
        HandleTouchInput();

        // Обробка клавіатури
        HandleKeyboardInput();
    }

    private void HandleTouchInput()
    {
        // Перевірка на дотик
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

            if (!isDragging)
            {
                // Початок перетягування
                touchStartPosition = touchPosition;
                currentCubeStartPosition = currentCube.transform.position;
                isDragging = true;
            }
            else
            {
                // Перетягування
                float horizontalDelta = (touchPosition.x - touchStartPosition.x) / Screen.width * movementSettings.HorizontalMoveSpeed;
                Vector3 newPosition = currentCubeStartPosition + Vector3.right * horizontalDelta;

                // Обмеження руху у визначеному діапазоні
                newPosition.x = Mathf.Clamp(newPosition.x, -movementSettings.MovementRange, movementSettings.MovementRange);

                // Зберігаємо Y і Z координати
                newPosition.y = currentCube.transform.position.y;
                newPosition.z = currentCube.transform.position.z;

                // Напряму змінюємо позицію куба замість генерування події
                currentCube.transform.position = newPosition;

                // Повідомляємо про зміну позиції (для сумісності з існуючим кодом)
                OnMoveCube?.Invoke(Vector3.zero); // Тут відправляємо нульовий вектор, бо ми вже перемістили куб
            }
        }
        else if (isDragging)
        {
            // Закінчення перетягування - запуск куба вперед
            OnLaunchCube?.Invoke();
            isDragging = false;
        }
    }

    private void HandleKeyboardInput()
    {
        // Горизонтальне переміщення стрілками
        float horizontalInput = Input.GetAxis("Horizontal");
        if (!isDragging && Mathf.Abs(horizontalInput) > 0.1f &&
            currentCubeRigidbody.linearVelocity.magnitude < 0.1f)
        {
            // Пряме переміщення куба в горизонтальному напрямку
            Vector3 moveDirection = Vector3.right * horizontalInput *
                movementSettings.HorizontalMoveSpeed * Time.deltaTime;

            Vector3 newPosition = currentCube.transform.position + moveDirection;

            // Обмеження руху
            newPosition.x = Mathf.Clamp(newPosition.x, -movementSettings.MovementRange, movementSettings.MovementRange);

            // Встановлюємо нову позицію
            currentCube.transform.position = newPosition;

            // Повідомляємо про рух (для сумісності)
            OnMoveCube?.Invoke(Vector3.zero);
        }

        // Запуск куба пробілом
        if (Input.GetKeyDown(KeyCode.Space) && !isDragging &&
            currentCubeRigidbody.linearVelocity.magnitude < 0.1f)
        {
            OnLaunchCube?.Invoke();
        }
    }
}
