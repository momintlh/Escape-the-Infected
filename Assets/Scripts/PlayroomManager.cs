using UnityEngine;
using Playroom;
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
    GameObject defaultPrefab;

    private CinemachineVirtualCamera virtualCamera;
    private List<Vector3> availableSpawnPoints = new List<Vector3>();
    private bool spawnPointsInitialized = false;
    private static readonly List<PlayroomKit.Player> players = new();
    private static readonly List<GameObject> playerGameObjects = new();
    private static Dictionary<string, GameObject> PlayerDict = new();

    private string myPlayerID;
    private Player myPlayerScript;
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
        if (playerJoined)
        {
            var myPlayer = _playroomKit.MyPlayer();
            int myIndex = players.IndexOf(myPlayer);
            
            // Add bounds checking to prevent ArgumentOutOfRangeException
            if (myIndex >= 0 && myIndex < playerGameObjects.Count && myIndex < players.Count)
            {
                var myObj = PlayerDict[myPlayer.id];
                var fpc = myObj.GetComponent<FirstPersonController>();
                fpc.JumpAndGravity();
                fpc.GroundedCheck();
                fpc.Move();
                myPlayer.SetState("position", myObj.transform.position);
                myPlayer.SetState("direction", myObj.transform.forward);
                //myPlayer.SetState("flashlight", myObj.GetComponentInChildren<FlashLight>(true).isOn);
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
                // bool flashLight = remotePlayer.GetState<bool>("flashlight");
                // Debug.Log("[Unity Log] Flashlight state: " + flashLight);
                // Debug.Log("[Unity Log] Local flashlight: " + remoteObj.GetComponentInChildren<FlashLight>(true).isOn);
                // if (flashLight != remoteObj.GetComponentInChildren<FlashLight>(true).isOn)
                // {
                //     remoteObj.GetComponentInChildren<FlashLight>(true).ToggleFlashlight();
                // }
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
        });
    }

    public void HandleFlashlightActive(string data, string sender)
    {
        var senderObj = PlayerDict[data];
        GameObject flashLight = senderObj.GetComponent<Player_Jan>().GetFlashLight();
        GameObject flashbangPos = senderObj.GetComponent<Player_Jan>().GetFlashbang();
        flashLight.gameObject.SetActive(true);
        flashbangPos.gameObject.SetActive(false);
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
 
    //  public string SetAvailableSpawnPoints()
    // {
    //     if (!spawnPointsInitialized)
    //     {
    //         availableSpawnPoints.Clear();
    //         foreach (GameObject go in GameObject.FindGameObjectsWithTag("SpawnPoint"))
    //         {
    //             Vector3 pos = go.transform.position;
    //             // Only add if not already in the list (manual duplicate checking)
    //             if (!availableSpawnPoints.Contains(pos))
    //             {
    //                 availableSpawnPoints.Add(pos);
    //             }
    //         }
    //         spawnPointsInitialized = true;
    //     }
        
    // }

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

