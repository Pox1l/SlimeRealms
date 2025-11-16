using UnityEngine;

public class CrystalInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.F;

    [Header("Reference")]
    public CrystalUIController crystalUI;
    public GameObject pressFHint;   // volitelné "Press F"

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
    }
}
