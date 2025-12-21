using UnityEngine;
using UnityEngine.UI;

public class ContextMenuSceneUI : MonoBehaviour
{
    [Header("Přetáhni sem objekty z Hierarchy")]
    public GameObject menuPanel;
    public Button useButton;
    public Button equipButton;
    public Button deleteButton;

    private void Start()
    {
        if (InventoryContextMenu.Instance != null)
        {
            InventoryContextMenu.Instance.RegisterSceneUI(this);
            Debug.Log("✅ ContextMenuSceneUI: Nahlášeno Manageru.");
        }
        else
        {
            // Pokud manager ještě není (start hry), zkusíme ho najít
            // Toto je pojistka, kdyby pořadí načítání skriptů zlobilo
            InventoryContextMenu mgr = FindObjectOfType<InventoryContextMenu>();
            if (mgr != null)
            {
                mgr.RegisterSceneUI(this);
            }
        }
    }
}