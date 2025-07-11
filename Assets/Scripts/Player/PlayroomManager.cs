using UnityEngine;
using Playroom;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using Cinemachine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

public class PlayroomManager : MonoBehaviour
{
    public static PlayroomManager Instance{get; private set;}
    private PlayroomKit _playroomKit;
    [SerializeField]
    GameObject defaultPrefab;

    private CinemachineVirtualCamera virtualCamera;
    private List<Vector3> availableSpawnPoints = new List<Vector3>();
    private bool spawnPointsInitialized = false;
    private static readonly List<PlayroomKit.Player> players = new();
    private static readonly List<GameObject> playerGameObjects = new();
    private static Dictionary<string, GameObject> PlayerDict = new();
    private bool hasSpawned = false;


    private bool playerJoined = false;

    void Awake()
    {
        _playroomKit = new PlayroomKit();
        Instance = this;
    }
    void Start()
    {
        InitializePlayroom();
    }

    void Update()
    {

    }

    void FixedUpdate()
{
    if (playerJoined && hasSpawned)
    {
        var myPlayer = _playroomKit.MyPlayer();
        int myIndex = players.IndexOf(myPlayer);

        if (myIndex >= 0 && myIndex < playerGameObjects.Count && myIndex < players.Count)
        {
            var myObj = PlayerDict[myPlayer.id];
            var fpc = myObj.GetComponent<FirstPersonController>();
            fpc.JumpAndGravity();
            fpc.GroundedCheck();
            fpc.Move();
            myPlayer.SetState("position", myObj.transform.position);
            myPlayer.SetState("direction", myObj.transform.forward);
        }
    }

    // Update remote players
    for (int i = 0; i < players.Count; i++)
    {
        var remotePlayer = players[i];
        if (_playroomKit.MyPlayer().id == remotePlayer.id) continue;

        if (PlayerDict.TryGetValue(remotePlayer.id, out var remoteObj))
        {
            Vector3 pos = remotePlayer.GetState<Vector3>("position");
            Vector3 dir = remotePlayer.GetState<Vector3>("direction");

            remoteObj.transform.position = pos;
            if (dir != Vector3.zero)
                remoteObj.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}

    void LateUpdate()
    {
        if (playerJoined)
        {
        var myPlayer = _playroomKit.MyPlayer();
            int myIndex = players.IndexOf(myPlayer);
        
        var myObj = PlayerDict[myPlayer.id];
        var fpc = myObj.GetComponent<FirstPersonController>();
        fpc.CameraRotation();
        }   
    }

    void InitializePlayroom()
    {
        _playroomKit.InsertCoin(new InitOptions()
        {
            gameId =  "vWV6hHHXlrUgYGzS3GW0",
            maxPlayersPerRoom = 4,
            defaultPlayerStates = new Dictionary<string, object>(),
        }, () =>
        {
            _playroomKit.OnPlayerJoin(spawnPlayer);
            print($"[Unity Log] isHost: {_playroomKit.IsHost()}");
            _playroomKit.RpcRegister("ToggleFlashlight", HandleToggleFlashlight);
            _playroomKit.RpcRegister("FlashlightActive", HandleFlashlightActive);
            _playroomKit.RpcRegister("FlashbangActive", HandleFlashbangActive);
            _playroomKit.RpcRegister("FlashbangThrow", HandleFlashbangThrow);
            _playroomKit.RpcRegister("setSpawnLocation", HandleSetSpawnLocation);
            
            if (_playroomKit.IsHost())
            {   
                List<Vector3> spawnPoints = GetRandomizedSpawnPoints();
                Dictionary<string, Vector3> spawnMap = new Dictionary<string, Vector3>();
                for (int i = 0; i < players.Count; i++)
                {
                    spawnMap.Add(players[i].id, spawnPoints[i]);
                }
                _playroomKit.RpcCall("setSpawnLocation", spawnMap, PlayroomKit.RpcMode.ALL);
            }
        });
    }
    public void HandleFlashbangThrow(string data, string sender)
    {
        var senderObj = PlayerDict[data];
        senderObj.GetComponent<Player_Jan>().FlashbangThrow();  
    }
    public void HandleFlashlightActive(string data, string sender)
    {
        var senderObj = PlayerDict[data];
        GameObject flashLight = senderObj.GetComponent<Player_Jan>().GetFlashLight();
        GameObject flashbangPos = senderObj.GetComponent<Player_Jan>().GetFlashbang();
        flashLight.gameObject.SetActive(true);
        flashbangPos.gameObject.SetActive(false);
    }

    public void HandleSetSpawnLocation(string data, string sender)
{
    Dictionary<string, Vector3> spawnMap = JsonConvert.DeserializeObject<Dictionary<string, Vector3>>(data);
    var myPlayer = _playroomKit.MyPlayer();
    if (!spawnMap.ContainsKey(myPlayer.id))
    {
        Debug.LogWarning($"[Spawn] Spawn map does not contain my player ID: {myPlayer.id}");
        return;
    }

    if (!PlayerDict.TryGetValue(myPlayer.id, out GameObject myObj))
    {
        Debug.LogWarning($"[Spawn] PlayerDict does not yet contain player {myPlayer.id}");
        return;
    }

    Vector3 spawnPos = spawnMap[myPlayer.id];
    myObj.transform.position = spawnPos;
    myPlayer.SetState("position", spawnPos);
    hasSpawned = true;

    Debug.Log($"[Spawn] Applied spawn position {spawnPos} for player {myPlayer.id}");
}

    public void HandleToggleFlashlight(string data, string sender)
    {
        var senderObj = PlayerDict[data];
        senderObj.GetComponentInChildren<FlashLight>(true).ToggleFlashlight();
    }
    public void HandleFlashbangActive(string data, string sender)
    {
        var senderObj = PlayerDict[data];
        GameObject flashLight = senderObj.GetComponent<Player_Jan>().GetFlashLight();
        GameObject flashbangPos = senderObj.GetComponent<Player_Jan>().GetFlashbang();
        flashbangPos.SetActive(true);
        flashLight.gameObject.SetActive(false);
    }
    void spawnPlayer(PlayroomKit.Player player)
    {
        playerJoined = true;
        
        GameObject playerObj;
        if (_playroomKit.IsHost())
        {
            playerObj = Instantiate(defaultPrefab, new Vector3(0, 2, 0), Quaternion.identity);
        }
        else
        {
            playerObj = Instantiate(defaultPrefab, new Vector3(0, 2, 5), Quaternion.identity);
        }
        // var info = new PlayerInfo(PlayerType.Human, playerObj.transform.position, Vector3.zero, new List<string>());
        //Player playerScript = playerObj.GetComponent<Player>();
        //playerScript.Info = info;

        playerGameObjects.Add(playerObj);
        players.Add(player);
        PlayerDict.Add(player.id, playerObj);
        virtualCamera = PlayerDict[player.id].GetComponentInChildren<CinemachineVirtualCamera>();

        bool isLocalPlayer = (player.id == _playroomKit.MyPlayer().id);
        var input = playerObj.GetComponent<PlayerInput>();
        if (!isLocalPlayer && input != null)
        {
            Destroy(input); // Prevent remote player from capturing input
            Destroy(virtualCamera);
        }
        player.OnQuit(RemovePlayer);

    if (_playroomKit.IsHost())
{
    // Recalculate and send spawn map now that a new player joined
    List<Vector3> spawnPoints = GetRandomizedSpawnPoints();
    Dictionary<string, Vector3> spawnMap = new();

    for (int i = 0; i < players.Count && i < spawnPoints.Count; i++)
    {
        spawnMap[players[i].id] = spawnPoints[i];
    }

    _playroomKit.RpcCall("setSpawnLocation", spawnMap, PlayroomKit.RpcMode.ALL);
}

    }

    void AssignRoles()
    {
        if (playerGameObjects.Count == 0) return;
        int monsterIndex = Random.Range(0, playerGameObjects.Count);
        for (int i = 0; i < playerGameObjects.Count; i++)
        {
            // var playerScript = playerGameObjects[i].GetComponent<Player>();
            // if (playerScript != null && playerScript.Info != null)
            // {
            //     playerScript.Info.Type = (i == monsterIndex) ? PlayerType.Monster : PlayerType.Human;
            // }
        }
    }
 

    public List<Vector3> GetRandomizedSpawnPoints()
    {
        if (!spawnPointsInitialized)
        {
            availableSpawnPoints.Clear();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("SpawnPoint"))
            {
                Vector3 pos = go.transform.position;
                if (!availableSpawnPoints.Contains(pos))
                    availableSpawnPoints.Add(pos);
            }

            // Shuffle
            for (int i = 0; i < availableSpawnPoints.Count; i++)
            {
                int rand = Random.Range(i, availableSpawnPoints.Count);
                (availableSpawnPoints[i], availableSpawnPoints[rand]) = 
                    (availableSpawnPoints[rand], availableSpawnPoints[i]);
            }

            spawnPointsInitialized = true;
        }

        return new List<Vector3>(availableSpawnPoints);
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

    public PlayroomKit GetPlayroomKit()
    {
        return _playroomKit;
    }
}

