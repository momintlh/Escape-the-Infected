using StarterAssets;
using UnityEngine;

public class Player_Jan : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject flashBang;
    [SerializeField] private Transform flashBangPos;
    [SerializeField] private Transform flashLight;


    private float throwForce = 10f;
    private float interactDistance = 3f;
    private StarterAssetsInputs inputSystem;
    private bool isDoorOpen;
    private bool flashLightEquiped;
    private bool flashBangEquiped;

    private void Start()
    {
        inputSystem = GetComponent<StarterAssetsInputs>();
        AssignEvents();

        flashLight.gameObject.SetActive(false);
        flashBangPos.gameObject.SetActive(false);
    }

    private void AssignEvents()
    {
        inputSystem.OnInteractPlayer += InputSystem_OnInteractPlayer;
        inputSystem.OnUseItemPlayer += InputSystem_OnUseItemPlayer;
        inputSystem.OnSlotChange1 += InputSystem_OnSlotChange1;
        inputSystem.OnSlotChange2 += InputSystem_OnSlotChange2;
    }

    private void InputSystem_OnSlotChange2(object sender, System.EventArgs e)
    {
        flashLight.gameObject.SetActive(false);
        flashBangPos.gameObject.SetActive(true);
    }

    private void InputSystem_OnSlotChange1(object sender, System.EventArgs e)
    {
        flashLight.gameObject.SetActive(true);
        flashBangPos.gameObject.SetActive(false);
    }

    private void InputSystem_OnUseItemPlayer(object sender, System.EventArgs e)
    {
        if (flashBangPos.gameObject.activeSelf)
        {
            Debug.Log("Flash bang Instantiate");
            GameObject throwFlashBang = Instantiate(flashBang, flashBangPos.position, flashBangPos.rotation);

            Rigidbody flashRb = throwFlashBang.GetComponent<Rigidbody>();

            if (flashRb != null)
            {
                flashRb.AddForce(flashBangPos.forward * throwForce, ForceMode.VelocityChange);
            }
        }
        else
        {
            Debug.Log("Flash Bang is not equiped");
        }
    }

    private void Update()
    {
        // CheckForGameObjectInView();
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


    private void InputSystem_OnInteractPlayer(object sender, System.EventArgs e)
    {
        CheckForGameObjectInView();
    }

}
