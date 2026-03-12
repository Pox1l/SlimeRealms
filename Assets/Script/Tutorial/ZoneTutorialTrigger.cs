using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ZoneTutorialTrigger : MonoBehaviour
{
    [Header("Tutorial")]
    public string tutorialEventName;

    [Header("Interakce")]
    public bool requireKeyPress = false;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Nápovìda")]
    public UnityEvent onZoneEnter;
    public UnityEvent onZoneExit;

    private bool playerInZone = false;

    private void Update()
    {
        if (playerInZone && requireKeyPress)
        {
            if (Input.GetKeyDown(interactKey))
            {
                TriggerTutorialEvent();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            onZoneEnter.Invoke();

            if (!requireKeyPress)
            {
                TriggerTutorialEvent();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            onZoneExit.Invoke();
        }
    }

    private void TriggerTutorialEvent()
    {
        if (!string.IsNullOrEmpty(tutorialEventName) && TutorialManager.Instance != null)
        {
            // Manager sám ví, jestli je tento event teï aktuální
            TutorialManager.Instance.TriggerEvent(tutorialEventName);

            // Pokud chcete, aby po splnìní nápovìda zmizela:
            // onZoneExit.Invoke(); 
        }
    }
}