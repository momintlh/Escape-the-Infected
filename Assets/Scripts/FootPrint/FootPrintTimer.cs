using UnityEngine;

public class FootPrintTimer : MonoBehaviour
{
    [Tooltip("this is how long the decal will stay, before it shrinks away totally")]
    public float Lifetime = 2.0f;

    private float mark;
    private Vector3 OrigSize;

    public void Start()
    {
        mark = Time.time;
        OrigSize = this.transform.localScale;
    }
    public void Update()
    {
        float ElapsedTime = Time.time - mark;
        if (ElapsedTime != 0)
        {
            float PercentTimeLeft = (Lifetime - ElapsedTime) / Lifetime;

            this.transform.localScale = new Vector3(OrigSize.x * PercentTimeLeft, OrigSize.y * PercentTimeLeft, OrigSize.z * PercentTimeLeft);
            if (ElapsedTime > Lifetime)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
