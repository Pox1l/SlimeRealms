using UnityEngine;

public class BossExitTrigger : MonoBehaviour
{
    [Header("Propojení")]
    public BossEntrance entranceScript; // Odkaz na barikádu (tu co zavíráš)
    public BossEncounter bossEncounter; // Odkaz na logiku bosse

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Reagujeme jen na hráèe
        if (!other.CompareTag("Player")) return;

        // Zavøeme JENOM pokud je boss mrtvý
        if (bossEncounter != null && bossEncounter.bossDefeated)
        {
            entranceScript.ResetBarrier();

            // Volitelné: Vypneme tento trigger, aby se kód nevolal zbyteènì znovu, 
            // dokud se boss neresetuje (pokud se resetuje celá scéna, není to tøeba).
            // gameObject.SetActive(false); 
        }
    }
}