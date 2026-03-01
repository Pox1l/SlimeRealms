using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SimpleTutorialManager : MonoBehaviour
{
    [Header("Data")]
    public TutorialSaveSystem saveSystem;

    [System.Serializable]
    public class TutorialSlide
    {
        [TextArea(5, 10)] public string description;
        public Sprite image;
    }

    [Header("Obsah")]
    public List<TutorialSlide> slides;

    [Header("UI Reference (Auto-Find)")]
    public GameObject tutorialPanel;
    public Image displayImage;
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI pageText;

    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    private int currentIndex = 0;
    private TutorialData currentData;

    void Awake()
    {
        FindReferences();
    }

    void Start()
    {
        // 1. Načtení Save systému
        if (saveSystem == null) saveSystem = GetComponent<TutorialSaveSystem>();

        // 2. Načtení dat (proběhl už tutoriál?)
        if (saveSystem != null) currentData = saveSystem.Load();
        else currentData = new TutorialData();

        // 3. KONTROLA: Pokud už je hotovo
        if (currentData.isCompleted)
        {
            // Vypneme panel
            if (tutorialPanel != null) tutorialPanel.SetActive(false);

            // Pojistka: Pokud UIManager omylem tutoriál zapnul, řekneme mu, ať ho vypne (vrátí HUD a čas)
            if (UIManager.Instance != null && UIManager.Instance.isTutorialOpen)
            {
                UIManager.Instance.CloseTutorial();
            }
            return; // Konec, nic dál se neděje
        }

        // 4. Pokud NENÍ hotovo -> Spustíme
        StartTutorial();
    }

    [ContextMenu("Najít UI Prvky")]
    public void FindReferences()
    {
        if (tutorialPanel == null)
            tutorialPanel = transform.Find("TutorialSimplePanel")?.gameObject;

        // Pokud panel stále nemáme (třeba je tento skript přímo na panelu), zkusíme gameObject
        if (tutorialPanel == null && gameObject.name == "TutorialSimplePanel")
            tutorialPanel = this.gameObject;

        if (tutorialPanel == null) return;

        Transform t = tutorialPanel.transform;

        if (nextButton == null) nextButton = t.Find("NextBTN")?.GetComponent<Button>();
        if (prevButton == null) prevButton = t.Find("PreviousBTN")?.GetComponent<Button>();
        if (closeButton == null) closeButton = t.Find("XBTN")?.GetComponent<Button>();

        if (displayImage == null) displayImage = t.Find("IMG")?.GetComponent<Image>();
        if (displayText == null) displayText = t.Find("DisplayText (TMP)")?.GetComponent<TextMeshProUGUI>();
        if (pageText == null) pageText = t.Find("PageText (TMP)")?.GetComponent<TextMeshProUGUI>();

        Debug.Log("✅ UI Reference automaticky nalezeny.");
    }

    public void StartTutorial()
    {
        // Otevřeme vizuálně panel
        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        currentIndex = 0;

        // Propojíme se s UIManagerem (pokud existuje), aby skryl HUD a zastavil čas
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenTutorial();
        }
        else
        {
            // Fallback kdyby nebyl UIManager
            Time.timeScale = 0f;
        }

        // Listenery
        if (nextButton) { nextButton.onClick.RemoveAllListeners(); nextButton.onClick.AddListener(NextSlide); }
        if (prevButton) { prevButton.onClick.RemoveAllListeners(); prevButton.onClick.AddListener(PrevSlide); }
        if (closeButton) { closeButton.onClick.RemoveAllListeners(); closeButton.onClick.AddListener(CloseTutorial); }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (slides.Count == 0) return;

        TutorialSlide currentSlide = slides[currentIndex];

        if (displayText) displayText.text = currentSlide.description;
        if (displayImage)
        {
            displayImage.gameObject.SetActive(currentSlide.image != null);
            displayImage.sprite = currentSlide.image;
        }
        if (pageText) pageText.text = $"{currentIndex + 1} / {slides.Count}";

        if (prevButton)
        {
            prevButton.gameObject.SetActive(true);
            prevButton.interactable = (currentIndex > 0);
        }

        if (nextButton)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = (currentIndex < slides.Count - 1);
        }

        // Přidáno: Tlačítko na zavření se ukáže až na posledním slajdu
        if (closeButton)
        {
            closeButton.gameObject.SetActive(currentIndex == slides.Count - 1);
        }
    }

    public void NextSlide()
    {
        if (currentIndex < slides.Count - 1)
        {
            currentIndex++;
            UpdateUI();
        }
    }

    public void PrevSlide()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateUI();
        }
    }

    public void CloseTutorial()
    {
        // 1. Uložíme, že je hotovo
        currentData.isCompleted = true;
        if (saveSystem != null) saveSystem.Save(currentData);

        // 2. Vypneme panel
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        // 3. Řekneme UIManageru, ať obnoví hru (zapne HUD, pustí čas)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseTutorial();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    [ContextMenu("Reset Save")]
    public void ResetTutorialSave()
    {
        if (saveSystem != null) saveSystem.DeleteSave();
        currentData = new TutorialData();
        Debug.Log("Save resetován - Tutorial se příště znovu zobrazí.");
    }
}