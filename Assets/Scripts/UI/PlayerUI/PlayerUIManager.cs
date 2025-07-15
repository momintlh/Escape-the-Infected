using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;
using System.Collections;


public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance {  get; private set; }

    [SerializeField] private Button Slot1;
    [SerializeField] private Button Slot2;
    [SerializeField] private Button Slot3;


    private bool haveFlashBang = true;
    private bool haveSyringe= true;

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
        if (haveFlashBang)
        {
            Slot2.Select();
        }
        else
        {
            StartCoroutine(NoItem(Slot2)); 
        }

    }
    public void Slot3Selected()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (haveSyringe)
        {
            Slot3.Select();
        }
        else
        {
            StartCoroutine(NoItem(Slot3));
        }
    }


    public void FlashBangItemFalse()
    {
       haveFlashBang = false;
    }
    public void SyringeItemFalse()
    {
        haveSyringe = false;
    }
    IEnumerator NoItem(Button button)
    {
        button.image.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        button.image.color = Color.white;
    }


}
