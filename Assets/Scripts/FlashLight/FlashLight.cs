using StarterAssets;
using UnityEngine;
using Playroom;


public class FlashLight : MonoBehaviour
{
    [SerializeField] private GameObject ON;
    [SerializeField] private GameObject OFF;
    public bool isOn;

    PlayroomKit _playroomKit;
    [SerializeField] private StarterAssetsInputs starterAssets;
    
    private void Start()
    {
      //  starterAssets = GetComponentInParent<StarterAssetsInputs>();
        ON.SetActive(false);
        OFF.SetActive(true);
        isOn = false;

        starterAssets.OnInteractionPlayer += StarterAssets_OnInteractionPlayer;
        _playroomKit = PlayroomManager.Instance.GetPlayroomKit();
    }

    private void StarterAssets_OnInteractionPlayer(object sender, System.EventArgs e)
    {
        ToggleFlashlight();
        _playroomKit.RpcCall("ToggleFlashlight", _playroomKit.MyPlayer().id, PlayroomKit.RpcMode.OTHERS);
    }

    public void ToggleFlashlight()
    {
        if (isOn) 
        {
            ON.SetActive(false);
            OFF.SetActive(true);

        }
        if (!isOn)
        {
            ON.SetActive(true) ;
            OFF.SetActive(false);
        }

        isOn = !isOn;
    }
}
