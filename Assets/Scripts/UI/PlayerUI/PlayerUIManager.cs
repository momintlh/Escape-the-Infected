using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance {  get; private set; }

    [SerializeField] private Button Slot1;
    [SerializeField] private Button Slot2;
    [SerializeField] private Button Slot3;


    private void Awake()
    {
        instance = this;
    }



    public void Slot1Selected()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Slot1.Select();
    }
    public void Slot2Selected()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Slot2.Select();
    }
    public void Slot3Selected()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Slot3.Select();
    }

}
