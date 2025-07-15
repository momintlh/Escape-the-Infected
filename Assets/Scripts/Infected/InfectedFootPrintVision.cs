using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InfectedFootPrintVision : MonoBehaviour
{
    [SerializeField]private Camera targetCamera;
    [SerializeField]private string layerToToggle = "FootPrintLayer";

    private bool isCooldown = false;
    private StarterAssetsInputs inputSystem;
    private Image coolDownImage;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        inputSystem = GetComponent<StarterAssetsInputs>();
        coolDownImage = InfectedUI.Instance.GetTimer();

        inputSystem.OnInteractionPlayer += InputSystem_OnInteractionPlayer; 
        CheckForFootPrints();
    }

    private void CheckForFootPrints()
    {
        int layer = LayerMask.NameToLayer(layerToToggle);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{layerToToggle}' does not exist!");
            return;
        }

        int layerMask = 1 << layer;
        targetCamera.cullingMask &= ~layerMask;

        Debug.Log($"Layer '{layerToToggle}' is hidden at start.");
    }

    private void InputSystem_OnInteractionPlayer(object sender, System.EventArgs e)
    {
       
        ToggleLayer();
    }


    //public void ToggleLayer()
    //{
    //    int layer = LayerMask.NameToLayer(layerToToggle);
    //    if (layer == -1)
    //    {
    //        Debug.LogError($"Layer '{layerToToggle}' does not exist!");
    //        return;
    //    }

    //    int layerMask = 1 << layer;

    //    // Check if the layer is currently included
    //    if ((targetCamera.cullingMask & layerMask) != 0)
    //    {
    //        // Layer is currently visible - remove it
    //        targetCamera.cullingMask &= ~layerMask;
    //        Debug.Log($"Hid layer: {layerToToggle}");
    //    }
    //    else
    //    {
    //        // Layer is currently hidden - add it
    //        targetCamera.cullingMask |= layerMask;
    //        Debug.Log($"Showed layer: {layerToToggle}");
    //    }
    //}

    public void ToggleLayer()
    {
        if (isCooldown)
        {
            Debug.Log("Toggle is on cooldown.");
            return;
        }

        int layer = LayerMask.NameToLayer(layerToToggle);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{layerToToggle}' does not exist!");
            return;
        }

        int layerMask = 1 << layer;

        // If hidden, show it
        if ((targetCamera.cullingMask & layerMask) == 0)
        {
            targetCamera.cullingMask |= layerMask;
            Debug.Log($"Showed layer: {layerToToggle}");

            StartCoroutine(AutoHideLayer(layerMask));
            StartCoroutine(ToggleCooldown());
        }
        else
        {
            Debug.Log("Layer is already visible. Ignoring.");
        }
    }

    private IEnumerator AutoHideLayer(int layerMask)
    {
        yield return new WaitForSeconds(3f);
        targetCamera.cullingMask &= ~layerMask;
        Debug.Log($"Auto-hid layer after 3 seconds");
    }

    private IEnumerator ToggleCooldown()
    {
        isCooldown = true;
        Debug.Log("Cooldown started (15s)");

        if (coolDownImage != null)
        {
            coolDownImage.fillAmount = 1f;
        }

        float cooldownTime = 15f;
        float timer = 0f;

        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            float remaining = Mathf.Clamp01(1f - (timer / cooldownTime));

            if (coolDownImage != null)
            {
                coolDownImage.fillAmount = remaining;
            }

            yield return null;
        }

        if (coolDownImage != null)
        {
            coolDownImage.fillAmount = 0f;
        }

        isCooldown = false;
        Debug.Log("Cooldown ended");
    }

}
