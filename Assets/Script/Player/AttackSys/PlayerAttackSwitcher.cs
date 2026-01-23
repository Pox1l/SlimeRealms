using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackSwitcher : MonoBehaviour
{
    [Header("Reference")]
    public PlayerAttackSystem attackSystem;
    public List<AttackBase> availableAttacks = new List<AttackBase>();

    [Header("UI Feedback (Auto-Find)")]
    public WeaponIconPopup iconPopup; // 🔥 Teď se to zkusí najít samo, když to necháš prázdné

    [Header("Current Attack")]
    [SerializeField] public int currentIndex = 0;

    void Start()
    {
        // 1. Najdeme Attack System
        if (attackSystem == null)
            attackSystem = GetComponent<PlayerAttackSystem>();

        // 2. 🔥 AUTOMATICKÉ HLEDÁNÍ POPUPU 🔥
        if (iconPopup == null)
        {
            // Hledá komponentu WeaponIconPopup v dětech (to je ten tvůj WeaponIconCanvas)
            iconPopup = GetComponentInChildren<WeaponIconPopup>();

            if (iconPopup == null)
                Debug.LogWarning("⚠️ PlayerAttackSwitcher: Nenašel jsem WeaponIconPopup! Máš WeaponIconCanvas jako dítě hráče?");
        }

        // 3. Nastavíme první útok
        if (availableAttacks.Count > 0)
            SetAttack(0);
    }

    void Update()
    {
        if (availableAttacks.Count == 0 || attackSystem == null) return;

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
        AttackBase selectedAttack = availableAttacks[index];

        attackSystem.currentAttack = selectedAttack;

        // Zobrazení ikony
        if (iconPopup != null && selectedAttack != null)
        {
            if (selectedAttack.icon != null)
            {
                iconPopup.Show(selectedAttack.icon);
            }
        }

        Debug.Log($"🔄 Switched attack to: {attackSystem.currentAttack.attackName}");
    }

    public void SetAttackByID(int index)
    {
        SetAttack(index);
    }
}