using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPortalManager : MonoBehaviour
{
    [Header("Portály dle indexu")]
    [Tooltip("0 = Cave, 1 = Forest, 2 = Graveyard atd.")]
    public GameObject[] portals;

    void Start()
    {
        // skryjeme všechny portály pøi startu
        foreach (var p in portals)
        {
            if (p != null)
                p.SetActive(false);
        }
    }

    public void OnWorldButtonClicked(int index)
    {
        if (index < 0 || index >= portals.Length)
        {
            Debug.LogWarning("Portal index mimo rozsah!");
            return;
        }

        Debug.Log("Zobrazit portál index: " + index);

        portals[index].SetActive(true);
    }
}
