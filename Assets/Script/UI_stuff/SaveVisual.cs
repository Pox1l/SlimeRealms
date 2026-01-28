using UnityEngine;
using System.Collections;

public class SaveVisual : MonoBehaviour
{
    [Header("Nastavení")]
    // Zrušil jsem [SerializeField], protože se to hledá samo
    private CanvasGroup saveIconGroup;

    [SerializeField] private float animationDuration = 2.0f;

    public static event System.Action OnAnySave;

    public static void ReportSave()
    {
        OnAnySave?.Invoke();
    }

    private Coroutine pulseRoutine;

    private void Awake()
    {
        // 1. Automatické nalezení CanvasGroupu v potomcích (na SaveIcon)
        saveIconGroup = GetComponentInChildren<CanvasGroup>();

        if (saveIconGroup == null)
        {
            Debug.LogError("SaveVisual: Nenašel jsem žádný CanvasGroup v potomcích!");
            return;
        }

        // 2. Okamžité skrytí ikony pøi startu hry
        saveIconGroup.alpha = 0f;
    }

    private void OnEnable() => OnAnySave += TriggerPulse;
    private void OnDisable() => OnAnySave -= TriggerPulse;

    private void TriggerPulse()
    {
        if (saveIconGroup == null) return;

        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        float elapsed = 0f;
        // Na zaèátku ukládání nastavíme nulu, pro jistotu
        saveIconGroup.alpha = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            saveIconGroup.alpha = Mathf.Abs(Mathf.Sin(elapsed * 4f));
            yield return null;
        }

        // Po skonèení animace zase zmizí
        saveIconGroup.alpha = 0f;
    }
}