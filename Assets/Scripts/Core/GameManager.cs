using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class GameManager : MonoBehaviour, IScoreManager, IGameStateManager
{
    // Реалізація патерну Singleton з lazy initialization та thread safety
    private static readonly object _lock = new object();
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance of GameManager already destroyed on application quit. Won't create again.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<GameManager>();
                        singleton.name = "(singleton) GameManager";
                        DontDestroyOnLoad(singleton);
                    }
                }

                return _instance;
            }
        }
    }

    private static bool _applicationIsQuitting = false;

    [Header("Dependencies")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CubeController cubeController;
    [SerializeField] private Transform cubesParentContainer;

    [Header("Game Settings")]
    [SerializeField] private int maxCubesOnScreen = 15;
    [SerializeField] private LayerMask cubeLayer;

    // Івенти для патерну Observer
    public event Action<int> OnScoreChanged;
    public event Action<int, int> OnScoreUIUpdated; // score, highScore
    public event Action<string> OnGameOver;
    public event Action OnGameWin;
    public event Action OnGameStart;

    // Стан гри
    private int _currentScore = 0;
    private int _highScore = 0;
    private bool _isGameOver = false;
    private bool _isGameActive = false;
    private GameState _currentState;

    private const string HighScoreKey = "HighScore2048";

    public bool IsGameActive => _isGameActive;

    // Патерн State для управління станом гри
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
        // Синглтон перевірка
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Ініціалізація контейнера кубів
        if (cubesParentContainer == null)
        {
            GameObject container = new GameObject("Cubes_Container");
            cubesParentContainer = container.transform;
            DontDestroyOnLoad(container);
        }

        // Ініціалізація стану гри
        _currentState = GameState.MainMenu;
    }

    private void OnDestroy()
    {
        _applicationIsQuitting = true;
    }

    private void Start()
    {
        // Завантаження рекорду
        _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        // Ініціалізація UI
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        if (uiManager != null)
        {
            UpdateScoreUI();
        }

        // Автоматичний запуск гри при старті
        StartGame();
    }

    // IGameStateManager implementation
    public void StartGame()
    {
        // Скидання ігрових змінних
        _isGameOver = false;
        _isGameActive = true;
        _currentScore = 0;
        _currentState = GameState.Playing;

        // Очищення сцени
        ClearAllCubes();

        // Оновлення UI
        UpdateScoreUI();

        // Активація контролера
        if (cubeController != null)
        {
            cubeController.enabled = true;
        }

        // Запуск перевірки умови програшу
        StartCoroutine(CheckGameOverCondition());

        // Сповіщення підписників
        OnGameStart?.Invoke();
    }

    // IGameStateManager implementation
    public void GameOver(string reason)
    {
        if (_isGameOver || !_isGameActive)
            return;

        _isGameOver = true;
        _currentState = GameState.GameOver;

        // Виводимо причину завершення гри
        Debug.LogWarning(reason);

        // Показ вікна програшу
        if (uiManager != null)
        {
            uiManager.ShowGameOverPanel();
        }

        // Деактивація контролера
        if (cubeController != null)
        {
            cubeController.enabled = false;
        }

        // Сповіщення підписників
        OnGameOver?.Invoke(reason);
    }

    // IGameStateManager implementation
    public void WinGame()
    {
        if (_isGameOver || !_isGameActive)
            return;

        _currentState = GameState.Win;

        // Показ вікна перемоги
        if (uiManager != null)
        {
            uiManager.ShowWinPanel();
        }

        // Деактивація контролера
        if (cubeController != null)
        {
            cubeController.enabled = false;
        }

        // Сповіщення підписників
        OnGameWin?.Invoke();
    }

    // IGameStateManager implementation
    public void ContinueAfterWin()
    {
        if (!_isGameActive)
            return;

        _currentState = GameState.Playing;

        // Активація контролера 
        if (cubeController != null)
        {
            cubeController.enabled = true;
        }
    }

    // IGameStateManager implementation
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // IScoreManager implementation
    public int GetCurrentScore()
    {
        return _currentScore;
    }

    // IScoreManager implementation
    public int GetHighScore()
    {
        return _highScore;
    }

    // IScoreManager implementation
    public void AddScore(int points)
    {
        if (_isGameOver || !_isGameActive)
            return;

        _currentScore += points;

        // Оновлення рекорду
        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
            PlayerPrefs.SetInt(HighScoreKey, _highScore);
            PlayerPrefs.Save();
        }

        // Оновлення UI
        UpdateScoreUI();

        // Сповіщення підписників
        OnScoreChanged?.Invoke(_currentScore);
    }

    // Доступ до батьківського контейнера кубів
    public Transform GetCubesContainer()
    {
        return cubesParentContainer;
    }

    private void UpdateScoreUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateScoreUI(_currentScore, _highScore);

            // Оновлення кількості кубів
            int activeCubes = CountActiveCubes();
            uiManager.UpdateCubeCountUI(activeCubes, maxCubesOnScreen);

            // Сповіщення підписників
            OnScoreUIUpdated?.Invoke(_currentScore, _highScore);
        }
    }

    private void ClearAllCubes()
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject cube in cubes)
        {
            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.ReturnToPool("Cube", cube);
            }
            else
            {
                Destroy(cube);
            }
        }
    }

    private IEnumerator CheckGameOverCondition()
    {
        while (!_isGameOver && _isGameActive)
        {
            yield return new WaitForSeconds(1f);

            int activeCubeCount = CountActiveCubes();
            UpdateScoreUI();

            Debug.Log($"Активних кубів: {activeCubeCount}/{maxCubesOnScreen}");

            if (activeCubeCount > maxCubesOnScreen)
            {
                GameOver($"Гра завершена: забагато кубів на сцені ({activeCubeCount} > {maxCubesOnScreen})");
            }
        }
    }

    private int CountActiveCubes()
    {
        int count = 0;
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

        foreach (GameObject cube in allCubes)
        {
            if (cube.activeInHierarchy && !IsObjectInPool(cube))
            {
                count++;
            }
        }

        return count;
    }

    private bool IsObjectInPool(GameObject obj)
    {
        // Перевірка позиції куба
        Vector3 pos = obj.transform.position;
        if (pos.y < -10f || pos.x > 100f || pos.x < -100f || pos.z > 100f || pos.z < -100f)
        {
            return true;
        }

        // Перевірка стану Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null && rb.isKinematic)
        {
            return true;
        }

        // Спочатку перевіряємо CubeFacade (новий підхід)
        CubeFacade cubeFacade = obj.GetComponent<CubeFacade>();
        if (cubeFacade != null && cubeFacade.IsInPool)
        {
            return true;
        }

        return false;
    }
}