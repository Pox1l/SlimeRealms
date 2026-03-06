using UnityEngine;

public class AudioZoneTrigger : MonoBehaviour
{
    [Header("Nastavení Zóny")]
    [Tooltip("0 = Chill, 1 = PreBoss, 2 = Battle, 3 = Victory/Jiné")]
    public float zoneID;

    [Tooltip("Zaškrtni, pokud se má zóna ignorovat, dokud je boss mrtvý.")]
    public bool disableAfterBossDeath = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && AudioManager.instance != null)
        {
            // POKUD JE BOSS MRTVÝ, IGNORUJ HRÁČE (nic se nepřehraje)
            if (disableAfterBossDeath && AudioManager.instance.isBossDead)
            {
                return; // 💥 ZMĚNA: Objekt se už nevypíná, jen přeruší kód.
            }

            // Pokud boss žije (nebo zóna nemá check), změň hudbu
            AudioManager.instance.SetZone(zoneID);
        }
    }
}