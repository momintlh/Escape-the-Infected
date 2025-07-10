using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashBang : MonoBehaviour
{
    [SerializeField] private Image whiteScreenImg;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private ParticleSystem flashparticle;

    private Camera playerCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        flashparticle = GetComponent<ParticleSystem>();

        playerCamera = Camera.main;
        if (whiteScreenImg == null)
        {
            whiteScreenImg = GameObject.FindGameObjectWithTag("WhiteScreen").GetComponent<Image>();
        }

        rb.angularVelocity = new Vector3(10, 0, 0);

        StartCoroutine(WhiteFade());
    }

    private IEnumerator WhiteFade()
    {
        yield return new WaitForSeconds(2f);

        if (IsInViewAndVisible())
        {
            whiteScreenImg.color = new Vector4(1, 1, 1, 1);
        }
        flashparticle.Play();
        meshRenderer.enabled = false;

        float fadeSpeed = 1.0f;
        float modifier = 0.01f;
        float waitTime = 0;

        for (int i = 0; whiteScreenImg.color.a > 0; i++)
        {
            whiteScreenImg.color = new Vector4(1,1,1,fadeSpeed);
            fadeSpeed = fadeSpeed - 0.025f;
            modifier = modifier * 1.5f;
            waitTime = 0.5f - modifier;
            if (waitTime < 0.1f) waitTime = 0.1f;
            yield return new WaitForSeconds(waitTime);
        }

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
    private bool IsInViewAndVisible()
    {
        // Step 1: Check if within view frustum
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(transform.position);

        bool inView = viewportPoint.z > 0 &&
                      viewportPoint.x > 0 && viewportPoint.x < 1 &&
                      viewportPoint.y > 0 && viewportPoint.y < 1;

        if (!inView) return false;

        // Step 2: Check if not blocked by wall
        Vector3 dirToFlash = transform.position - playerCamera.transform.position;
        Ray ray = new Ray(playerCamera.transform.position, dirToFlash.normalized);

        if (Physics.Raycast(ray, out RaycastHit hit, dirToFlash.magnitude))
        {
            // Not visible if something else is in the way
            if (hit.transform != transform)
                return false;
        }

        return true;
    }

}
