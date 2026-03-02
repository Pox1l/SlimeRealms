using UnityEngine;

public class AudioZoneTrigger : MonoBehaviour
{
    [Header("Nastavení Zóny")]
    [Tooltip("0 = Chill, 1 = PreBoss, 2 = Battle, 3 = Victory")]
    public float zoneID;

    // 🔥 OPRAVENO: Změněno z OnTriggerEnter na OnTriggerEnter2D a Collider na Collider2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Funguje jen pro hráče a pokud existuje AudioManager
        if (other.CompareTag("Player") && AudioManager.instance != null)
        {
            AudioManager.instance.SetZone(zoneID);
        }
    }
}