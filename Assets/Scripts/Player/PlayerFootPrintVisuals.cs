using UnityEngine;

public class PlayerFootPrintVisuals : MonoBehaviour
{
    [Header("FootPrint Prefabs")]
    [SerializeField] private GameObject leftFootPrintPrefab;
    [SerializeField] private GameObject rightFootPrintPrefab;

    [Header("Space Between FootPrints")]
    [SerializeField] private float footPrintSpacing = 1.0f;

    [Header("Ground settings")]
    public LayerMask groundMask = ~0;  // By default, raycasts hit everything

    private Vector3 lastFootprintPos;
    private bool useLeftFoot = true;
    private bool isGrounded = true;

    void Start()
    {
        lastFootprintPos = transform.position;
    }

    void Update()
    {
        // Check distance traveled since last footprint
        float distance = Vector3.Distance(transform.position, lastFootprintPos);

        if (isGrounded && distance >= footPrintSpacing)
        {
            SpawnFootprint();
            lastFootprintPos = transform.position;
        }
    }

    private void SpawnFootprint()
    {
        // Raycast down to find ground
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2f, groundMask))
        {
            GameObject prefabToUse = useLeftFoot ? leftFootPrintPrefab : rightFootPrintPrefab;
            if (prefabToUse == null) return;

            // Spawn footprint at hit point
            GameObject footprint = Instantiate(prefabToUse, hit.point + Vector3.up * 0.01f, Quaternion.identity);
            footprint.transform.rotation = Quaternion.Euler(90, transform.eulerAngles.y, 0);

            useLeftFoot = !useLeftFoot;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }


}
