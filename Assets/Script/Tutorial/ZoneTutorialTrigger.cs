using UnityEngine;
using UnityEngine.Events; // Pøidáno pro UI eventy

[RequireComponent(typeof(Collider2D))]
public class ZoneTutorialTrigger : MonoBehaviour
{
    [Header("Tutorial")]
    [Tooltip("Název eventu pro tutoriál (napø. 'crystalZone').")]
    public string tutorialEventName;

    [Header("Interakce (Zmaèknutí tlaèítka)")]
    [Tooltip("Pokud je zaškrtnuto, hráè musí v zónì zmáèknout klávesu.")]
    public bool requireKeyPress = false;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Nápovìda (Zobrazit/Skrýt panel)")]
    public UnityEvent onZoneEnter; // Spustí se, když hráè vejde (napø. zapnutí UI "Zmáèkni E")
    public UnityEvent onZoneExit;  // Spustí se, když hráè odejde nebo splní úkol (vypnutí UI)

    private bool playerInZone = false;
    private bool hasTriggered = false;

    private void Update()
    {
        // Kontroluje zmáèknutí klávesy pouze pokud je hráè v zónì a úkol ještì nebyl splnìn
        if (playerInZone && !hasTriggered && requireKeyPress)
        {
            if (Input.GetKeyDown(interactKey))
            {
                TriggerTutorialEvent();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            if (requireKeyPress)
            {
                playerInZone = true;
                onZoneEnter.Invoke(); // Zobrazí nápovìdu "Zmáèkni E"
            }
            else
            {
                TriggerTutorialEvent(); // Pokud není potøeba tlaèítko, rovnou splní
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            onZoneExit.Invoke(); // Skryje nápovìdu, když hráè odejde
        }
    }

    private void TriggerTutorialEvent()
    {
        if (!string.IsNullOrEmpty(tutorialEventName) && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.TriggerEvent(tutorialEventName);
            hasTriggered = true;
            onZoneExit.Invoke(); // Skryje nápovìdu "Zmáèkni E" po úspìšném splnìní
        }
    }
}