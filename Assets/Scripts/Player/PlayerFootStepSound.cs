using UnityEngine;

public class PlayerFootStepSound : MonoBehaviour
{
    public void PlaySoundFootSteps()
    {
        SoundManager.PlaySound(SoundType.Running);
    }
}
