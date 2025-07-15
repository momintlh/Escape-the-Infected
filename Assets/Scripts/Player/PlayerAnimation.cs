using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private float ikWeight = 1.0f;

    private void Update()
    {
        WalkAnim();
    }
    private void WalkAnim()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(horizontal, 0, vertical);
        float inputMagnitude = input.normalized.magnitude; // 0 to 1

        // Scale to your thresholds
        float speed = inputMagnitude * 0.5f; // Walk = 0.5 when fully pressed

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = inputMagnitude * 1.0f; // Run = 1 when fully pressed
        }

        Debug.Log("Speed: " + speed);
        SetSpeed(speed);
    }



    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }


}
