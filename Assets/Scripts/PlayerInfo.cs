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
    public string playroomID;
    public PlayerType Type;
    public Vector3 Position;
    public Vector3 Direction;
    public List<string> CurrentPowerups;

    public PlayerInfo(string playroomID, PlayerType type, Vector3 position, Vector3 direction, List<string> powerups)
    {
        this.playroomID = playroomID;
        Type = type;
        Position = position;
        Direction = direction;
        CurrentPowerups = new List<string>(powerups);
    }
}
