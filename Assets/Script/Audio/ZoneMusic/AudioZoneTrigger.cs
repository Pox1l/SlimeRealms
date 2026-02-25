using UnityEngine;

public class AudioZoneTrigger : MonoBehaviour
{
    [Header("Nastavení Zóny")]
    [Tooltip("0 = Chill, 1 = PreBoss, 2 = Battle, 3 = Victory")]
    public float zoneID;

    private void OnTriggerEnter(Collider other)
    {
        // Funguje jen pro hráèe a pokud existuje AudioManager
        if (other.CompareTag("Player") && AudioManager.instance != null)
        {
            AudioManager.instance.SetZone(zoneID);
        }
    }
}