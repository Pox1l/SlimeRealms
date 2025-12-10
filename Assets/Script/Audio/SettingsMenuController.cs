using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [System.Serializable]
    public class MenuTab
    {
        public string name;
        public GameObject panel;
        public Button button;
    }

    [Header("Auto Setup References")]
    public Transform buttonContainer; // Sem pøetáhni objekt, v kterém jsou tlaèítka
    public Transform panelContainer;  // Sem pøetáhni objekt, v kterém jsou panely

    [Header("Tabs Configuration")]
    public List<MenuTab> tabs = new List<MenuTab>();

    [Header("Visuals")]
    public Color normalColor = Color.white;
    public Color activeColor = Color.green;

    void Start()
    {
        // Pokud je seznam prázdný a máme kontejnery, zkusíme to naplnit sami
        if (tabs.Count == 0 && buttonContainer != null && panelContainer != null)
        {
            AutoFindTabs();
        }

        foreach (var tab in tabs)
        {
            // Musíme použít lokální promìnnou pro lambda výraz
            var currentTab = tab;
            if (currentTab.button != null)
            {
                currentTab.button.onClick.AddListener(() => OnTabClicked(currentTab));
            }
        }

        if (tabs.Count > 0)
            OnTabClicked(tabs[0]);
    }

    // Funkce pro automatické nalezení
    [ContextMenu("Load Tabs From Containers")] // Umožní spustit to i pøes pravé tlaèítko v editoru
    public void AutoFindTabs()
    {
        tabs.Clear();

        int count = Mathf.Min(buttonContainer.childCount, panelContainer.childCount);

        for (int i = 0; i < count; i++)
        {
            MenuTab newTab = new MenuTab();

            // Vezmeme tlaèítko a panel na stejném indexu (poøadí)
            Transform btnObj = buttonContainer.GetChild(i);
            Transform pnlObj = panelContainer.GetChild(i);

            newTab.name = btnObj.name;
            newTab.button = btnObj.GetComponent<Button>();
            newTab.panel = pnlObj.gameObject;

            if (newTab.button != null)
            {
                tabs.Add(newTab);
            }
        }
        Debug.Log($"Automaticky naèteno {tabs.Count} tabù.");
    }

    private void OnTabClicked(MenuTab activeTab)
    {
        foreach (var tab in tabs)
        {
            bool isActive = (tab == activeTab);
            if (tab.panel != null) tab.panel.SetActive(isActive);

            if (tab.button != null)
            {
                var image = tab.button.GetComponent<Image>();
                if (image != null) image.color = isActive ? activeColor : normalColor;
            }
        }
    }
}