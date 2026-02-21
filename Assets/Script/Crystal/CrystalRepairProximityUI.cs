using UnityEngine;
using TMPro;

public class CrystalRepairProximityUI : MonoBehaviour
{
    [Header("References")]
    public CrystalUIController crystalController;
    public TextMeshProUGUI progressText;
    public GameObject phaseBG; // PŘIDÁNO: Reference na pozadí

    [Header("Settings")]
    public string playerTag = "Player";
    public string progressTextTag = "PhaseText";
    public string phaseBGTag = "PhaseBG"; // PŘIDÁNO: Tag pro pozadí

    private Collider2D proximityCollider;
    private bool isPlayerInRange = false;

    void Start()
    {
        // --- Setup Progress Text ---
        if (progressText == null)
        {
            FindProgressTextBackup();
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("CrystalRepairProximityUI: Progress Text není nastaven ani nalezen pomocí tagu.");
        }

        // --- Setup Phase BG --- (PŘIDÁNO)
        if (phaseBG == null)
        {
            phaseBG = GameObject.FindWithTag(phaseBGTag);
        }

        if (phaseBG != null)
        {
            phaseBG.SetActive(false);
        }

        // --- Setup Collider ---
        proximityCollider = GetComponent<Collider2D>();
        if (proximityCollider == null || !proximityCollider.isTrigger)
        {
            Debug.LogError("CrystalRepairProximityUI vyžaduje na objektu Collider2D nastavený jako Trigger.");
        }

        if (crystalController == null)
        {
            Debug.LogError("Crystal Controller reference not set in CrystalRepairProximityUI!");
        }
    }

    private void FindProgressTextBackup()
    {
        GameObject textObject = GameObject.FindWithTag(progressTextTag);
        if (textObject != null)
        {
            progressText = textObject.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (isPlayerInRange && progressText != null && crystalController != null)
        {
            float repairPercentage = crystalController.GetRepairPercentage();
            progressText.text = $"Crystal proggresion: {repairPercentage:F0}%";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            if (progressText != null) progressText.gameObject.SetActive(true);
            if (phaseBG != null) phaseBG.SetActive(true); // PŘIDÁNO
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;

        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            if (progressText != null) progressText.gameObject.SetActive(false);
            if (phaseBG != null) phaseBG.SetActive(false); // PŘIDÁNO
        }
    }
}