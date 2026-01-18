using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Systémové Reference")]
    public TutorialSaveSystem saveSystem;

    [Header("UI Reference")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI instructionTextUI;
    public GameObject uiBlocker;            // Černý panel pro spotlight efekt
    // centralMenuRoot už nepotřebujeme, zeptáme se UIManageru

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea] public string instructionText;
        public GameObject objectToEnable;         // 3D šipka ve světě

        [Header("UI Interakce")]
        public RectTransform uiElementToHighlight; // Tlačítko, které má svítit
        public bool requireMenuOpen;               // Musí být otevřené menu?
    }

    [Header("Nastavení Kroků")]
    public List<TutorialStep> steps;

    private TutorialData currentData;
    private bool wDone, aDone, sDone, dDone;

    // Proměnné pro Highlight
    private Canvas tempCanvas;
    private GraphicRaycaster tempRaycaster;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 1. Načtení dat
        if (saveSystem != null) currentData = saveSystem.Load();
        else currentData = new TutorialData();

        if (uiBlocker != null) uiBlocker.SetActive(false);

        // 2. Start nebo Skrytí
        if (!currentData.isCompleted)
        {
            InitializeStep(currentData.currentStepIndex);
        }
        else
        {
            tutorialPanel.SetActive(false);
            foreach (var step in steps)
                if (step.objectToEnable != null) step.objectToEnable.SetActive(false);
        }
    }

    private void Update()
    {
        if (currentData.isCompleted) return;
        if (UIManager.Instance == null) return; // Pojistka

        int index = currentData.currentStepIndex;
        if (index >= steps.Count) return;

        TutorialStep step = steps[index];

        // --- A) KROK 0: WASD POHYB ---
        if (index == 0)
        {
            CheckWASDInput();
        }

        // --- B) KROK VYŽADUJÍCÍ MENU ---

        // 1. Pokud krok chce menu, ale menu je ZAVŘENÉ -> Čekáme
        if (step.requireMenuOpen && !UIManager.Instance.isGameMenuOpen)
        {
            // Tady hráč musí zmáčknout TAB.
            // UIManager to zachytí a nastaví isGameMenuOpen = true.
        }

        // 2. Pokud krok chce menu a menu JE OTEVŘENÉ
        else if (step.requireMenuOpen && UIManager.Instance.isGameMenuOpen)
        {
            // Zkontrolujeme, jestli krok nemá tlačítko na kliknutí (Highlight)
            // Pokud nemá Highlight (jen úkol "Otevři menu"), tak ho rovnou splníme.
            if (step.uiElementToHighlight == null)
            {
                AdvanceStep();
            }
            // Pokud MÁ Highlight (např. "Klikni na Inventář"), čekáme na kliknutí (řeší funkce HighlightUIElement)
        }
    }

    // --- Logika pro WASD ---
    void CheckWASDInput()
    {
        bool changed = false;
        if (Input.GetKey(KeyCode.W) && !wDone) { wDone = true; changed = true; }
        if (Input.GetKey(KeyCode.A) && !aDone) { aDone = true; changed = true; }
        if (Input.GetKey(KeyCode.S) && !sDone) { sDone = true; changed = true; }
        if (Input.GetKey(KeyCode.D) && !dDone) { dDone = true; changed = true; }

        if (changed) UpdateWASDText();

        if (wDone && aDone && sDone && dDone) AdvanceStep();
    }

    void UpdateWASDText()
    {
        string w = wDone ? "<color=green>W</color>" : "W";
        string a = aDone ? "<color=green>A</color>" : "A";
        string s = sDone ? "<color=green>S</color>" : "S";
        string d = dDone ? "<color=green>D</color>" : "D";
        instructionTextUI.text = $"Pohyb: {w} {a} {s} {d}";
    }

    // --- Hlavní posuvník kroků ---
    public void AdvanceStep()
    {
        CleanupHighlight();

        // Vypnout 3D objekt z minulého kroku
        if (currentData.currentStepIndex < steps.Count)
        {
            var prevObj = steps[currentData.currentStepIndex].objectToEnable;
            if (prevObj != null) prevObj.SetActive(false);
        }

        currentData.currentStepIndex++;
        if (saveSystem != null) saveSystem.Save(currentData);

        InitializeStep(currentData.currentStepIndex);
    }

    private void InitializeStep(int index)
    {
        if (index >= steps.Count)
        {
            CompleteTutorial();
            return;
        }

        tutorialPanel.SetActive(true);
        TutorialStep currentStep = steps[index];

        // Nastavení textu
        if (index == 0)
        {
            wDone = false; aDone = false; sDone = false; dDone = false;
            UpdateWASDText();
        }
        else
        {
            instructionTextUI.text = currentStep.instructionText;
        }

        // Zapnutí 3D objektu
        if (currentStep.objectToEnable != null) currentStep.objectToEnable.SetActive(true);

        // Zapnutí UI Highlightu (pokud je definován) 
        if (currentStep.uiElementToHighlight != null)
        {
            HighlightUIElement(currentStep.uiElementToHighlight);
        }
    }

    // --- SPOTLIGHT EFEKT (Vysvícení tlačítka) ---
    private void HighlightUIElement(RectTransform element)
    {
        if (uiBlocker != null) uiBlocker.SetActive(true); // Zapneme tmu

        // Přidáme Canvas, aby se tlačítko vykreslilo NAD tmou
        tempCanvas = element.GetComponent<Canvas>();
        if (tempCanvas == null) tempCanvas = element.gameObject.AddComponent<Canvas>();

        tempCanvas.overrideSorting = true;
        tempCanvas.sortingOrder = 30000; // Musí být vyšší než sortingOrder UIManageru

        // Přidáme Raycaster pro klikání
        tempRaycaster = element.GetComponent<GraphicRaycaster>();
        if (tempRaycaster == null) tempRaycaster = element.gameObject.AddComponent<GraphicRaycaster>();

        // Přidáme posluchače na tlačítko
        Button btn = element.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnHighlightedButtonClicked);
        }
    }

    private void OnHighlightedButtonClicked()
    {
        // Kliknul na správné tlačítko -> další krok
        AdvanceStep();
    }

    private void CleanupHighlight()
    {
        if (uiBlocker != null) uiBlocker.SetActive(false);

        if (currentData.currentStepIndex < steps.Count)
        {
            var step = steps[currentData.currentStepIndex];
            if (step.uiElementToHighlight != null)
            {
                Button btn = step.uiElementToHighlight.GetComponent<Button>();
                if (btn != null) btn.onClick.RemoveListener(OnHighlightedButtonClicked);

                if (tempRaycaster != null) Destroy(tempRaycaster);
                if (tempCanvas != null) Destroy(tempCanvas);
            }
        }
    }

    private void CompleteTutorial()
    {
        currentData.isCompleted = true;
        if (saveSystem != null) saveSystem.Save(currentData);

        CleanupHighlight();
        tutorialPanel.SetActive(false);
        Debug.Log("🎓 Tutoriál kompletně dokončen!");
    }
}