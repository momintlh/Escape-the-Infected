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

    //Clue
    private bool isPickingObject;
    private GameObject pickedClueObject;
    PlayroomKit _playroomKit;


    private int FlashBangCount;
    private int SyringeCount;


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
        FlashBangCount = 0;
    }

    private void AssignEvents()
    {
        inputSystem.OnInteractPlayer += InputSystem_OnInteractPlayer;
        inputSystem.OnUseItemPlayer += InputSystem_OnUseItemPlayer;
        inputSystem.OnSlotChange1 += InputSystem_OnSlotChange1;
        inputSystem.OnSlotChange2 += InputSystem_OnSlotChange2;
        inputSystem.OnSlotChange3 += InputSystem_OnSlotChange3;

        LocalGameManager.Instance.OnGameOver += LocalGameManager_OnGameOver;
    }

    private void LocalGameManager_OnGameOver(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
    }

    private void InputSystem_OnSlotChange3(object sender, System.EventArgs e)
    {
        if(SyringeCount == 2)
        {
            PlayerUIManager.instance.SyringeItemFalse();
        }
        else
        {
            flashLight.gameObject.SetActive(false);
            flashBangPos.gameObject.SetActive(false);
            adrenalineShotPos.gameObject.SetActive(true);
        }
        PlayerUIManager.instance.Slot3Selected();
        _playroomKit.RpcCall("AdrenalineActive", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    private void InputSystem_OnSlotChange2(object sender, System.EventArgs e)
    {
        if (FlashBangCount == 2)
        {
            PlayerUIManager.instance.FlashBangItemFalse();
        }
        else
        {
            flashLight.gameObject.SetActive(false);
            adrenalineShotPos.gameObject.SetActive(false);
            flashBangPos.gameObject.SetActive(true);
        }

        PlayerUIManager.instance.Slot2Selected();

        _playroomKit.RpcCall("FlashbangActive", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    private void InputSystem_OnSlotChange1(object sender, System.EventArgs e)
    {
        flashLight.gameObject.SetActive(true);
        adrenalineShotPos.gameObject.SetActive(false);
        flashBangPos.gameObject.SetActive(false);
        PlayerUIManager.instance.Slot1Selected();
         _playroomKit.RpcCall("FlashlightActive", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    private void InputSystem_OnUseItemPlayer(object sender, System.EventArgs e)
    {
        UseItem();

        _playroomKit.RpcCall("UseItem", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    public void UseItem()
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
            FlashBangCount++;
            flashBangPos.gameObject.SetActive(false);
        }
        else if (adrenalineShotPos.gameObject.activeSelf)
        {
            SyringeCount++;
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
        if(isPickingObject)
        {
            Destroy(pickedClueObject);
            isPickingObject = false;
            pickedClueObject = null;
            _playroomKit.RpcCall("PickClue", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.ALL);
        }
    }

    // Adrenaline Shot Coroutine
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
        adrenalineShotPos.gameObject.SetActive(false);
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


    // Trigger enter and exist

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Clue"))
        {
            isPickingObject = true;
            pickedClueObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Clue"))
        {
            isPickingObject = false;
            pickedClueObject = null;
        }
    }
}
