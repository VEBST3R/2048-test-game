using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour, IScoreManager
{
    public event Action<int> OnScoreChanged;
    public event Action<int, int> OnScoreUIUpdated;

    private int _currentScore = 0;
    private int _highScore = 0;

    private const string HighScoreKey = "HighScore2048";

    private void Start()
    {
        LoadHighScore();
    }

    public void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        OnScoreUIUpdated?.Invoke(_currentScore, _highScore);
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

        OnScoreChanged?.Invoke(_currentScore);
        OnScoreUIUpdated?.Invoke(_currentScore, _highScore);
    }

    public void ResetScore()
    {
        _currentScore = 0;
        OnScoreChanged?.Invoke(_currentScore);
        OnScoreUIUpdated?.Invoke(_currentScore, _highScore);
    }
}
