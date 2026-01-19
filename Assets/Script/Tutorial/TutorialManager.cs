using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Systémové Reference")]
    public TutorialSaveSystem saveSystem;

    [Header("World UI (HUD - Roh obrazovky)")]
    public GameObject tutorialPanel;          // Panel s úkolem v rohu
    public TextMeshProUGUI instructionTextUI; // Text v tom panelu

    [Header("Menu UI (Bublina & Blocker)")]
    public GameObject uiBlocker;              // Černé poloprůhledné pozadí
    public GameObject bubblePanel;            // Bublina s textem
    public TextMeshProUGUI bubbleTextUI;      // Text v bublině
    public Button bubbleNextButton;           // Tlačítko "Pokračovat" v bublině
    public Vector3 bubbleOffset = new Vector3(0, -100, 0); // Odsazení bubliny od cíle

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea] public string instructionText; // Text úkolu
        public GameObject objectToEnable;         // Volitelné: 3D šipka ve světě

        [Header("UI Interakce")]
        public List<RectTransform> uiTargets;     // Seznam UI prvků k vysvícení (Highlight)

        public bool requireMenuOpen;              // Máme čekat, dokud nebude UIManager.isGameMenuOpen?
        public bool waitForClickOnItem;           // TRUE = Hráč musí kliknout na vysvícený item. FALSE = Hráč kliká na bublinu "Pokračovat".
    }

    [Header("Nastavení Kroků")]
    public List<TutorialStep> steps;

    // Lokální data
    private TutorialData currentData;

    // Pomocné pro WASD
    private bool wDone, aDone, sDone, dDone;

    // Seznamy pro čištění Highlightu
    private List<Canvas> tempCanvases = new List<Canvas>();
    private List<GraphicRaycaster> tempRaycasters = new List<GraphicRaycaster>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 1. Načtení dat
        if (saveSystem != null) currentData = saveSystem.Load();
        else currentData = new TutorialData();

        // 2. Reset UI na startu
        if (uiBlocker != null) uiBlocker.SetActive(false);
        if (bubblePanel != null) bubblePanel.SetActive(false);

        // Listener pro tlačítko v bublině ("Pokračovat")
        if (bubbleNextButton != null)
        {
            bubbleNextButton.onClick.RemoveAllListeners();
            bubbleNextButton.onClick.AddListener(OnBubbleNextClicked);
        }

        // 3. Spuštění kroku nebo skrytí tutoriálu
        if (!currentData.isCompleted)
        {
            InitializeStep(currentData.currentStepIndex);
        }
        else
        {
            tutorialPanel.SetActive(false);
            if (bubblePanel != null) bubblePanel.SetActive(false);
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

        // --- B) ČEKÁNÍ NA MENU ---

        // 1. Pokud krok vyžaduje menu, ale to je ZAVŘENÉ -> Čekáme na TAB/I...
        if (step.requireMenuOpen && !UIManager.Instance.isGameMenuOpen)
        {
            // Nic neděláme, HUD panel ukazuje "Otevři menu".
        }

        // 2. Pokud krok vyžaduje menu a to JE OTEVŘENÉ
        else if (step.requireMenuOpen && UIManager.Instance.isGameMenuOpen)
        {
            // Pokud krok nemá žádné UI cíle k vysvícení (jen úkol "Otevři menu"), rovnou ho splníme.
            if (step.uiTargets.Count == 0)
            {
                AdvanceStep();
            }
            // Pokud má UI cíle, už jsou vysvícené (InitializeStep to udělal) a čekáme na kliknutí.
        }
    }

    // --- LOGIKA POSUNU KROKŮ ---

    // Volá se tlačítkem v bublině "Pokračovat"
    private void OnBubbleNextClicked()
    {
        AdvanceStep();
    }

    // Volá se, když hráč klikne na vysvícený předmět (pokud je waitForClickOnItem = true)
    private void OnHighlightedItemClicked()
    {
        AdvanceStep();
    }

    public void AdvanceStep()
    {
        CleanupHighlight(); // Úklid po starém kroku

        // Vypnout 3D objekt z minulého kroku
        if (currentData.currentStepIndex < steps.Count)
        {
            var prevObj = steps[currentData.currentStepIndex].objectToEnable;
            if (prevObj != null) prevObj.SetActive(false);
        }

        // Zvýšit index a uložit
        currentData.currentStepIndex++;
        if (saveSystem != null) saveSystem.Save(currentData);

        // Spustit nový krok
        InitializeStep(currentData.currentStepIndex);
    }

    private void InitializeStep(int index)
    {
        if (index >= steps.Count)
        {
            CompleteTutorial();
            return;
        }

        TutorialStep currentStep = steps[index];

        // Zapnout 3D objekt (pokud existuje)
        if (currentStep.objectToEnable != null) currentStep.objectToEnable.SetActive(true);

        // --- ROZHODOVÁNÍ: HUD vs. BUBLINA ---

        // Pokud máme UI cíle k vysvícení -> Režim Bublina + Highlight
        if (currentStep.uiTargets != null && currentStep.uiTargets.Count > 0)
        {
            // HUD skryjeme
            if (tutorialPanel != null) tutorialPanel.SetActive(false);

            // Aktivujeme Highlight
            HighlightMultipleElements(currentStep);
        }
        else
        {
            // Nemáme cíle -> Režim HUD (Svět / WASD / Obyčejný úkol)
            if (bubblePanel != null) bubblePanel.SetActive(false);
            if (uiBlocker != null) uiBlocker.SetActive(false);
            if (tutorialPanel != null) tutorialPanel.SetActive(true);

            // WASD má speciální text, ostatní berou text z Inspectoru
            if (index == 0)
            {
                wDone = false; aDone = false; sDone = false; dDone = false;
                UpdateWASDText();
            }
            else
            {
                if (instructionTextUI != null) instructionTextUI.text = currentStep.instructionText;
            }
        }
    }

    // --- HIGHLIGHT & BUBLINA ---
    private void HighlightMultipleElements(TutorialStep step)
    {
        if (uiBlocker != null) uiBlocker.SetActive(true); // Tma

        // 1. Vysvícení všech cílů
        foreach (RectTransform target in step.uiTargets)
        {
            if (target == null) continue;

            // Přidáme Canvas (aby to bylo nad tmou)
            Canvas cv = target.GetComponent<Canvas>();
            if (cv == null) cv = target.gameObject.AddComponent<Canvas>();
            cv.overrideSorting = true;
            cv.sortingOrder = 30000;
            tempCanvases.Add(cv);

            // Přidáme Raycaster (aby šlo klikat)
            GraphicRaycaster gr = target.GetComponent<GraphicRaycaster>();
            if (gr == null) gr = target.gameObject.AddComponent<GraphicRaycaster>();
            tempRaycasters.Add(gr);

            // Pokud čekáme na kliknutí přímo na item, přidáme Listener
            if (step.waitForClickOnItem)
            {
                Button btn = target.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(OnHighlightedItemClicked);
            }
        }

        // 2. Nastavení Bubliny
        if (bubblePanel != null)
        {
            bubblePanel.SetActive(true);
            if (bubbleTextUI != null) bubbleTextUI.text = step.instructionText;

            // Pozice: Pod prvním elementem v seznamu
            if (step.uiTargets.Count > 0 && step.uiTargets[0] != null)
            {
                bubblePanel.transform.position = step.uiTargets[0].position + bubbleOffset;
            }

            // Viditelnost tlačítka "Pokračovat":
            // Pokud čekáme na item (např. Profil), tlačítko skryjeme.
            // Pokud jen vysvětlujeme (např. HUD), tlačítko ukážeme.
            if (bubbleNextButton != null)
            {
                bubbleNextButton.gameObject.SetActive(!step.waitForClickOnItem);
            }
        }
    }

    private void CleanupHighlight()
    {
        if (uiBlocker != null) uiBlocker.SetActive(false);
        if (bubblePanel != null) bubblePanel.SetActive(false);

        // Odstranit listenery z tlačítek v aktuálním kroku
        if (currentData.currentStepIndex < steps.Count)
        {
            var step = steps[currentData.currentStepIndex];
            if (step.uiTargets != null)
            {
                foreach (var target in step.uiTargets)
                {
                    if (target == null) continue;
                    Button btn = target.GetComponent<Button>();
                    if (btn != null) btn.onClick.RemoveListener(OnHighlightedItemClicked);
                }
            }
        }

        // Zničit dočasné komponenty
        foreach (var gr in tempRaycasters) if (gr != null) Destroy(gr);
        foreach (var cv in tempCanvases) if (cv != null) Destroy(cv);

        tempRaycasters.Clear();
        tempCanvases.Clear();
    }

    // --- WASD LOGIKA ---
    void CheckWASDInput()
    {
        bool changed = false;
        if (Input.GetKey(KeyCode.W) && !wDone) { wDone = true; changed = true; }
        if (Input.GetKey(KeyCode.A) && !aDone) { aDone = true; changed = true; }
        if (Input.GetKey(KeyCode.S) && !sDone) { sDone = true; changed = true; }
        if (Input.GetKey(KeyCode.D) && !dDone) { dDone = true; changed = true; }

        if (changed) UpdateWASDText();

        if (wDone && aDone && sDone && dDone)
        {
            AdvanceStep();
        }
    }

    void UpdateWASDText()
    {
        string w = wDone ? "<color=green>W</color>" : "W";
        string a = aDone ? "<color=green>A</color>" : "A";
        string s = sDone ? "<color=green>S</color>" : "S";
        string d = dDone ? "<color=green>D</color>" : "D";

        if (instructionTextUI != null)
            instructionTextUI.text = $"Pohyb: {w} {a} {s} {d}";
    }

    private void CompleteTutorial()
    {
        currentData.isCompleted = true;
        if (saveSystem != null) saveSystem.Save(currentData);

        CleanupHighlight();
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        Debug.Log("🎓 Tutoriál kompletně dokončen!");
    }
}