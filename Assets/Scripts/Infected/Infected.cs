using StarterAssets;
using UnityEngine;
using System.Collections;
using Playroom;


public class Infected : MonoBehaviour
{
    private const string VIGNETTE_INTENSITY = "_VignetteIntensity";
    private const string VIGNETTE_POWER = "_VignettePower";


    [SerializeField] private Camera playerCamera;
    [SerializeField] private Material bloodVisionMat;

    private float interactDistance = 3f;
    private StarterAssetsInputs inputSystem;
    private FirstPersonController firstPersonController;
    private InfectedAnimation infectedAnimation;
    private float currentIntensity = 20;
    private float currentPower = 7;

    private bool captureZone;
    PlayroomKit _playroomKit;

    void Start()
    {
        playerCamera = Camera.main;
        inputSystem = GetComponent<StarterAssetsInputs>();
        firstPersonController = GetComponent<FirstPersonController>();
        infectedAnimation = GetComponent<InfectedAnimation>();
        bloodVisionMat.SetFloat(VIGNETTE_INTENSITY, currentIntensity);
        AssignsEvents();
        _playroomKit = PlayroomManager.Instance.GetPlayroomKit();
        StartCoroutine(IncreaseIntensityEveryMinute());
    }

    private void AssignsEvents()
    {
        inputSystem.OnInteractPlayer += InputSystem_OnInteractPlayer;
    }

    private void InputSystem_OnInteractPlayer(object sender, System.EventArgs e)
    {
        CheckForGameObjectInView();
        if (captureZone)
        {
            PlayerUIManager.instance.ShowGameOver();
            LocalGameManager.Instance.GameOver();
        }
       _playroomKit.RpcCall("AttackAnimation", 1, PlayroomKit.RpcMode.ALL);
        CheckAndDestroyPlayerInFront();
        SoundManager.PlaySound(SoundType.Attacking);

    }

    // Checks for a 'Player' object within a 1 unit cone in front of the infected and destroys it if found
    private void CheckAndDestroyPlayerInFront()
    {
        float coneAngle = 30f; // degrees
        float coneDistance = 1f;
        Collider[] hits = Physics.OverlapSphere(transform.position, coneDistance);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);
                if (angle < coneAngle / 2f)
                {
                    string playerId = PlayroomManager.GetPlayerIdFromGameObject(hit.gameObject);
                    if (!string.IsNullOrEmpty(playerId))
                    {
                        _playroomKit.RpcCall("KillPlayer", playerId, PlayroomKit.RpcMode.ALL);
                        PlayroomManager.RemovePlayer(playerId);
                    }
                    break;
                }
            }
        }
    }


    private void CheckForGameObjectInView()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Door"))
            {
                DoorAnimtion doorAnim = hit.collider.GetComponent<DoorAnimtion>();
                if (doorAnim != null)
                {
                    doorAnim.ToggleDoor();
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            captureZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            captureZone = false;
        }
    }

    private IEnumerator IncreaseIntensityEveryMinute()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);  // Wait 1 minute

            firstPersonController.SprintSpeed += 0.5f;
            currentIntensity += 5f;
            currentPower -= 0.5f;
            bloodVisionMat.SetFloat(VIGNETTE_INTENSITY, currentIntensity);
            bloodVisionMat.SetFloat(VIGNETTE_POWER, currentPower);
            if (currentPower <= 3.5f) yield break;
        }
    }
}
