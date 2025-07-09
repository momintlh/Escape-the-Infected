using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public PlayerInfo Info;

    void Start()
    {
        
    }

    void Update()
    {
        // Update the info with the current position and direction
        Info.Position = transform.position;
        Info.Direction = transform.forward;
    }

    // Example: Add a powerup
    public void AddPowerup(string powerup)
    {
        if (!Info.CurrentPowerups.Contains(powerup))
            Info.CurrentPowerups.Add(powerup);
    }
}