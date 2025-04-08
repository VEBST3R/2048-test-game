using System;
using UnityEngine;

public interface IGameStateManager
{
    event Action OnGameStart;
    event Action<string> OnGameOver;
    event Action OnGameWin;

    bool IsGameActive { get; }
    void StartGame();
    void GameOver(string reason);
    void WinGame();
    void ContinueAfterWin();
    void RestartGame();
}
