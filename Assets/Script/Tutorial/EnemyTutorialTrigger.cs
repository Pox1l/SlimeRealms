using UnityEngine;

[RequireComponent(typeof(EnemyHealth))] // Automaticky vyžaduje skript EnemyHealth
public class EnemyTutorialTrigger : MonoBehaviour
{
    [Header("Tutorial")]
    [Tooltip("Název eventu pro tutoriál (napø. 'ZabitGreenSlime').")]
    public string tutorialEventName;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        // Pøihlášení k odbìru eventu OnDeath
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += TriggerTutorialEvent;
        }
    }

    private void OnDisable()
    {
        // Odhlášení z odbìru (dùležité pro prevenci memory leaks)
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= TriggerTutorialEvent;
        }
    }

    private void TriggerTutorialEvent()
    {
        if (!string.IsNullOrEmpty(tutorialEventName) && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.TriggerEvent(tutorialEventName);
        }
    }
}