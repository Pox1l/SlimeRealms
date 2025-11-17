using UnityEngine;

public class CrystalInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;

    [Header("Reference")]
    public CrystalUIController crystalUI;
    public GameObject pressFHint;

    [Header("UI které se má vypnout při otevření krystalu")]
    public GameObject hudUI;   // <-- přiřaď tady HUD canvas

    private bool playerInRange = false;

    void Start()
    {
        if (pressFHint != null)
            pressFHint.SetActive(false);

        if (crystalUI != null && crystalUI.mainPanel != null)
            crystalUI.mainPanel.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (crystalUI == null)
            {
                Debug.LogWarning("CrystalInteraction: chybí reference na CrystalUIController!");
                return;
            }

            // === Toggle UI ===
            if (crystalUI.mainPanel.activeSelf)
            {
                CloseCrystalUI();
            }
            else
            {
                OpenCrystalUI();
            }
        }
    }

    private void OpenCrystalUI()
    {
        crystalUI.OpenUI();

        // vypni HUD
        if (hudUI != null)
            hudUI.SetActive(false);
    }

    private void CloseCrystalUI()
    {
        crystalUI.CloseUI(); // musíš mít metodu na zavření
                             // pokud nemáš, vytvořím ti ji

        // zapni HUD
        if (hudUI != null)
            hudUI.SetActive(true);
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

        // když hráč odejde, zavři UI
        if (crystalUI != null && crystalUI.mainPanel.activeSelf)
        {
            CloseCrystalUI();
        }
    }
}
