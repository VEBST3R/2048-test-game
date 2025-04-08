using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour, IGameStateManager
{
    public event Action OnGameStart;
    public event Action<string> OnGameOver;
    public event Action OnGameWin;

    [SerializeField] private int maxCubesOnScreen = 15;
    [SerializeField] private float gameOverHeight = 4f;
    [SerializeField] private LayerMask cubeLayer;
    [SerializeField] private CubeController cubeController;

    private bool _isGameOver = false;
    private bool _isGameActive = false;

    private GameState _currentState;

    public bool IsGameActive => _isGameActive;

    private enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Win
    }

    private void Start()
    {
        _currentState = GameState.MainMenu;
    }

    public void StartGame()
    {
        // Скидання ігрових змінних
        _isGameOver = false;
        _isGameActive = true;
        _currentState = GameState.Playing;

        // Очищення сцени
        ClearAllCubes();

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

    public void GameOver(string reason)
    {
        if (_isGameOver || !_isGameActive)
            return;

        _isGameOver = true;
        _currentState = GameState.GameOver;

        // Виводимо причину завершення гри
        Debug.LogWarning(reason);

        // Деактивація контролера
        if (cubeController != null)
        {
            cubeController.enabled = false;
        }

        // Сповіщення підписників
        OnGameOver?.Invoke(reason);
    }

    public void WinGame()
    {
        if (_isGameOver || !_isGameActive)
            return;

        _currentState = GameState.Win;

        // Деактивація контролера
        if (cubeController != null)
        {
            cubeController.enabled = false;
        }

        // Сповіщення підписників
        OnGameWin?.Invoke();
    }

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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator CheckGameOverCondition()
    {
        while (!_isGameOver && _isGameActive)
        {
            yield return new WaitForSeconds(1f);

            int activeCubeCount = CountActiveCubes();
            Debug.Log($"Активних кубів: {activeCubeCount}/{maxCubesOnScreen}");

            if (activeCubeCount > maxCubesOnScreen)
            {
                GameOver($"Гра завершена: забагато кубів на сцені ({activeCubeCount} > {maxCubesOnScreen})");
                continue;
            }

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

    private int CountActiveCubes()
    {
        int count = 0;
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

        foreach (GameObject cube in allCubes)
        {
            if (cube.activeInHierarchy && !IsObjectInPool(cube) && cube.GetComponent<CubeValue>() != null)
            {
                count++;
            }
        }

        return count;
    }

    private bool IsObjectInPool(GameObject obj)
    {
        // Перевірка позиції
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
