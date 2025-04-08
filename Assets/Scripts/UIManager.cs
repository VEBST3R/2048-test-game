using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gamePlayPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI scoreAddedText;
    [SerializeField] private TextMeshProUGUI cubeCountText;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;

    [Header("Win UI")]
    [SerializeField] private TextMeshProUGUI winScoreText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button winRestartButton;

    private void Start()
    {
        // Ініціалізація кнопки рестарту
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        // Додаємо обробники для нових кнопок
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);

        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(OnRestartButtonClicked);

        // Приховуємо всі панелі крім ігрової на початку
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            // Оновлюємо текст рахунку в панелі програшу
            if (gameOverScoreText != null)
            {
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    gameOverScoreText.text = "Your Score: " + gameManager.GetCurrentScore().ToString();
                }
            }

            gameOverPanel.SetActive(true);
            // Додаємо анімацію появи панелі
            StartCoroutine(AnimatePanelScale(gameOverPanel));
        }
    }

    public void UpdateScoreUI(int score, int highScore)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (highScoreText != null)
            highScoreText.text = "Record: " + highScore;
    }

    public void ShowScoreAdded(int points)
    {
        if (scoreAddedText != null)
        {
            scoreAddedText.text = "+" + points;
            scoreAddedText.gameObject.SetActive(true);

            // Приховування через деякий час
            StartCoroutine(HideScoreAddedAfterDelay());
        }
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            // Оновлюємо текст рахунку в панелі виграшу
            if (winScoreText != null)
            {
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    winScoreText.text = "Your Score: " + gameManager.GetCurrentScore().ToString();
                }
            }

            winPanel.SetActive(true);
            // Додаємо анімацію появи панелі
            StartCoroutine(AnimatePanelScale(winPanel));
        }
    }

    public void UpdateCubeCountUI(int currentCount, int maxCount)
    {
        if (cubeCountText != null)
        {
            // Змінюємо колір тексту залежно від наближення до максимуму
            if (currentCount > maxCount * 0.8f) // Якщо більше 80% від максимуму
            {
                cubeCountText.color = Color.red; // Червоний колір для попередження
            }
            else if (currentCount > maxCount * 0.6f) // Якщо більше 60% від максимуму
            {
                cubeCountText.color = new Color(1.0f, 0.5f, 0.0f); // Оранжевий колір
            }
            else
            {
                cubeCountText.color = Color.white; // Звичайний білий колір
            }

            // Форматуємо текст
            cubeCountText.text = $"Blocks: {currentCount}/{maxCount}";
        }
    }

    private IEnumerator HideScoreAddedAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        if (scoreAddedText != null)
        {
            scoreAddedText.gameObject.SetActive(false);
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
        // Перезапуск гри
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    private void OnContinueButtonClicked()
    {
        // Закриваємо панель перемоги
        if (winPanel != null)
            winPanel.SetActive(false);

        // Продовжуємо гру
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ContinueAfterWin();
        }
    }
}