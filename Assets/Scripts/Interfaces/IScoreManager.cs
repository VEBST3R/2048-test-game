using System;

public interface IScoreManager
{
    event Action<int> OnScoreChanged;
    event Action<int, int> OnScoreUIUpdated;

    int GetCurrentScore();
    int GetHighScore();
    void AddScore(int points);
    void ResetScore();
}
