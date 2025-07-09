using UnityEngine;

public class DoorAnimtion : MonoBehaviour
{

    private Animator animator;
    private bool isOpen = false;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            DoorCloseAnim();
        }
        else
        {
            DoorOpenAnim();
        }
    }

    public void DoorOpenAnim()
    {
        if (isOpen) return;
        animator.SetTrigger("DoorOpen");
        isOpen = true;
    }

    public void DoorCloseAnim()
    {
        if(!isOpen) return;
        animator.SetTrigger("DoorClose");
        isOpen = false;
    }
}
