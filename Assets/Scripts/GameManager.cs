using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private UIManager uiManager;

    [Header("Game Settings")]
    [SerializeField] private int maxCubesOnScreen = 15;
    [SerializeField] private float gameOverHeight = 4f;
    [SerializeField] private LayerMask cubeLayer;

    [Header("Platform")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CubeController cubeController;

    private int currentScore = 0;
    private int highScore = 0;
    private bool isGameOver = false;
    private bool isGameActive = false;

    private const string HighScoreKey = "HighScore2048";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Завантаження рекорду
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        // Ініціалізація UI
        if (uiManager != null)
        {
            uiManager.UpdateScoreUI(currentScore, highScore);
        }
        else
        {
            // Автоматичний пошук UI Manager
            uiManager = FindFirstObjectByType<UIManager>();
        }

        // Автоматичний запуск гри при старті
        StartGame();

        // Початкове оновлення кількості кубів в UI
        UpdateCubeCountUI();
    }

    public void StartGame()
    {
        // Скидання ігрових змінних
        isGameOver = false;
        isGameActive = true;
        currentScore = 0;

        // Очищення сцени від попередніх кубів
        ClearAllCubes();

        // Оновлення інтерфейсу
        if (uiManager != null)
        {
            uiManager.UpdateScoreUI(currentScore, highScore);
        }

        // Активація контролера куба
        if (cubeController != null)
        {
            cubeController.enabled = true;
        }

        // Запуск перевірки умови програшу
        StartCoroutine(CheckGameOverCondition());
    }

    private void ClearAllCubes()
    {
        // Return all cubes to the pool or destroy them if no pool exists
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

    public void AddScore(int points)
    {
        if (isGameOver || !isGameActive)
            return;

        currentScore += points;

        // Оновлення рекорду, якщо потрібно
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        // Оновлення інтерфейсу
        if (uiManager != null)
        {
            uiManager.UpdateScoreUI(currentScore, highScore);
            uiManager.ShowScoreAdded(points);
        }

        // Оновлюємо кількість кубів в UI
        UpdateCubeCountUI();
    }

    public void WinGame()
    {
        if (isGameOver || !isGameActive)
            return;

        // Показуємо вікно перемоги
        if (uiManager != null)
        {
            uiManager.ShowWinPanel();
        }

        // Деактивація контролера куба
        if (cubeController != null)
        {
            cubeController.enabled = false;
        }
    }

    public void ContinueAfterWin()
    {
        if (!isGameActive)
            return;

        // Дозволяємо гравцю продовжити гру після перемоги
        // Активація контролера куба
        if (cubeController != null)
        {
            cubeController.enabled = true;
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    private IEnumerator CheckGameOverCondition()
    {
        while (!isGameOver && isGameActive)
        {
            yield return new WaitForSeconds(1f);

            // Перевірка на кількість АКТИВНИХ кубів (виключаючи ті, що в пулі)
            int activeCubeCount = CountActiveCubes();

            // Оновлюємо інформацію в UI при кожній перевірці
            UpdateCubeCountUI();

            Debug.Log($"Активних кубів: {activeCubeCount}/{maxCubesOnScreen}");

            if (activeCubeCount > maxCubesOnScreen)
            {
                GameOver($"Гра завершена: забагато кубів на сцені ({activeCubeCount} > {maxCubesOnScreen})");
                continue;
            }

            // Перевірка висоти стопки куба
            Collider[] hitColliders = Physics.OverlapBox(
                new Vector3(0, gameOverHeight, 0),
                new Vector3(5, 0.5f, 5),
                Quaternion.identity,
                cubeLayer
            );

            if (hitColliders.Length > 0)
            {
                GameOver($"Гра завершена: куби досягли критичної висоти ({gameOverHeight})");
            }
        }
    }

    // Повністю перероблений метод підрахунку кубів
    private int CountActiveCubes()
    {
        int count = 0;
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

        foreach (GameObject cube in allCubes)
        {
            // Рахуємо лише активні об'єкти на ігровому полі
            if (cube.activeInHierarchy &&
                !IsObjectInPool(cube) &&
                cube.GetComponent<CubeValue>() != null)
            {
                count++;
            }
        }

        return count;
    }

    // Новий метод для перевірки, чи об'єкт знаходиться в пулі
    private bool IsObjectInPool(GameObject obj)
    {
        // Перевіряємо, чи знаходиться об'єкт за межами ігрового поля
        // або якщо він має флаг InPool з компоненту CubeValue
        Vector3 pos = obj.transform.position;
        if (pos.y < -10f || pos.x > 100f || pos.x < -100f || pos.z > 100f || pos.z < -100f)
        {
            return true;
        }

        // Перевіряємо, чи має об'єкт компоненти, які деактивовані в пулі
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null && rb.isKinematic)
        {
            return true;
        }

        // Додаткова перевірка на контрольне значення у CubeValue
        CubeValue cubeValue = obj.GetComponent<CubeValue>();
        if (cubeValue != null && cubeValue.IsInPool)
        {
            return true;
        }

        return false;
    }

    private void GameOver(string reason = "")
    {
        if (isGameOver || !isGameActive)
            return;

        isGameOver = true;

        // Виводимо причину завершення гри
        Debug.LogWarning(reason);

        // Показуємо вікно програшу
        if (uiManager != null)
        {
            uiManager.ShowGameOverPanel();
        }

        // Деактивація контролера куба
        if (cubeController != null)
        {
            cubeController.enabled = false;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateCubeCountUI()
    {
        if (uiManager != null)
        {
            int activeCubes = CountActiveCubes();
            Debug.Log($"Оновлення UI: активних кубів: {activeCubes}");
            uiManager.UpdateCubeCountUI(activeCubes, maxCubesOnScreen);
        }
    }
}