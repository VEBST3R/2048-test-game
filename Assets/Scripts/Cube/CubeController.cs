using UnityEngine;

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

        if (inputHandler != null)
        {
            inputHandler.OnLaunchCube += LaunchCubeForward;
        }

        SpawnNewCube();
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnLaunchCube -= LaunchCubeForward;
        }
    }

    private void Initialize()
    {
        if (isInitialized) return;

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

    public GameObject GetCurrentCube()
    {
        return currentCube;
    }

    public void LaunchCubeForward()
    {
        if (currentCubeRigidbody == null) return;

        if (currentCube != null)
        {
            currentCube.tag = "Cube";
        }

        currentCubeRigidbody.AddForce(Vector3.forward * movementSettings.ForwardForce, ForceMode.Impulse);

        Invoke(nameof(SpawnNewCube), movementSettings.SpawnDelay);

        currentCube = null;
        currentCubeRigidbody = null;
    }

    public void SpawnNewCube()
    {
        if (currentCube != null)
        {
            return;
        }

        if (!isInitialized)
        {
            Initialize();
        }

        if (cubeFactory == null)
        {
            cubeFactory = FindFirstObjectByType<CubeFactory>();

            if (cubeFactory == null)
            {
                cubeFactory = gameObject.AddComponent<CubeFactory>();
            }
        }

        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnObj.transform.parent = transform;
            spawnObj.transform.localPosition = new Vector3(0, 5, 0);
            spawnPoint = spawnObj.transform;
        }

        Transform cubesContainer = GameStateManager.Instance?.GetCubesContainer();

        currentCube = cubeFactory.GetCube(spawnPoint.position, Quaternion.identity, cubesContainer);

        if (currentCube != null)
        {
            if (GameStateManager.Instance != null)
            {
                if (GameStateManager.Instance.CheckForTooManyCubes())
                {
                    return;
                }
            }

            currentCube.tag = "CurrentCube";

            if (cubesContainer != null && currentCube.transform.parent != cubesContainer)
            {
                currentCube.transform.SetParent(cubesContainer);
            }

            currentCubeRigidbody = currentCube.GetComponent<Rigidbody>();
            cubeFactory.SetRandomValue(currentCube, movementSettings.Probability2);

            if (inputHandler != null)
            {
                inputHandler.SetCurrentCube(currentCube);
            }
            else
            {
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.CheckForTooManyCubes();
            }
        }
        else
        {
        }
    }
}