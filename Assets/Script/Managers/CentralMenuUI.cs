using UnityEngine;

public class CentralMenuUI : MonoBehaviour
{
    [Header("Root menu")]
    public GameObject centralMenuRoot;

    [Header("Sub-panels")]
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject skillTreePanel;

    private bool isMenuOpen = false;

    void Start()
    {
        centralMenuRoot.SetActive(false);
        CloseAllPanels();
    }

    void Update()
    {
        // TAB toggluje centrální menu
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleMenu();

        // I = rovnou inventáø
        if (Input.GetKeyDown(KeyCode.P))
            OpenProfileDirect();

        if (Input.GetKeyDown(KeyCode.I))
            OpenInventoryDirect();

        // L = rovnou skill tree
        if (Input.GetKeyDown(KeyCode.L))
            OpenSkillTreeDirect();
    }

    // ---- Pøepínání ----
    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        centralMenuRoot.SetActive(isMenuOpen);
        Time.timeScale = isMenuOpen ? 0 : 1;

        if (isMenuOpen)
        {
            // Default: otevøe se profil
            ShowPanel(inventoryPanel);
        }
        else
        {
            CloseAllPanels();
        }
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        centralMenuRoot.SetActive(false);
        Time.timeScale = 1;
        CloseAllPanels();
    }

    // ---- Tlaèítka ----
    public void OpenProfile()
    {
        EnsureMenuOpen();
        ShowPanel(profilePanel);
    }
    public void OpenProfileDirect()
    {
        EnsureMenuOpen();
        ShowPanel(profilePanel);
    }

    public void OpenInventory()
    {
        EnsureMenuOpen();
        ShowPanel(inventoryPanel);
    }
    private void OpenInventoryDirect()
    {
        EnsureMenuOpen();
        ShowPanel(inventoryPanel);
    }

    public void OpenSkillTree()
    {
        EnsureMenuOpen();
        ShowPanel(skillTreePanel);
    }

    

    private void OpenSkillTreeDirect()
    {
        EnsureMenuOpen();
        ShowPanel(skillTreePanel);
    }

    
    void EnsureMenuOpen()
    {
        if (!isMenuOpen)
        {
            isMenuOpen = true;
            centralMenuRoot.SetActive(true);
            Time.timeScale = 0;
        }
    }

    void ShowPanel(GameObject panel)
    {
        CloseAllPanels();
        panel.SetActive(true);
    }

    void CloseAllPanels()
    {
        if (profilePanel) profilePanel.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (skillTreePanel) skillTreePanel.SetActive(false);
    }
}
