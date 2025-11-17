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
        if (index < 0 || index >= worlds.Count)
        {
            Debug.LogWarning($"[WorldPortalButtons] Index {index} mimo rozsah.");
            return;
        }

        var entry = worlds[index];

        if (entry.portalObject != null)
        {
            entry.portalObject.SetActive(true);
            Debug.Log($"[WorldPortalButtons] Zobrazen portál pro svìt index {index}.");
        }
        else
        {
            Debug.LogWarning($"[WorldPortalButtons] Svìt {index} nemá nastavený portalObject.");
        }
    }
}
