using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldPortalButtons : MonoBehaviour
{
    [System.Serializable]
    public class WorldEntry
    {
        [Header("UI tlaèítko svìta")]
        public Button worldButton;

        [Header("Portál ve scénì, který se má zobrazit")]
        public GameObject portalObject;
    }

    [Header("Seznam svìtù (index = poøadí ve listu)")]
    public List<WorldEntry> worlds = new List<WorldEntry>();

    void Start()
    {
        // 1) schovat všechny portály na zaèátku
        for (int i = 0; i < worlds.Count; i++)
        {
            var entry = worlds[i];

            if (entry.portalObject != null)
                entry.portalObject.SetActive(false);

            // 2) nastavit listener na button
            if (entry.worldButton != null)
            {
                // smažeme všechny runtime listenery (aby se to nevolalo 3×,
                // pokud bys tøeba omylem pøidal znovu)
                entry.worldButton.onClick.RemoveAllListeners();

                int capturedIndex = i; // dùležité – jinak by všechny lambda mìly poslední index
                entry.worldButton.onClick.AddListener(() => OnWorldButtonClicked(capturedIndex));
            }
        }
    }

    // Tohle už se volá jen z AddListener v kódu – NE z OnClick v Inspectoru
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
