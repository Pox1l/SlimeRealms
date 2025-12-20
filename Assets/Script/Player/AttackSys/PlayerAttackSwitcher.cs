using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackSwitcher : MonoBehaviour
{
    [Header("Reference")]
    public PlayerAttackSystem attackSystem;  // reference na PlayerAttackSystem (kde se provádí útok)
    public List<AttackBase> availableAttacks = new List<AttackBase>(); // všechny útoky, které má hráč

    [Header("Current Attack")]
    [SerializeField] public int currentIndex = 0;

    void Start()
    {
        if (attackSystem == null)
            attackSystem = GetComponent<PlayerAttackSystem>();

        // Nastaví výchozí útok, pokud existuje
        if (availableAttacks.Count > 0)
            SetAttack(0);
    }

    void Update()
    {
        if (availableAttacks.Count == 0 || attackSystem == null)
            return;

        // 🔢 Přepínání útoků klávesami 1–9
        for (int i = 0; i < availableAttacks.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetAttack(i);
                break;
            }
        }
    }

    void SetAttack(int index)
    {
        if (index < 0 || index >= availableAttacks.Count) return;

        currentIndex = index;
        attackSystem.currentAttack = availableAttacks[index];

        Debug.Log($"🔄 Switched attack to: {attackSystem.currentAttack.attackName} (slot {index + 1})");
    }

    // 👇 (volitelné) veřejná funkce pro přepnutí z jiného scriptu (např. ze skill tree)
    public void SetAttackByID(int index)
    {
        SetAttack(index);
    }
}
