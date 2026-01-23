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
        [TextArea(3, 5)] public string description;
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
    public Button closeButton; // To je to XBTN

    private int currentIndex = 0;
    private TutorialData currentData;

    void Awake()
    {
        FindReferences();
    }

    void Start()
    {
        if (saveSystem == null) saveSystem = GetComponent<TutorialSaveSystem>();

        if (saveSystem != null) currentData = saveSystem.Load();
        else currentData = new TutorialData();

        if (currentData.isCompleted)
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            return;
        }

        StartTutorial();
    }

    [ContextMenu("Najít UI Prvky")]
    public void FindReferences()
    {
        if (tutorialPanel == null)
            tutorialPanel = transform.Find("TutorialSimplePanel")?.gameObject;

        if (tutorialPanel == null) return;

        Transform t = tutorialPanel.transform;

        // Tlačítka podle tvého obrázku
        if (nextButton == null) nextButton = t.Find("NextBTN")?.GetComponent<Button>();
        if (prevButton == null) prevButton = t.Find("PreviousBTN")?.GetComponent<Button>();
        if (closeButton == null) closeButton = t.Find("XBTN")?.GetComponent<Button>(); // Křížek

        // Texty a Obrázek
        if (displayImage == null) displayImage = t.Find("IMG")?.GetComponent<Image>();
        if (displayText == null) displayText = t.Find("DisplayText (TMP)")?.GetComponent<TextMeshProUGUI>();
        if (pageText == null) pageText = t.Find("PageText (TMP)")?.GetComponent<TextMeshProUGUI>();

        Debug.Log("✅ UI Reference automaticky nalezeny.");
    }

    public void StartTutorial()
    {
        tutorialPanel.SetActive(true);
        currentIndex = 0;
        Time.timeScale = 0f; // Stop hry

        // Listenery
        if (nextButton) { nextButton.onClick.RemoveAllListeners(); nextButton.onClick.AddListener(NextSlide); }
        if (prevButton) { prevButton.onClick.RemoveAllListeners(); prevButton.onClick.AddListener(PrevSlide); }
        if (closeButton) { closeButton.onClick.RemoveAllListeners(); closeButton.onClick.AddListener(CloseTutorial); }

        // 🔥 Zajistíme, že Křížek je vždy aktivní a viditelný
        if (closeButton) closeButton.gameObject.SetActive(true);

        UpdateUI();
    }

    void UpdateUI()
    {
        if (slides.Count == 0) return;

        TutorialSlide currentSlide = slides[currentIndex];

        // 1. Obsah
        if (displayText) displayText.text = currentSlide.description;
        if (displayImage)
        {
            displayImage.gameObject.SetActive(currentSlide.image != null);
            displayImage.sprite = currentSlide.image;
        }
        if (pageText) pageText.text = $"{currentIndex + 1} / {slides.Count}";

        // 2. Logika tlačítek (Změněno!)

        // Previous: Aktivní jen pokud nejsme na začátku (ale nezmizí, jen zešedne)
        if (prevButton)
        {
            prevButton.gameObject.SetActive(true); // Vždy vidět
            prevButton.interactable = (currentIndex > 0);
        }

        // Next: Aktivní jen pokud nejsme na konci (ale nezmizí, jen zešedne)
        if (nextButton)
        {
            nextButton.gameObject.SetActive(true); // Vždy vidět
            nextButton.interactable = (currentIndex < slides.Count - 1);
        }

        // XBTN (Close): Neřešíme zde, protože je zapnuté trvale v StartTutorial()
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
        currentData.isCompleted = true;
        if (saveSystem != null) saveSystem.Save(currentData);

        tutorialPanel.SetActive(false);
        Time.timeScale = 1f; // Start hry
    }

    [ContextMenu("Reset Save")]
    public void ResetTutorialSave()
    {
        if (saveSystem != null) saveSystem.DeleteSave();
        currentData = new TutorialData();
        Debug.Log("Save resetován.");
    }
}