using UnityEngine;
using UnityEngine.UI;

public class InfectedUI : MonoBehaviour
{
    public static InfectedUI Instance { get; private set; }

    [SerializeField] private Image timer;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        timer.fillAmount = 0;
    }

    public Image GetTimer()
    {
        return timer;
    }
}
