using UnityEngine;

public class CrystalInteraction : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactKey = KeyCode.F;

    [Header("References")]
    public CrystalUIController crystalUI;  // reference na UI controller
    public GameObject pressFHint;          // volitelné: ikonka "F"

    private bool playerInRange = false;

    void Start()
    {
        if (crystalUI != null && crystalUI.mainPanel != null)
        {
            crystalUI.mainPanel.SetActive(false); // UI na zaèátku vypnuté
        }

        if (pressFHint != null)
            pressFHint.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (crystalUI != null)
            {
                crystalUI.OpenUI();
            }
            else
            {
                Debug.LogWarning("CrystalInteraction: chybí reference na CrystalUIController!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (pressFHint != null)
                pressFHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (pressFHint != null)
                pressFHint.SetActive(false);
        }
    }
}
