using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZoneTutorialTrigger : MonoBehaviour
{
    [Header("Tutorial")]
    [Tooltip("Název eventu pro tutoriál (napø. 'DoselDoTabora').")]
    public string tutorialEventName;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Zkontroluje, zda do zóny vešel hráè a jestli už se event nespustil
        if (!hasTriggered && other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(tutorialEventName) && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.TriggerEvent(tutorialEventName);
                hasTriggered = true;
            }
        }
    }
}