using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Systém a UI")]
    public TutorialSaveSystem saveSystem;
    public GameObject tutorialPanel;
    public TextMeshProUGUI instructionTextUI;

    [Header("Nastavení Šipky (Pointer)")]
    [Tooltip("Hráč se najde automaticky podle tagu 'Player'")]
    public Transform playerTransform;      // Hráč, kolem kterého se točí šipka
    public Transform pointerTransform;     // Objekt šipky ve světě
    public SpriteRenderer pointerRenderer; // SpriteRenderer šipky pro změnu obrázku
    public float pointerOffset = 1.5f;     // Jak daleko od hráče šipka krouží

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea] public string instructionText;

        [Header("Zóny a Ukazatel")]
        public Sprite customPointerSprite;     // Sprite, který se má ukázat (např. zelená šipka)
        public List<Transform> targetZones;    // Seznam zón (ukáže na nejbližší)
        public float hideArrowDistance = 3f;   // Vzdálenost, kdy šipka zmizí (Range)

        [Header("Podmínky pro splnění")]
        public string requiredEventName;       // Např. "ZabitGreenSlime"
        public int requiredEventCount = 1;     // Kolikrát se to musí stát (např. 4)
    }

    [Header("Kroky tutoriálu")]
    public List<TutorialStep> steps;

    private TutorialData currentData;
    private int currentEventProgress = 0;      // Počítá, kolik slimů už hráč zabil v aktuálním kroku

    private void Awake()
    {
        Instance = this;

        // PŘIDÁNO: Automatické hledání hráče podle tagu "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Objekt s tagem 'Player' nebyl v této scéně nalezen!");
        }
    }

    private void Start()
    {
        if (saveSystem != null) currentData = saveSystem.Load();
        else currentData = new TutorialData();

        if (!currentData.isCompleted && steps.Count > 0)
        {
            ShowCurrentStep();
        }
        else
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            if (pointerTransform != null) pointerTransform.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (currentData.isCompleted || currentData.currentStepIndex >= steps.Count) return;

        UpdatePointer();
    }

    // Tuto funkci zavoláš např. po zabití slima
    public void TriggerEvent(string eventName)
    {
        if (currentData.isCompleted || currentData.currentStepIndex >= steps.Count) return;

        TutorialStep currentStep = steps[currentData.currentStepIndex];

        if (currentStep.requiredEventName == eventName)
        {
            currentEventProgress++;
            UpdateInstructionText(); // Aktualizuje text (např. 1/4)

            if (currentEventProgress >= currentStep.requiredEventCount)
            {
                AdvanceStep();
            }
        }
    }

    private void AdvanceStep()
    {
        currentEventProgress = 0;
        currentData.currentStepIndex++;

        if (saveSystem != null) saveSystem.Save(currentData);

        if (currentData.currentStepIndex < steps.Count)
        {
            ShowCurrentStep();
        }
        else
        {
            CompleteTutorial();
        }
    }

    private void ShowCurrentStep()
    {
        tutorialPanel.SetActive(true);
        TutorialStep step = steps[currentData.currentStepIndex];

        UpdateInstructionText();

        // Nastavení vzhledu šipky
        if (pointerRenderer != null && step.customPointerSprite != null)
        {
            pointerRenderer.sprite = step.customPointerSprite;
        }
    }

    private void UpdateInstructionText()
    {
        TutorialStep step = steps[currentData.currentStepIndex];
        if (instructionTextUI != null)
        {
            // Pokud je potřeba víc eventů (např. zabít 4 slimy), ukáže to progres "Zabij slimy: 1/4"
            if (step.requiredEventCount > 1)
            {
                instructionTextUI.text = $"{step.instructionText} ({currentEventProgress}/{step.requiredEventCount})";
            }
            else
            {
                instructionTextUI.text = step.instructionText;
            }
        }
    }

    private void UpdatePointer()
    {
        TutorialStep step = steps[currentData.currentStepIndex];

        if (playerTransform == null || pointerTransform == null || step.targetZones.Count == 0)
        {
            if (pointerTransform != null) pointerTransform.gameObject.SetActive(false);
            return;
        }

        // Najít nejbližší zónu ze seznamu
        Transform closestZone = null;
        float minDistance = float.MaxValue;

        foreach (Transform zone in step.targetZones)
        {
            if (zone == null) continue;
            float dist = Vector2.Distance(playerTransform.position, zone.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestZone = zone;
            }
        }

        if (closestZone == null) return;

        // Skrytí šipky, pokud je hráč dost blízko k nejbližší zóně
        if (minDistance <= step.hideArrowDistance)
        {
            pointerTransform.gameObject.SetActive(false);
        }
        else
        {
            pointerTransform.gameObject.SetActive(true);

            // Výpočet úhlu a pozice pro 2D šipku
            Vector3 dir = (closestZone.position - playerTransform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            pointerTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            pointerTransform.position = playerTransform.position + dir * pointerOffset;
        }
    }

    private void CompleteTutorial()
    {
        currentData.isCompleted = true;
        if (saveSystem != null) saveSystem.Save(currentData);

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (pointerTransform != null) pointerTransform.gameObject.SetActive(false);
    }
}