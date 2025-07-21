using UnityEngine;
using Playroom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayroomManager : MonoBehaviour
{
    public static PlayroomManager Instance{get; private set;}
    private PlayroomKit _playroomKit;
    [SerializeField]
    private GameObject defaultPrefab;
    [SerializeField]
    private GameObject monsterPrefab;
    [SerializeField]
    public static int maxClueCount = 5;
    

    private CinemachineVirtualCamera virtualCamera;
    private List<Vector3> availableSpawnPoints =  new List<Vector3>();
    private List<GameObject> availableClues = new List<GameObject>();
    private bool spawnPointsInitialized = false;
    private bool clueLocationsInitialized = false;
    private static readonly List<PlayroomKit.Player> players = new();
    private static readonly List<GameObject> playerGameObjects = new();
    private static Dictionary<string, GameObject> PlayerDict = new();
    private static Dictionary<string, bool> isMonster = new Dictionary<string, bool>();
    public static List<GameObject> doors = new();
    private bool spawned = false;
    private bool monsterAssigned = false;
    public static int clueCount = 0;

    private bool playerJoined = false;
    LocalGameManager _localGameManager;

    void Awake()
    {
        _playroomKit = new PlayroomKit();
        Instance = this;
        Time.timeScale = 0.0f;
    }
    void Start()
    {
        doors = GetAllDoors();
        InitializePlayroom();
        _localGameManager = LocalGameManager.Instance.GetLocalGameManager();
    }

    void Update()
    {

    }

    void FixedUpdate()
     {
        if (spawned)
        {
            var myPlayer = _playroomKit.MyPlayer();
            int myIndex = players.IndexOf(myPlayer);
            
            // Add bounds checking to prevent ArgumentOutOfRangeException
            if (myIndex >= 0 && myIndex < playerGameObjects.Count && myIndex < players.Count)
            {
                if (!PlayerDict.TryGetValue(myPlayer.id, out var myObj) || myObj == null)
                return;

                var fpc = myObj.GetComponent<FirstPersonController>();
                fpc.JumpAndGravity();
                fpc.GroundedCheck();
                fpc.Move();
                myPlayer.SetState("position", myObj.transform.position);
                myPlayer.SetState("direction", myObj.transform.forward);
            }
            else
            {
                Debug.LogWarning($"Invalid player index: {myIndex}, players count: {players.Count}, gameObjects count: {playerGameObjects.Count}");
            }


            // Update remote players' transforms from their PlayerInfo
            for (int i = 0; i < players.Count; i++)
            {
                if (_playroomKit.MyPlayer().id == players[i].id) continue;
                var remotePlayer = players[i];
                GameObject remoteObj;
                bool found = PlayerDict.TryGetValue(remotePlayer.id, out remoteObj);
                // Get position/direction from remotePlayer's state
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
        if (spawned)
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
            _playroomKit.RpcRegister("AdrenalineActive", HandleAdrenalineActive);
            _playroomKit.RpcRegister("ToggleDoor", HandleToggleDoor);
            _playroomKit.RpcRegister("AssignMonster", HandleAssignMonster);
            _playroomKit.RpcRegister("PickClue", HandlePickClue);
            _playroomKit.RpcRegister("SetClueLocations", HandleSetClueLocations);
            _playroomKit.RpcRegister("AttackAnimation", HandleAttackAnimation);
            _playroomKit.RpcRegister("KillPlayer", HandleKillPlayer);
            availableClues = SortClues();
            if (_playroomKit.IsHost())
            {
                List<int> RandomClueIndices = RandomizeClueIndices(availableClues);
                string RandomClueIndicesString = string.Join(",", RandomClueIndices);
                availableSpawnPoints = GetRandomizedSpawnPoints();
                _playroomKit.RpcCall("SetClueLocations", RandomClueIndicesString, PlayroomKit.RpcMode.ALL);
            }
            Time.timeScale = 1.0f;
        });
    }
    public void HandleAttackAnimation(string data, string sender)
    {
        var senderObj = PlayerDict[sender];
        senderObj.GetComponent<InfectedAnimation>().AttackAnim();
    }
    public void HandleKillPlayer(string data, string sender)
    {
        if (data == _playroomKit.MyPlayer().id)
        _localGameManager.GameOver();
        else
        PlayerDict[data].SetActive(false);
    }
    private List<int> RandomizeClueIndices(List<GameObject> clues)
    {
                // Get a list of indices
        List<int> clueIndices = Enumerable.Range(0, clues.Count).ToList();

        for (int i = 0; i < clueIndices.Count; i++)
        {
            int rand = Random.Range(i, clueIndices.Count);
            (clueIndices[i], clueIndices[rand]) = (clueIndices[rand], clueIndices[i]);
        }
        // Select maxClueCount + 1 random indices
        return clueIndices.Take(maxClueCount + 1).ToList();
    }

    public void HandleSetClueLocations(string data, string sender)
    {
        List<int> RandomClueIndices = data.Split(',').Select(int.Parse).ToList();

        for (int i = 0; i < availableClues.Count; i++)
        {
            if (RandomClueIndices.Contains(i))
            {
                availableClues[i].SetActive(true);
            }
            else
            {
                availableClues[i].SetActive(false);
            }
        }
        Debug.Log($"Clue Locations Set: {string.Join(",", RandomClueIndices)}");
    }
    public void HandlePickClue(string data, string sender)
    {
        clueCount++;
        Debug.Log($"Clue Count: {clueCount}");
        Destroy(GameObject.Find(data));
    }

    public void HandleToggleDoor(string data, string sender)
    {
        GameObject door = doors[int.Parse(data)];
        door.GetComponent<DoorAnimtion>().ToggleDoor();
        Debug.Log("Door Toggled");
    }
    public void HandleFlashbangThrow(string data, string sender)
    {
        var senderObj = PlayerDict[data];
        senderObj.GetComponent<Player_Jan>().UseItem();  
    }

    public void HandleAssignMonster(string data, string sender)
    {
        string monsterID = data;
        Debug.Log($"Monster ID: {monsterID}");
        Debug.Log($"My Player ID: {_playroomKit.MyPlayer().id}");    
        if (!monsterAssigned)
        {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].id == monsterID)
            {
                isMonster.Add(players[i].id, true);
                Debug.Log($"Player {players[i].id} is a monster");
            }
            else
            {
                isMonster.Add(players[i].id, false);
                Debug.Log($"Player {players[i].id} is not a monster");
            }
        }
        }
                monsterAssigned = true;
        if (_playroomKit.MyPlayer().id == monsterID)
        {
            GameObject.FindWithTag("PlayerCanvas").SetActive(true);
        }
        else
        {
            GameObject.FindWithTag("InfectedCanvas").SetActive(true);
        }
    }

    public void HandleAdrenalineActive(string data, string sender)
    {
        var senderObj = PlayerDict[data];
        GameObject adrenalineShotPos = senderObj.GetComponent<Player_Jan>().GetAdrenaline();
        GameObject flashbangPos = senderObj.GetComponent<Player_Jan>().GetFlashbang();
        GameObject flashLight = senderObj.GetComponent<Player_Jan>().GetFlashLight();
        adrenalineShotPos.gameObject.SetActive(true);
        flashbangPos.gameObject.SetActive(false);
        flashLight.gameObject.SetActive(false);
    }

    public void HandleFlashlightActive(string data, string sender)
    {
        var senderObj = PlayerDict[data];
        GameObject flashLight = senderObj.GetComponent<Player_Jan>().GetFlashLight();
        GameObject flashbangPos = senderObj.GetComponent<Player_Jan>().GetFlashbang();
        GameObject adrenalineShotPos = senderObj.GetComponent<Player_Jan>().GetAdrenaline();
        flashLight.gameObject.SetActive(true);
        flashbangPos.gameObject.SetActive(false);
        adrenalineShotPos.gameObject.SetActive(false);
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
        GameObject adrenalineShotPos = senderObj.GetComponent<Player_Jan>().GetAdrenaline();
        flashbangPos.SetActive(true);
        flashLight.gameObject.SetActive(false);
        adrenalineShotPos.gameObject.SetActive(false);
    }
    void spawnPlayer(PlayroomKit.Player player)
    {
        playerJoined = true;
        players.Add(player);
        Vector3 spawnPosition = Vector3.zero;
        Debug.Log($"Player {player.id} joined");
        if (_playroomKit.IsHost() && !monsterAssigned)
        {
            string monsterID = AssignRoles();
            _playroomKit.RpcCall("AssignMonster", monsterID, PlayroomKit.RpcMode.ALL);
            spawnPosition = availableSpawnPoints[^1];
            availableSpawnPoints.RemoveAt(availableSpawnPoints.Count - 1);
            player.SetState("position", spawnPosition);
            Debug.Log($"Host set spawn position for player {player.id} at {spawnPosition}");
        }
            StartCoroutine(SpawnNonHostPlayerAfterDelay(player));
            return;

    }

    public string AssignRoles()
    {
        if (players.Count == 0) return "";
        int monsterIndex = Random.Range(0, players.Count);
        return players[monsterIndex].id;
    }
 
    public List<GameObject> SortClues()
    {
        List<GameObject> allClues = GameObject.FindGameObjectsWithTag("Clue").ToList();
        // Sort all clues (by name for determinism, or another property if needed)
        allClues.Sort((a, b) => a.name.CompareTo(b.name));

        return allClues;
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
    // Coroutine for non-host player instantiation after delay
    private IEnumerator SpawnNonHostPlayerAfterDelay(PlayroomKit.Player player)
    {
        yield return new WaitForSeconds(5f);
        Vector3 spawnPosition = player.GetState<Vector3>("position");
        GameObject playerObj;
        if (isMonster[player.id])
        {
            playerObj = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Monster instantiated for player {player.id}");
        }
        else
        {
            playerObj = Instantiate(defaultPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Player instantiated for player {player.id}");
        }
        playerGameObjects.Add(playerObj);
        PlayerDict.Add(player.id, playerObj);
        spawned = true;
        virtualCamera = PlayerDict[player.id].GetComponentInChildren<CinemachineVirtualCamera>();
        bool isLocalPlayer = (player.id == _playroomKit.MyPlayer().id);
        var input = playerObj.GetComponent<PlayerInput>();
        if (!isLocalPlayer && input != null)
        {
            Destroy(input); // Prevent remote player from capturing input
            Destroy(virtualCamera);
        }
        player.OnQuit(RemovePlayer);
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

    public List<GameObject> GetAllDoors()
    {
        return GameObject.FindGameObjectsWithTag("Door").ToList();
    }

    public static string GetPlayerIdFromGameObject(GameObject obj)
    {
        foreach (var kvp in PlayerDict)
        {
            if (kvp.Value == obj)
                return kvp.Key;
        }
        return null;
    }
}   

