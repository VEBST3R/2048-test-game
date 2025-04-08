using UnityEngine;

/// <summary>
/// Відповідає за керування поточним кубом та створення нових
/// </summary>
[RequireComponent(typeof(CubeInputHandler))]
public class CubeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CubeMovementSettings movementSettings;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ICubeFactory cubeFactory;

    private CubeInputHandler inputHandler;
    private GameObject currentCube;
    private Rigidbody currentCubeRigidbody;
    private bool isInitialized = false;

    private void Awake()
    {
        inputHandler = GetComponent<CubeInputHandler>();
    }

    private void Start()
    {
        Initialize();

        // Підписуємося на події вводу
        if (inputHandler != null)
        {
            inputHandler.OnLaunchCube += LaunchCubeForward;
        }

        // Створюємо перший куб
        SpawnNewCube();
    }

    private void OnDestroy()
    {
        // Відписуємося від подій
        if (inputHandler != null)
        {
            inputHandler.OnLaunchCube -= LaunchCubeForward;
        }
    }

    private void Initialize()
    {
        if (isInitialized) return;

        // Отримуємо фабрику кубів
        if (cubeFactory == null)
        {
            var factoryComponent = GetComponent<CubeFactory>();
            if (factoryComponent == null)
            {
                factoryComponent = gameObject.AddComponent<CubeFactory>();
            }
            cubeFactory = factoryComponent;
        }

        isInitialized = true;
    }

    public void LaunchCubeForward()
    {
        if (currentCubeRigidbody == null) return;

        // Додаємо силу для запуску вперед
        currentCubeRigidbody.AddForce(Vector3.forward * movementSettings.ForwardForce, ForceMode.Impulse);

        // Викликаємо створення нового куба з затримкою
        Invoke(nameof(SpawnNewCube), movementSettings.SpawnDelay);

        // Скидаємо посилання
        currentCube = null;
        currentCubeRigidbody = null;
    }

    public void SpawnNewCube()
    {
        // Перевіряємо, чи немає активного куба
        if (currentCube != null) return;

        // Переконуємося, що фабрика ініціалізована
        if (!isInitialized)
        {
            Initialize();
        }

        if (cubeFactory == null)
        {
            Debug.LogError("[CubeController] Cube factory is null!");
            return;
        }

        // Отримуємо контейнер для кубів
        Transform cubesContainer = GameManager.Instance?.GetCubesContainer();

        // Створюємо новий куб
        currentCube = cubeFactory.GetCube(spawnPoint.position, Quaternion.identity, cubesContainer);

        if (currentCube != null)
        {
            currentCubeRigidbody = currentCube.GetComponent<Rigidbody>();
            cubeFactory.SetRandomValue(currentCube, movementSettings.Probability2);

            // Повідомляємо обробник вводу про новий куб
            inputHandler?.SetCurrentCube(currentCube);
        }
        else
        {
            Debug.LogError("[CubeController] Failed to create a new cube!");
        }
    }
}