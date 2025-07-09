using StarterAssets;
using UnityEngine;

public class Player_Jan : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;


    private float interactDistance = 3f;
    private StarterAssetsInputs inputSystem;
    private bool isDoorOpen;

    private void Start()
    {
        inputSystem = GetComponent<StarterAssetsInputs>();

        inputSystem.OnInteractPlayer += InputSystem_OnInteractPlayer;
    }
    private void Update()
    {
       // CheckForGameObjectInView();
    }

    private void CheckForGameObjectInView()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width /2 , Screen.height /2));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if(hit.collider.CompareTag("Door"))
            {
                DoorAnimtion doorAnim = hit.collider.GetComponent<DoorAnimtion>();
                if (doorAnim != null)
                {
                   doorAnim.ToggleDoor();
                }
            }
        }
    }


    private void InputSystem_OnInteractPlayer(object sender, System.EventArgs e)
    {
        CheckForGameObjectInView();
    }

}
