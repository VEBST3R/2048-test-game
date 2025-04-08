using UnityEngine;

public interface IScoreManager
{
    int GetCurrentScore();
    int GetHighScore();
    void AddScore(int points);
}
