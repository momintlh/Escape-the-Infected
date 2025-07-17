using UnityEngine;

public class InfectedAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float maxSpeed = 6.0f; // Set to SprintSpeed
    [SerializeField] private float smoothing = 10f;
    private float currentAnimSpeed = 0f;

    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        UpdateSpeedAnim();
    }

    private void UpdateSpeedAnim()
    {
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;
        delta.y = 0; // Ignore vertical movement

        float speed = delta.magnitude / Time.deltaTime; // units per second
        float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);

        currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, normalizedSpeed, smoothing * Time.deltaTime);
        SetSpeed(currentAnimSpeed);

        lastPosition = currentPosition;
    }

    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    public void AttackAnim()
    {
        animator.SetTrigger("Attack");
    }
}
