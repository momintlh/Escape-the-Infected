using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfectedUI : MonoBehaviour
{
    public static InfectedUI Instance { get; private set; }

    [SerializeField] private Image timer;
    [SerializeField] private TextMeshProUGUI gameOverTimerText;
    [SerializeField] private GameObject gameOverPanel;


    private void Awake()
    {
        Instance = this;
        HideGameOver();
    }

    private void Start()
    {
        timer.fillAmount = 0;
    }

    public Image GetTimer()
    {
        return timer;
    }

    public TextMeshProUGUI GetGameOverTimerText()
    {
        return gameOverTimerText;
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }
    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
    }
}
