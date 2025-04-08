using UnityEngine;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CubeMovementSettings movementSettings;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CubeFactory cubeFactory;

    [Header("Physics Settings")]
    [SerializeField] private float cubeDrag = 0.5f;
    [SerializeField] private float cubeAngularDrag = 0.2f;
    [SerializeField] private float cubeMass = 1.0f;

    private Vector2 touchStartPosition;
    private bool isDragging = false;
    private GameObject currentCube;
    private Vector3 currentCubeStartPosition;
    private Rigidbody currentCubeRigidbody;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Initialize cube factory if not assigned
        if (cubeFactory == null)
        {
            cubeFactory = GetComponent<CubeFactory>();
            if (cubeFactory == null)
            {
                cubeFactory = gameObject.AddComponent<CubeFactory>();
            }
        }

        SpawnNewCube();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Перевіряємо, чи існує куб для керування
        if (currentCube == null || currentCubeRigidbody == null)
            return;

        // Перевірка на дотик або клік мишею
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

                currentCube.transform.position = newPosition;
            }
        }
        else if (isDragging)
        {
            // Закінчення перетягування - запуск куба вперед
            LaunchCubeForward();
            isDragging = false;
        }

        // Альтернативний контроль для ПК - клавіші стрілок ліворуч/праворуч
        float horizontalInput = Input.GetAxis("Horizontal");
        if (!isDragging && Mathf.Abs(horizontalInput) > 0.1f && currentCubeRigidbody.linearVelocity.magnitude < 0.1f)
        {
            Vector3 newPosition = currentCube.transform.position + Vector3.right * horizontalInput * movementSettings.HorizontalMoveSpeed * Time.deltaTime;
            newPosition.x = Mathf.Clamp(newPosition.x, -movementSettings.MovementRange, movementSettings.MovementRange);
            currentCube.transform.position = newPosition;
        }

        // Запуск куба вперед при натисканні Space
        if (Input.GetKeyDown(KeyCode.Space) && !isDragging && currentCubeRigidbody.linearVelocity.magnitude < 0.1f)
        {
            LaunchCubeForward();
        }
    }

    private void LaunchCubeForward()
    {
        if (currentCubeRigidbody != null)
        {
            currentCubeRigidbody.AddForce(Vector3.forward * movementSettings.ForwardForce, ForceMode.Impulse);

            // Створення нового куба через деякий час
            Invoke("SpawnNewCube", movementSettings.SpawnDelay);

            // Прибираємо посилання на запущений куб
            currentCube = null;
            currentCubeRigidbody = null;
        }
    }

    private void SpawnNewCube()
    {
        // Переконуємося, що немає активного куба для керування
        if (currentCube == null)
        {
            // Використовуємо фабрику для отримання куба з пула
            currentCube = cubeFactory.GetCube(spawnPoint.position, Quaternion.identity);

            if (currentCube != null)
            {
                currentCubeRigidbody = currentCube.GetComponent<Rigidbody>();

                // Встановлюємо початкове значення (2 або 4)
                cubeFactory.SetRandomValue(currentCube, movementSettings.Probability2);
            }
        }
    }
}