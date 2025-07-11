using StarterAssets;
using UnityEngine;

public class Infected : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;


    private float interactDistance = 3f;
    private StarterAssetsInputs inputSystem;

    void Start()
    {
        inputSystem = GetComponent<StarterAssetsInputs>();

        AssignsEvents();
    }

    private void AssignsEvents()
    {
        inputSystem.OnInteractPlayer += InputSystem_OnInteractPlayer;
    }

    private void InputSystem_OnInteractPlayer(object sender, System.EventArgs e)
    {
        CheckForGameObjectInView();
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
}
