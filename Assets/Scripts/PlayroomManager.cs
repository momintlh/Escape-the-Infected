using UnityEngine;
using Playroom;
using System.Collections.Generic;
public class PlayroomManager : MonoBehaviour
{

    private PlayroomKit _playroomKit;
    [SerializeField] GameObject defaultPrefab;
    private List<PlayerInfo> playerInfos = new List<PlayerInfo>();
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private bool spawnPointsInitialized = false;
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
        // Initialize spawn points only once
        if (!spawnPointsInitialized)
        {
            availableSpawnPoints.Clear();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("SpawnPoint"))
            {
                availableSpawnPoints.Add(go.transform);
            }
            // Shuffle the list for randomness
            for (int i = 0; i < availableSpawnPoints.Count; i++)
            {
                Transform temp = availableSpawnPoints[i];
                int randomIndex = Random.Range(i, availableSpawnPoints.Count);
                availableSpawnPoints[i] = availableSpawnPoints[randomIndex];
                availableSpawnPoints[randomIndex] = temp;
            }
            spawnPointsInitialized = true;
        }

        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No available spawn points left!");
            return;
        }

        // Get and remove a spawn point from the list
        Transform spawnPoint = availableSpawnPoints[0];
        availableSpawnPoints.RemoveAt(0);

        GameObject playerObj = Instantiate(defaultPrefab, spawnPoint.position, Quaternion.identity);
        var info = new PlayerInfo(PlayerType.Human, spawnPoint.position, spawnPoint.forward, new List<string>());
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

