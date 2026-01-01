using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class GlobalUISounds : MonoBehaviour
{
    public EventReference clickSound;
    private float lastPlayTime;
    private float cooldown = 0.1f; // Minimální pauza mezi zvuky v sekundách

    void Start()
    {
        Button[] allButtons = Object.FindObjectsOfType<Button>(true);
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() => PlayClick());
        }
    }

    void PlayClick()
    {
        // Ochrana proti spamování a kontrola existence zvuku
        if (Time.unscaledTime - lastPlayTime > cooldown && !clickSound.IsNull)
        {
            RuntimeManager.PlayOneShot(clickSound);
            lastPlayTime = Time.unscaledTime;
        }
    }
}