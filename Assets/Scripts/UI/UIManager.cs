using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI cubeCountText;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;

    [Header("Win UI")]
    [SerializeField] private TextMeshProUGUI winScoreText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button winRestartButton;

    [SerializeField] private GameStateManager gameStateManager;

    private IGameStateManager _gameStateManager;
    private IScoreManager _scoreManager;

    private void Awake()
    {
        HideAllPanels();
    }

    private void Start()
    {
        InitializeManagers();
        InitializeButtons();
    }

    private void InitializeManagers()
    {
        _gameStateManager = gameStateManager;

        if (_gameStateManager == null)
        {
            _gameStateManager = FindFirstObjectByType<GameStateManager>();
            if (_gameStateManager == null)
                Debug.LogWarning("GameStateManager not found. UI functionality will be limited.");
        }

        _scoreManager = _gameStateManager as IScoreManager;
        if (_scoreManager == null)
            Debug.LogWarning("ScoreManager interface not implemented. Score UI will not update.");

        ConnectEventListeners();
    }

    private void ConnectEventListeners()
    {
        if (_gameStateManager != null)
        {
            _gameStateManager.OnGameOver += HandleGameOver;
            _gameStateManager.OnGameWin += HandleGameWin;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged += HandleScoreChanged;
        }
    }

    private void OnDestroy()
    {
        DisconnectEventListeners();
    }

    private void DisconnectEventListeners()
    {
        if (_gameStateManager != null)
        {
            _gameStateManager.OnGameOver -= HandleGameOver;
            _gameStateManager.OnGameWin -= HandleGameWin;
        }

        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void InitializeButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        else
            Debug.LogWarning("Restart button not assigned in UIManager");

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        else
            Debug.LogWarning("Continue button not assigned in UIManager");

        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(OnRestartButtonClicked);
        else
            Debug.LogWarning("Win restart button not assigned in UIManager");
    }

    public void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    private void HandleGameOver(string reason)
    {
        ShowGameOverPanel();
    }

    private void HandleGameWin()
    {
        ShowWinPanel();
    }

    private void HandleScoreChanged(int newScore)
    {
        int highScore = 0;
        if (_scoreManager != null)
            highScore = _scoreManager.GetHighScore();

        UpdateScoreUI(newScore, highScore);
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        StartCoroutine(DisableControllersAsync());

        if (gameOverScoreText != null && _scoreManager != null)
        {
            gameOverScoreText.text = "Your Score: " + _scoreManager.GetCurrentScore().ToString();
        }

        StartCoroutine(AnimatePanelScale(gameOverPanel));
    }

    private IEnumerator DisableControllersAsync()
    {
        yield return null;
        DisableAllCubeControllers();
    }

    public void UpdateScoreUI(int score, int highScore)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void ShowWinPanel()
    {
        if (winPanel == null) return;

        DisableAllCubeControllers();

        if (winScoreText != null && _scoreManager != null)
        {
            winScoreText.text = "Your Score: " + _scoreManager.GetCurrentScore().ToString();
        }

        winPanel.SetActive(true);
        StartCoroutine(AnimatePanelScale(winPanel));
    }

    public void UpdateCubeCountUI(int currentCount, int maxCount)
    {
        if (cubeCountText == null) return;

        if (currentCount > maxCount * 0.8f)
        {
            cubeCountText.color = Color.red;
        }
        else if (currentCount > maxCount * 0.6f)
        {
            cubeCountText.color = new Color(1.0f, 0.5f, 0.0f);
        }
        else
        {
            cubeCountText.color = Color.white;
        }

        cubeCountText.text = $"Blocks: {currentCount}/{maxCount}";
    }

    public void DisableAllCubeControllers()
    {
        try
        {
            CubeController[] cubeControllers = FindObjectsByType<CubeController>(FindObjectsSortMode.InstanceID);
            foreach (var controller in cubeControllers)
            {
                if (controller != null && controller.isActiveAndEnabled)
                {
                    controller.enabled = false;
                    Debug.Log($"Disabling cube controller: {controller.name}");
                }
            }

            CubeInputHandler[] inputHandlers = FindObjectsByType<CubeInputHandler>(FindObjectsSortMode.InstanceID);
            foreach (var handler in inputHandlers)
            {
                if (handler != null && handler.isActiveAndEnabled)
                {
                    handler.enabled = false;
                    Debug.Log($"Disabling cube input handler: {handler.name}");
                }
            }

            GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");
            if (allCubes != null && allCubes.Length > 0)
            {
                foreach (GameObject cube in allCubes)
                {
                    if (cube != null && cube.activeInHierarchy)
                    {
                        Rigidbody rb = cube.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = true;
                            rb.linearVelocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                        }
                    }
                }
            }

            Debug.Log("All cube controllers have been disabled");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error disabling cube controllers: {e.Message}\n{e.StackTrace}");
        }
    }

    private IEnumerator AnimatePanelScale(GameObject panel)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        float duration = 0.3f;
        float elapsed = 0f;

        rectTransform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float easedProgress = 1f - Mathf.Cos(progress * Mathf.PI * 0.5f);

            rectTransform.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
            yield return null;
        }

        rectTransform.localScale = endScale;
    }

    private void OnRestartButtonClicked()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (_gameStateManager != null)
            _gameStateManager.RestartGame();
        else
            Debug.LogError("Cannot restart game: GameStateManager is null");
    }

    private void OnContinueButtonClicked()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (_gameStateManager != null)
            _gameStateManager.ContinueAfterWin();
        else
            Debug.LogError("Cannot continue game: GameStateManager is null");
    }
}