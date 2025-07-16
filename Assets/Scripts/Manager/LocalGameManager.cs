using System;
using TMPro;
using UnityEngine;
using System.Collections;


public class LocalGameManager : MonoBehaviour
{
    public static LocalGameManager Instance { get; private set; }

    public event EventHandler OnGameOver;

    private TextMeshProUGUI playerGameOverTimer;
    private TextMeshProUGUI infectedGameOverTimer;

    [Header("Game Over Timer Settings")]
    [SerializeField] private int countdownStartSeconds = 300;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PlayerUIManager.instance != null)
            playerGameOverTimer = PlayerUIManager.instance.GetGameOverTimerText();
        if(InfectedUI.Instance != null)
            infectedGameOverTimer = InfectedUI.Instance.GetGameOverTimerText();
        
        StartCountdown();
    }

    public void StartCountdown()
    {
        StopAllCoroutines();
        StartCoroutine(GameOverCountdownCoroutine());
    }

    private IEnumerator GameOverCountdownCoroutine()
    {
        float timeLeft = countdownStartSeconds;

        while (timeLeft > 0)
        {
            UpdateTimerTexts(timeLeft);
            yield return null;
            timeLeft -= Time.deltaTime;
        }

        // Clamp to zero
        UpdateTimerTexts(0);
        PlayerUIManager.instance.ShowGameOver();    //          <------------------- Player GameOver Call
        InfectedUI.Instance.ShowGameOver();        //           <------------------- Infected GameOver Call                        
        GameOver();
    }

    private void UpdateTimerTexts(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        string formattedTime = $"{minutes:D1}:{seconds:D2}";

        Color colorToUse = timeLeft <= 60 ? Color.red : Color.white;

        if (playerGameOverTimer != null)
        {
            playerGameOverTimer.text = formattedTime;
            playerGameOverTimer.color = colorToUse;
        }


        if (infectedGameOverTimer != null)
        {
            infectedGameOverTimer.text = formattedTime;
            infectedGameOverTimer.color = colorToUse;
        }
    }
    public void GameOver()
    {
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }


}
