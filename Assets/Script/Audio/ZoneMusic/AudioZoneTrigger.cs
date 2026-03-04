using UnityEngine;

public class AudioZoneTrigger : MonoBehaviour
{
    [Header("Nastavení Zóny")]
    [Tooltip("0 = Chill, 1 = PreBoss, 2 = Battle, 3 = Victory")]
    public float zoneID;

    // --- PŘIDÁNO: Vypnutí zóny po boss fightu ---
    [Tooltip("Zaškrtni, pokud se má zóna trvale vypnout, jakmile boss zemře.")]
    public bool disableAfterBossDeath = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && AudioManager.instance != null)
        {
            // PŘIDÁNO: Zkontroluje, jestli už není po bossovi
            if (disableAfterBossDeath && AudioManager.instance.isBossDead)
            {
                gameObject.SetActive(false); // Vypne trigger navždy (v této instanci)
                return;
            }

            AudioManager.instance.SetZone(zoneID);
        }
    }
}