using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [Header("Hudba pro tuto scénu")]
    public AudioClip musicClip;

    private void Start()
    {
        // Øekne AudioManageru, a zaène hrát tuto hudbu (s pøechodem 1s)
        if (musicClip != null)
        {
           // AudioManager.instance.PlayMusic(musicClip);
        }
    }
}