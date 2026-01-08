using UnityEngine;

public class CrystalInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;

    [Header("Visual Hint")]
    public GameObject pressFHint; // Bublina "Stiskni E" nad krystalem

    private bool playerInRange = false;

    void Start()
    {
        // Jen skryjeme nápovědu, UI už neřešíme (to dělá Manager)
        if (pressFHint != null)
            pressFHint.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        // Pokud je pauza nebo hráč je mrtvý, tlačítko nebude fungovat
        if (UIManager.Instance.isPaused || UIManager.Instance.isDead) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (UIManager.Instance != null)
            {
                // 🔥 Voláme Managera, ať to vyřeší
                UIManager.Instance.ToggleCrystalUI();
            }
            else
            {
                Debug.LogWarning("Chybí UIManager ve scéně!");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (pressFHint != null)
            pressFHint.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (pressFHint != null)
            pressFHint.SetActive(false);

        // Když hráč odejde daleko, řekneme Manageru, ať to zavře
        if (UIManager.Instance != null && UIManager.Instance.isCrystalUIOpen)
        {
            UIManager.Instance.CloseCrystalMenu();
        }
    }
}