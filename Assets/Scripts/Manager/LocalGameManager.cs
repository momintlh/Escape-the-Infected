using System;
using UnityEngine;

public class LocalGameManager : MonoBehaviour
{
    public static LocalGameManager Instance {  get; private set; }

    public event EventHandler OnGameOver;

    private void Awake()
    {
        Instance = this;
    }

    public void GameOver()
    {
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }
}
