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

            // Compute offset behind player in local space (-1 on local Z)
            Vector3 localOffset = new Vector3(0, 0, -1f);
            Vector3 worldOffset = transform.TransformDirection(localOffset);

            // Place footprint with offset
            Vector3 spawnPos = hit.point + worldOffset + Vector3.up * 0.01f;

            GameObject footprint = Instantiate(prefabToUse, spawnPos, Quaternion.identity);
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
