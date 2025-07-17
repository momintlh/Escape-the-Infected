using UnityEngine;

public class ExitScript : MonoBehaviour
{
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (PlayroomManager.clueCount >= PlayroomManager.maxClueCount)
        {
            Debug.Log("You win!");
        }
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
        }
    }
}
