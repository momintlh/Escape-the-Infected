using UnityEngine;
using Playroom;
using System.Collections.Generic;
public class PlayroomManager : MonoBehaviour
{

    private PlayroomKit _playroomKit;
    [SerializeField] GameObject defaultPrefab;
    private List<PlayerInfo> playerInfos = new List<PlayerInfo>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    InitializePlayroom();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        _playroomKit = new PlayroomKit();
    }
    void InitializePlayroom()
    {
        _playroomKit.InsertCoin(new InitOptions()
        {
            maxPlayersPerRoom = 4,
            defaultPlayerStates = new() {
        },
        }, () =>
        {
            _playroomKit.OnPlayerJoin(spawnPlayer);
            print($"[Unity Log] isHost: {_playroomKit.IsHost()}");

        });
    }
    void spawnPlayer(PlayroomKit.Player player)
    {
        GameObject playerObj = Instantiate(defaultPrefab, Vector3.zero, Quaternion.identity);
        var info = new PlayerInfo(PlayerType.Human, Vector3.zero, Vector3.zero, new List<string>());
        playerObj.GetComponent<Player>().Info = info;

        playerInfos.Add(info);
        AssignRoles();
        print($"[Unity Log] playerInfos: {playerInfos}");
    }

    void AssignRoles()
    {
        if (playerInfos.Count == 0) return;

        int monsterIndex = Random.Range(0, playerInfos.Count);

        for (int i = 0; i < playerInfos.Count; i++)
        {
            playerInfos[i].Type = (i == monsterIndex) ? PlayerType.Monster : PlayerType.Human;
        }
    }
}

