using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour, IGameStateManager, IScoreManager
{
    private static GameStateManager _instance;
    private static bool _applicationIsQuitting = false;

    public static GameStateManager Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance of GameStateManager already destroyed on application quit. Won't create again.");
                return null;
            }

            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameStateManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<GameStateManager>();
                    singleton.name = "(singleton) GameStateManager";
                }
            }

            return _instance;
        }
    }

    [Header("Dependencies")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CubeController cubeController;
    [SerializeField] private Transform cubesParentContainer;

    [Header("Game Settings")]
    [SerializeField] private int maxCubesOnScreen = 15;
    public int MaxCubesOnScreen => maxCubesOnScreen;

    public event Action OnGameStart;
    public event Action<string> OnGameOver;
    public event Action OnGameWin;

    public event Action<int> OnScoreChanged;
    public event Action<int, int> OnScoreUIUpdated;

    private bool _isGameOver = false;
    private bool _isGameActive = false;
    private GameState _currentState;

    private int _currentScore = 0;
    private int _highScore = 0;
    private const string HighScoreKey = "HighScore2048";

    private GameObject _protectedCube;

    public bool IsGameActive => _isGameActive;

    private enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Win
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        if (cubesParentContainer == null)
        {
            GameObject container = new GameObject("Cubes_Container");
            cubesParentContainer = container.transform;
            DontDestroyOnLoad(container);
        }
    }

    private void OnDestroy()
    {
        _applicationIsQuitting = true;
    }

    private void Start()
    {
        _currentState = GameState.MainMenu;
        _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        UpdateScoreUI();
        StartGame();
    }

    public void StartGame()
    {
        _isGameOver = false;
        _isGameActive = true;
        _currentState = GameState.Playing;
        _protectedCube = null;

        ResetScore();

        StartCoroutine(CleanAndSpawn());

        StartCoroutine(CheckGameOverCondition());

        OnGameStart?.Invoke();
    }

    private IEnumerator CleanAndSpawn()
    {
        ClearAllCubes();

        yield return new WaitForEndOfFrame();

        UpdateCubeCountUI();

        CreateInitialCube();
    }

    private void CreateInitialCube()
    {
        if (cubeController == null)
        {
            cubeController = FindFirstObjectByType<CubeController>();
        }

        if (cubeController == null)
        {
            Debug.LogError("[GameStateManager] CubeController не знайдено! Гра не може продовжуватись без нього.");
            return;
        }

        cubeController.enabled = true;

        var inputHandler = cubeController.GetComponent<CubeInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.enabled = true;
        }
        else
        {
            Debug.LogWarning("[GameStateManager] InputHandler не знайдено!");
        }

        try
        {
            cubeController.SpawnNewCube();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameStateManager] Помилка при створенні нового куба: {e.Message}");
        }
    }

    private IEnumerator SaveProtectedCubeReference()
    {
        yield return new WaitForSeconds(0.1f);

        if (cubeController != null)
        {
            var methodInfo = cubeController.GetType().GetMethod("GetCurrentCube");
            if (methodInfo != null)
            {
                _protectedCube = methodInfo.Invoke(cubeController, null) as GameObject;
                if (_protectedCube != null)
                {
                    Debug.Log($"[GameStateManager] Захищено куб від знищення: {_protectedCube.name}");
                }
            }
            else
            {
                Debug.LogWarning("[GameStateManager] У CubeController немає методу GetCurrentCube!");
            }
        }
    }

    public void GameOver(string reason)
    {
        if (_isGameOver || !_isGameActive)
            return;

        _isGameOver = true;
        _currentState = GameState.GameOver;

        Debug.LogWarning(reason);

        if (uiManager != null)
        {
            StartCoroutine(ShowGameOverPanelNextFrame(reason));
        }

        if (cubeController != null)
        {
            cubeController.enabled = false;

            var inputHandler = cubeController.GetComponent<CubeInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.enabled = false;
            }
        }

        OnGameOver?.Invoke(reason);
    }

    private IEnumerator ShowGameOverPanelNextFrame(string reason)
    {
        yield return null;

        uiManager.ShowGameOverPanel();
        Debug.Log($"[GameStateManager] Показано панель завершення гри. Причина: {reason}");
    }

    public void WinGame()
    {
        if (_isGameOver || !_isGameActive)
            return;

        _currentState = GameState.Win;

        if (uiManager != null)
        {
            uiManager.ShowWinPanel();
        }

        if (cubeController != null)
        {
            cubeController.enabled = false;
        }

        OnGameWin?.Invoke();
    }

    public void ContinueAfterWin()
    {
        if (!_isGameActive)
            return;

        _currentState = GameState.Playing;

        if (cubeController != null)
        {
            cubeController.enabled = true;
        }
    }

    public void RestartGame()
    {
        Debug.Log("Перезапуск гри");

        StopAllCoroutines();

        ClearAllCubes(forceDestroy: true);

        _isGameOver = false;
        _isGameActive = false;
        _currentScore = 0;
        _currentState = GameState.MainMenu;

        uiManager = FindFirstObjectByType<UIManager>();
        cubeController = FindFirstObjectByType<CubeController>();

        if (uiManager != null)
        {
            uiManager.HideAllPanels();
            UpdateScoreUI();
        }

        StartCoroutine(DelayedGameStart());
    }

    private IEnumerator DelayedGameStart()
    {
        Debug.Log("[GameStateManager] Відкладений запуск гри...");
        yield return new WaitForSeconds(0.3f);

        CheckSceneObjects();

        if (cubeController == null)
        {
            cubeController = FindFirstObjectByType<CubeController>();
            Debug.Log("[GameStateManager] Знайдений CubeController: " + (cubeController != null ? "Так" : "Ні"));

            if (cubeController == null)
            {
                Debug.LogWarning("[GameStateManager] Створюємо новий CubeController...");
                GameObject controllerObj = new GameObject("CubeController");
                cubeController = controllerObj.AddComponent<CubeController>();
                controllerObj.AddComponent<CubeInputHandler>();
                controllerObj.AddComponent<CubeFactory>();
            }
        }

        StartGame();
    }

    private void CheckSceneObjects()
    {
        Debug.Log("[GameStateManager] Перевірка об'єктів сцени...");

        CubeController[] controllers = FindObjectsByType<CubeController>(FindObjectsSortMode.None);
        Debug.Log($"[GameStateManager] Знайдено CubeController: {controllers.Length}");

        bool hasPooler = ObjectPooler.Instance != null;
        Debug.Log($"[GameStateManager] ObjectPooler доступний: {hasPooler}");

        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        Debug.Log($"[GameStateManager] Знайдено кубів на сцені: {cubes.Length}");
    }

    private IEnumerator CheckGameOverCondition()
    {
        yield return new WaitForSeconds(0.3f);

        while (!_isGameOver && _isGameActive)
        {
            yield return new WaitForSeconds(0.5f);

            int activeCubeCount = CountActiveCubes();

            UpdateCubeCountUI(activeCubeCount);

            if (activeCubeCount > maxCubesOnScreen)
            {
                GameOver($"Гра завершена: забагато кубів на сцені ({activeCubeCount} > {maxCubesOnScreen})");
                break;
            }
        }
    }

    private IEnumerator VerifyCubeClearance()
    {
        yield return new WaitForSeconds(0.1f);

        try
        {
            GameObject[] remainingCubes = GameObject.FindGameObjectsWithTag("Cube");
            if (remainingCubes == null || remainingCubes.Length == 0)
                yield break;

            int cubesNeedingCleanup = 0;
            foreach (GameObject cube in remainingCubes)
            {
                if (cube == null)
                    continue;

                if (cube == _protectedCube)
                {
                    Debug.Log($"[GameStateManager] Пропускаємо захищений куб: {cube.name}");
                    continue;
                }

                if (cubeController != null)
                {
                    GameObject currentCube = null;

                    var methodInfo = cubeController.GetType().GetMethod("GetCurrentCube");
                    if (methodInfo != null)
                    {
                        currentCube = methodInfo.Invoke(cubeController, null) as GameObject;
                    }

                    if (currentCube == cube)
                    {
                        Debug.Log($"[GameStateManager] Пропускаємо поточний куб контролера: {cube.name}");
                        continue;
                    }
                }

                if (cube.activeInHierarchy)
                {
                    cubesNeedingCleanup++;
                    Debug.Log($"[GameStateManager] Знищуємо куб: {cube.name}");
                    Destroy(cube);
                }
            }

            if (cubesNeedingCleanup > 0)
            {
                Debug.LogWarning($"[GameStateManager] Знищено {cubesNeedingCleanup} залишкових кубів");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameStateManager] Помилка при очищенні кубів: {e.Message}");
        }
    }

    #region Score Management

    public void ResetScore()
    {
        _currentScore = 0;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(_currentScore);
    }

    public int GetCurrentScore()
    {
        return _currentScore;
    }

    public int GetHighScore()
    {
        return _highScore;
    }

    public void AddScore(int points)
    {
        _currentScore += points;

        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
            PlayerPrefs.SetInt(HighScoreKey, _highScore);
            PlayerPrefs.Save();
        }

        UpdateScoreUI();
        OnScoreChanged?.Invoke(_currentScore);
    }

    public Transform GetCubesContainer()
    {
        return cubesParentContainer;
    }

    private void UpdateScoreUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateScoreUI(_currentScore, _highScore);
            OnScoreUIUpdated?.Invoke(_currentScore, _highScore);
        }
    }

    #endregion

    #region Cube Management

    private void ClearAllCubes(bool forceDestroy = false)
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        Debug.Log($"Знайдено {cubes.Length} кубів для очищення");

        foreach (GameObject cube in cubes)
        {
            if (cube != null)
            {
                if (forceDestroy || ObjectPooler.Instance == null)
                {
                    Debug.Log($"Знищення куба: {cube.name}");
                    Destroy(cube);
                }
                else
                {
                    DisableCubeComponents(cube);

                    Debug.Log($"Повернення куба в пул: {cube.name}");
                    ObjectPooler.Instance.ReturnToPool("Cube", cube);
                }
            }
        }

        StartCoroutine(VerifyCubeClearance());
    }

    private void DisableCubeComponents(GameObject cube)
    {
        if (cube == null) return;

        Collider[] colliders = cube.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != null) col.enabled = false;
        }

        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        CubeFacade facade = cube.GetComponent<CubeFacade>();
        if (facade != null)
        {
            facade.SetInPool(true);
        }
    }

    private void UpdateCubeCountUI(int activeCubes = -1)
    {
        if (uiManager != null)
        {
            if (activeCubes < 0)
            {
                activeCubes = CountActiveCubes();
            }

            uiManager.UpdateCubeCountUI(activeCubes, maxCubesOnScreen);
        }
    }

    public int CountActiveCubes()
    {
        int count = 0;
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

        foreach (GameObject cube in allCubes)
        {
            if (cube.activeInHierarchy)
            {
                CubeFacade facade = cube.GetComponent<CubeFacade>();
                if (facade == null || !facade.IsInPool)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public bool CheckForTooManyCubes()
    {
        int activeCubeCount = CountActiveCubes();

        UpdateCubeCountUI(activeCubeCount);

        if (activeCubeCount > maxCubesOnScreen)
        {
            Debug.LogWarning($"[GameStateManager] Критичне перевищення кількості кубів: {activeCubeCount} > {maxCubesOnScreen}");
            GameOver($"Гра завершена: забагато кубів на сцені ({activeCubeCount} > {maxCubesOnScreen})");
            return true;
        }

        return false;
    }

    #endregion
}
