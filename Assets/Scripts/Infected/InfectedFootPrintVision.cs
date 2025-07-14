using StarterAssets;
using UnityEngine;

public class InfectedFootPrintVision : MonoBehaviour
{
    [SerializeField]private Camera targetCamera;
    [SerializeField]private string layerToToggle = "FootPrintLayer";

    private StarterAssetsInputs inputSystem;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        inputSystem = GetComponent<StarterAssetsInputs>();

        inputSystem.OnInteractionPlayer += InputSystem_OnInteractionPlayer; ;
    }

    private void InputSystem_OnInteractionPlayer(object sender, System.EventArgs e)
    {
        ToggleLayer();

    }


    public void ToggleLayer()
    {
        int layer = LayerMask.NameToLayer(layerToToggle);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{layerToToggle}' does not exist!");
            return;
        }

        int layerMask = 1 << layer;

        // Check if the layer is currently included
        if ((targetCamera.cullingMask & layerMask) != 0)
        {
            // Layer is currently visible - remove it
            targetCamera.cullingMask &= ~layerMask;
            Debug.Log($"Hid layer: {layerToToggle}");
        }
        else
        {
            // Layer is currently hidden - add it
            targetCamera.cullingMask |= layerMask;
            Debug.Log($"Showed layer: {layerToToggle}");
        }
    }

}
