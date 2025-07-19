using UnityEngine;

public class ExitScript : MonoBehaviour
{
    LocalGameManager _localGameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _localGameManager = LocalGameManager.Instance.GetLocalGameManager();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (PlayroomManager.clueCount >= PlayroomManager.maxClueCount && other.CompareTag("Player"))
        {
            _localGameManager.GameOver();
        }
    }
}
