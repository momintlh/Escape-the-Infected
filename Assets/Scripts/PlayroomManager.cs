using UnityEngine;
using Playroom;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;

public class PlayroomManager : MonoBehaviour
{
    private PlayroomKit _playroomKit;
    [SerializeField] GameObject defaultPrefab;
    private List<Vector3> availableSpawnPoints = new List<Vector3>();
    private bool spawnPointsInitialized = false;
    private static readonly List<PlayroomKit.Player> players = new();
    private static readonly List<GameObject> playerGameObjects = new();
    private static Dictionary<string, GameObject> PlayerDict = new();

    private string myPlayerID;
    private Player myPlayerScript;
    private bool playerJoined = false;

    void Start()
    {
        InitializePlayroom();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (playerJoined && myPlayerID != null)
        {
            var myPlayer = _playroomKit.MyPlayer();
            int myIndex = players.IndexOf(myPlayer);
            
            // Add bounds checking to prevent ArgumentOutOfRangeException
            if (myIndex >= 0 && myIndex < playerGameObjects.Count && myIndex < players.Count)
            {
                var myObj = playerGameObjects[myIndex];
                var fpc = myObj.GetComponentInChildren<FirstPersonController>();
                if (fpc != null)
                {
                    fpc.JumpAndGravity();
                    fpc.GroundedCheck();
                    fpc.Move();
                    myPlayer.SetState("position", myObj.transform.position);
                    myPlayer.SetState("direction", myObj.transform.forward);
                }
                else
                {
                    Debug.LogWarning("FirstPersonController not found on player object!");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid player index: {myIndex}, players count: {players.Count}, gameObjects count: {playerGameObjects.Count}");
            }


            // Update remote players' transforms from their PlayerInfo
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == myPlayer) continue;
                var remoteObj = playerGameObjects[i];
                var remotePlayer = players[i];
                // Get position/direction from remotePlayer's state
                Vector3 pos = remotePlayer.GetState<Vector3>("position");
                Vector3 dir = remotePlayer.GetState<Vector3>("direction");
                remoteObj.transform.position = pos;
                if (dir != Vector3.zero)
                    remoteObj.transform.rotation = Quaternion.LookRotation(dir);
            }
        }
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
            defaultPlayerStates = new Dictionary<string, object>(),
        }, () =>
        {
            _playroomKit.SetState("spawnPoints", SetAvailableSpawnPoints());   
            _playroomKit.OnPlayerJoin(spawnPlayer);
            print($"[Unity Log] isHost: {_playroomKit.IsHost()}"); 
        });
    }

    void spawnPlayer(PlayroomKit.Player player)
    {
        playerJoined = true;
        
        // Get spawn points as JSON string from PlayroomKit state
        string spawnPointsJson = _playroomKit.GetState<string>("spawnPoints");
        Debug.Log($"Player {player.id} - SpawnPoints JSON: {spawnPointsJson}");
        
        // Deserialize the JSON string to List<Vector3>
        if (!string.IsNullOrEmpty(spawnPointsJson))
        {
            availableSpawnPoints = JsonUtility.FromJson<SpawnPointsData>(spawnPointsJson).spawnPoints;
            Debug.Log($"Player {player.id} - Available spawn points count: {availableSpawnPoints.Count}");
        }
        else
        {
            availableSpawnPoints = new List<Vector3>();
            Debug.Log($"Player {player.id} - No spawn points JSON found, using empty list");
        }
        
        Vector3 spawnPosition;
        if (availableSpawnPoints == null || availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning($"Player {player.id} - No spawn points available, using Vector3.zero");
            spawnPosition = Vector3.zero;
        }
        else
        {
            spawnPosition = availableSpawnPoints[0];
            Debug.Log($"Player {player.id} - Using spawn position: {spawnPosition}");
            availableSpawnPoints.RemoveAt(0);
            Debug.Log($"Player {player.id} - Remaining spawn points: {availableSpawnPoints.Count}");
        }
        
        GameObject playerObj = Instantiate(defaultPrefab, spawnPosition, Quaternion.identity);
        var info = new PlayerInfo(PlayerType.Human, spawnPosition, Vector3.zero, new List<string>());
        Player playerScript = playerObj.GetComponent<Player>();
        playerScript.Info = info;

        playerGameObjects.Add(playerObj);
        players.Add(player);
        PlayerDict[player.id] = playerObj;

        // Serialize back to JSON string for PlayroomKit
        SpawnPointsData data = new SpawnPointsData { spawnPoints = availableSpawnPoints };
        string updatedJson = JsonUtility.ToJson(data);
        Debug.Log($"Player {player.id} - Updated spawn points JSON: {updatedJson}");
        _playroomKit.SetState("spawnPoints", updatedJson);
    }

    void AssignRoles()
    {
        if (playerGameObjects.Count == 0) return;
        int monsterIndex = Random.Range(0, playerGameObjects.Count);
        for (int i = 0; i < playerGameObjects.Count; i++)
        {
            var playerScript = playerGameObjects[i].GetComponent<Player>();
            if (playerScript != null && playerScript.Info != null)
            {
                playerScript.Info.Type = (i == monsterIndex) ? PlayerType.Monster : PlayerType.Human;
            }
        }
    }

    public string SetAvailableSpawnPoints()
    {
        if (!spawnPointsInitialized)
        {
            availableSpawnPoints.Clear();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("SpawnPoint"))
            {
                Vector3 pos = go.transform.position;
                // Only add if not already in the list (manual duplicate checking)
                if (!availableSpawnPoints.Contains(pos))
                {
                    availableSpawnPoints.Add(pos);
                }
            }
            spawnPointsInitialized = true;
        }
        
        // Serialize to JSON string for PlayroomKit
        SpawnPointsData data = new SpawnPointsData { spawnPoints = availableSpawnPoints };
        return JsonUtility.ToJson(data);
    }

    // Helper class for JSON serialization
    [System.Serializable]
    public class SpawnPointsData
    {
        public List<Vector3> spawnPoints;
    }


    public static void RemovePlayer(string playerID)
    {
        if (PlayerDict.TryGetValue(playerID, out GameObject playerObj))
        {
            int index = playerGameObjects.IndexOf(playerObj);
            if (index >= 0)
            {
                playerGameObjects.RemoveAt(index);
                players.RemoveAt(index);
            }
            PlayerDict.Remove(playerID);
            Object.Destroy(playerObj);
        }
        else
        {
            Debug.LogWarning("Player is not in dictionary");
        }
    }
}

