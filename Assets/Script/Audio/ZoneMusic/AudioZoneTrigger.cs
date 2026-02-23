using UnityEngine;

public class AudioZoneTrigger : MonoBehaviour
{
    [Tooltip("Hodnota parametru pro tuto zónu (napø. 1 = fight, 2 = jeskynì)")]
    public float zoneAudioState = 1f;

    private void OnTriggerEnter(Collider other)
    {
        // Pøidána kontrola, zda SoundZoneManager existuje
        if (other.CompareTag("Player") && SoundZoneManager.instance != null)
        {
            SoundZoneManager.instance.SetZoneState(zoneAudioState);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && SoundZoneManager.instance != null)
        {
            // Návrat do klidového stavu (0)
            SoundZoneManager.instance.SetZoneState(0f);
        }
    }
}