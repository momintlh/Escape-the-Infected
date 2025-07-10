using StarterAssets;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    [SerializeField] private GameObject ON;
    [SerializeField] private GameObject OFF;
    private bool isOn;

    [SerializeField] private StarterAssetsInputs starterAssets;
    
    private void Start()
    {
      //  starterAssets = GetComponentInParent<StarterAssetsInputs>();
        ON.SetActive(false);
        OFF.SetActive(true);
        isOn = false;

        starterAssets.OnInteractionPlayer += StarterAssets_OnInteractionPlayer;
        
    }

    private void StarterAssets_OnInteractionPlayer(object sender, System.EventArgs e)
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
