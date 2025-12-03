using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    // Žádné odkazy na Main Menu! Settings øeší jen sám sebe.

    [System.Serializable]
    public class MenuTab
    {
        public string name;
        public GameObject panel;
        public Button button;
    }

    [Header("Tabs Configuration")]
    public List<MenuTab> tabs = new List<MenuTab>();

    [Header("Visuals")]
    public Color normalColor = Color.white;
    public Color activeColor = Color.green;

    void Start()
    {
        foreach (var tab in tabs)
        {
            tab.button.onClick.AddListener(() => OnTabClicked(tab));
        }

        if (tabs.Count > 0)
            OnTabClicked(tabs[0]);
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