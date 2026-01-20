using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WorldButtonInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Data")]
    public List<ItemSO> worldResources;

    [Header("UI Setup")]
    public GameObject resourcePrefab;   // Prefab, na kterém je komponenta Image
    public Transform container;         // Objekt, který má Layout Group (rodiè)
    public GameObject displayPanel;     // Celé okno (worldsResourcesDisplay)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (displayPanel != null) displayPanel.SetActive(true);

        // 1. Smazat staré ikonky z pøedchozího najetí myší
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // 2. Vygenerovat nové ikonky
        foreach (ItemSO item in worldResources)
        {
            if (item != null)
            {
                // Vytvoøí prefab uvnitø containeru
                GameObject newObj = Instantiate(resourcePrefab, container);

                // Najde Image a nastaví sprite z SO
                // (Pøedpokládá, že Image je pøímo na tom prefabu)
                newObj.GetComponentInChildren<Image>().sprite = item.icon;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Volitelné: po odjetí myší panel schovat nebo vyèistit
        // if (displayPanel != null) displayPanel.SetActive(false);
    }
}