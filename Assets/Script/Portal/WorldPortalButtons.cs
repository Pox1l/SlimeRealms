using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldPortalButtons : MonoBehaviour
{
    [System.Serializable]
    public class WorldEntry
    {
        [Header("UI tlaèítko svìta v krystalu")]
        public Button worldButton;

        [Header("Portál ve scénì, který se má zobrazit")]
        public GameObject portalObject;
    }

    [Header("Seznam svìtù (index = poøadí ve listu)")]
    public List<WorldEntry> worlds = new List<WorldEntry>();

    void Start()
    {
        
        for (int i = 0; i < worlds.Count; i++)
        {
            var entry = worlds[i];

            if (entry.portalObject != null)
                entry.portalObject.SetActive(false);

           
            if (entry.worldButton != null)
            {
                
                entry.worldButton.onClick.RemoveAllListeners();

                int capturedIndex = i; 
                entry.worldButton.onClick.AddListener(() => OnWorldButtonClicked(capturedIndex));
            }
        }
    }

    
    void OnWorldButtonClicked(int index)
    {
        if (index < 0 || index >= worlds.Count) return;

        // Projde všechny svìty
        for (int i = 0; i < worlds.Count; i++)
        {
            if (worlds[i].portalObject != null)
            {
                // Zapne jen ten vybraný, ostatní vypne
                bool isActive = (i == index);
                worlds[i].portalObject.SetActive(isActive);
            }
        }
        
        Debug.Log($"[WorldPortalButtons] Pøepnuto na svìt index {index}.");
    }
}
