using UnityEngine;
using TMPro; 


public class CrystalRepairProximityUI : MonoBehaviour
{
    [Header("References")]
    public CrystalUIController crystalController;
    public TextMeshProUGUI progressText;

    [Header("Settings")]
    public string playerTag = "Player"; 
    public string progressTextTag = "PhaseText"; 

    private Collider2D proximityCollider;

    private bool isPlayerInRange = false;

    void Start()
    {
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
            if (progressText != null)
            {
                Debug.Log($"✅ ProgressText nalezen zálohou pomocí Tagu: {progressTextTag}");
            }
            else
            {
                Debug.LogError($"GameObject s tagem '{progressTextTag}' neobsahuje komponentu TextMeshProUGUI.");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Nebyl nalezen žádný GameObject s tagem: {progressTextTag}");
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
        if (progressText == null || other == null) return;

        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            progressText.gameObject.SetActive(true);
        }
    }

   
    private void OnTriggerExit2D(Collider2D other)
    {
        if (progressText == null || other == null) return;

        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            progressText.gameObject.SetActive(false);
        }
    }
}