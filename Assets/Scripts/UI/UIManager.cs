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

    private IGameStateManager _gameStateManager;
    private IScoreManager _scoreManager;

    private void Start()
    {
        _gameStateManager = GameManager.Instance as IGameStateManager;
        _scoreManager = GameManager.Instance as IScoreManager;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnGameWin += HandleGameWin;
            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        }

        InitializeButtons();
        HideAllPanels();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnGameWin -= HandleGameWin;
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void InitializeButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);

        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(OnRestartButtonClicked);
    }

    private void HideAllPanels()
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
        UpdateScoreUI(newScore, _scoreManager?.GetHighScore() ?? 0);
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            if (gameOverScoreText != null)
            {
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    gameOverScoreText.text = "Your Score: " + gameManager.GetCurrentScore().ToString();
                }
            }

            gameOverPanel.SetActive(true);
            StartCoroutine(AnimatePanelScale(gameOverPanel));
        }
    }

    public void UpdateScoreUI(int score, int highScore)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            if (winScoreText != null)
            {
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    winScoreText.text = "Your Score: " + gameManager.GetCurrentScore().ToString();
                }
            }

            winPanel.SetActive(true);
            StartCoroutine(AnimatePanelScale(winPanel));
        }
    }

    public void UpdateCubeCountUI(int currentCount, int maxCount)
    {
        if (cubeCountText != null)
        {
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
    }

    private IEnumerator AnimatePanelScale(GameObject panel)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
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
    }

    private void OnRestartButtonClicked()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        _gameStateManager?.RestartGame();
    }

    private void OnContinueButtonClicked()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        _gameStateManager?.ContinueAfterWin();
    }
}