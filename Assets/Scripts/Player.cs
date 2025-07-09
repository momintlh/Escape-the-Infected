using UnityEngine;
using System.Collections.Generic;

public enum PlayerType
{
    Human,
    Monster
}

[System.Serializable]
public class PlayerInfo
{
    public PlayerType Type;
    public Vector3 Position;
    public Vector3 Direction;
    public List<string> CurrentPowerups;

    public PlayerInfo(PlayerType type, Vector3 position, Vector3 direction, List<string> powerups)
    {
        Type = type;
        Position = position;
        Direction = direction;
        CurrentPowerups = new List<string>(powerups);
    }
}