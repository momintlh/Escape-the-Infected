using StarterAssets;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Playroom;
using UnityEngine.Rendering.Universal;

public class Player_Jan : MonoBehaviour
{
    private const string VIGNETTE_INTENSITY = "_VignetteIntensity";

    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject flashBang;
    [SerializeField] private Transform flashBangPos;
    [SerializeField] private Transform adrenalineShotPos;
    [SerializeField] private Transform flashLight;
    [SerializeField] private Material bloodVisionMat;


    private float throwForce = 10f;
    private float interactDistance = 3f;
    private StarterAssetsInputs inputSystem;
    private FirstPersonController firstPersonController;
    private bool isDoorOpen;
    private bool isAdrenalineActive;
    PlayroomKit _playroomKit;


    private void Start()
    {
        bloodVisionMat.SetFloat(VIGNETTE_INTENSITY, 0);
        inputSystem = GetComponent<StarterAssetsInputs>();
        firstPersonController = GetComponent<FirstPersonController>();
        AssignEvents();

        flashLight.gameObject.SetActive(false);
        flashBangPos.gameObject.SetActive(false);
        adrenalineShotPos.gameObject.SetActive(false);
        _playroomKit = PlayroomManager.Instance.GetPlayroomKit();
        
    }

    private void AssignEvents()
    {
        inputSystem.OnInteractPlayer += InputSystem_OnInteractPlayer;
        inputSystem.OnUseItemPlayer += InputSystem_OnUseItemPlayer;
        inputSystem.OnSlotChange1 += InputSystem_OnSlotChange1;
        inputSystem.OnSlotChange2 += InputSystem_OnSlotChange2;
        inputSystem.OnSlotChange3 += InputSystem_OnSlotChange3;
    }

    private void InputSystem_OnSlotChange3(object sender, System.EventArgs e)
    {
        flashLight.gameObject.SetActive(false);
        flashBangPos.gameObject.SetActive(false);
        adrenalineShotPos.gameObject.SetActive(true);
        _playroomKit.RpcCall("AdrenalineActive", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    private void InputSystem_OnSlotChange2(object sender, System.EventArgs e)
    {
        flashLight.gameObject.SetActive(false);
        adrenalineShotPos.gameObject.SetActive(false);
        flashBangPos.gameObject.SetActive(true);
        _playroomKit.RpcCall("FlashbangActive", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    private void InputSystem_OnSlotChange1(object sender, System.EventArgs e)
    {
        flashLight.gameObject.SetActive(true);
        adrenalineShotPos.gameObject.SetActive(false);
        flashBangPos.gameObject.SetActive(false);
        _playroomKit.RpcCall("FlashlightActive", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    private void InputSystem_OnUseItemPlayer(object sender, System.EventArgs e)
    {
        FlashbangThrow();
        _playroomKit.RpcCall("FlashbangThrow", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    public void FlashbangThrow()
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
        else if(adrenalineShotPos.gameObject.activeSelf)
        {
            StartCoroutine(UseAdrenalineShot());
        }
        else
        {
            Debug.Log("Nothing is not equiped");
        }
    }

    private void CheckForGameObjectInView()
    {
        playerCamera = Camera.main;
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Door"))
            {
                int doorIndex = PlayroomManager.doors.IndexOf(hit.collider.gameObject);
                _playroomKit.RpcCall("ToggleDoor", doorIndex, PlayroomKit.RpcMode.ALL);
            }
        }
    }

    private void InputSystem_OnInteractPlayer(object sender, System.EventArgs e)
    {
        CheckForGameObjectInView();
    }

    IEnumerator UseAdrenalineShot()
    {
        if (isAdrenalineActive)
        {
            yield break;
        }
        isAdrenalineActive = true;
        adrenalineShotPos.gameObject.GetComponentInChildren<Animator>().SetTrigger("UseShot");
        float original = firstPersonController.SprintSpeed;
        float speedBoost = 4f;

        firstPersonController.SprintSpeed += speedBoost;

        yield return new WaitForSeconds(3.0f);

        firstPersonController.SprintSpeed = original;
        
        isAdrenalineActive = false;
    }

    public GameObject GetFlashLight()
    {
        return flashLight.gameObject;
    }
    
    public GameObject GetFlashbang()
    {
        return flashBangPos.gameObject;
    }
    public GameObject GetAdrenaline()
    {
        return adrenalineShotPos.gameObject;
    }
}
