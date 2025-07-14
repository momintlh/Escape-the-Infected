using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Update()
    {
        WalkAnim();
    }
    private void WalkAnim()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(horizontal, 0, vertical);
        float inputMagnitude = input.normalized.magnitude; // Will always be 0 to 1

        // Walk = 1, Run = 2
        float speed = inputMagnitude;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = inputMagnitude * 2f;
        }

        Debug.Log("Speed: " + speed);
        SetSpeed(speed);
    }



    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }
}
