using UnityEngine;
using UnityEngine.UI;
// TMPro už nepotřebujeme, pokud tam nebudou jiné texty

public class PlayerCombatUI : MonoBehaviour
{
    [Header("References - Player")]
    public PlayerAttackSystem attackSystem;
    public PlayerAttackSwitcher attackSwitcher;

    [Header("Key Sprites Configuration")]
    // 🖼️ Sem přetáhni sprity kláves v pořadí 1, 2, 3, 4...
    public Sprite[] keySprites;

    [Header("UI - Active Slot (Velký)")]
    public Image activeIcon;
    public Image activeCooldown;
    public Image activeKeyImage; // 🖼️ Změna: Místo Textu je tu Image

    [Header("UI - Next Slot (Malý)")]
    public Image nextIcon;
    public Image nextKeyImage;   // 🖼️ Změna: Místo Textu je tu Image

    private void Start()
    {
        if (attackSystem == null || attackSwitcher == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                attackSystem = playerObj.GetComponentInChildren<PlayerAttackSystem>();
                attackSwitcher = playerObj.GetComponentInChildren<PlayerAttackSwitcher>();
            }
        }
    }

    void Update()
    {
        if (attackSystem == null || attackSwitcher == null) return;
        if (attackSwitcher.availableAttacks.Count == 0) return;

        // --- LOGIKA INDEXŮ ---
        int currentIdx = attackSwitcher.currentIndex;
        int nextIdx = (currentIdx + 1) % attackSwitcher.availableAttacks.Count;

        AttackBase currentAttack = attackSwitcher.availableAttacks[currentIdx];
        AttackBase nextAttack = attackSwitcher.availableAttacks[nextIdx];

        // --- 1. UPDATE AKTIVNÍHO SLOTU ---
        if (activeIcon.sprite != currentAttack.icon)
            activeIcon.sprite = currentAttack.icon;

        // 🖼️ Nastavení SPRITU klávesy (s kontrolou, zda existuje)
        if (activeKeyImage != null && keySprites != null && currentIdx < keySprites.Length)
        {
            // Optimalizace: měníme jen když je potřeba
            if (activeKeyImage.sprite != keySprites[currentIdx])
                activeKeyImage.sprite = keySprites[currentIdx];
        }

        // Cooldown
        float timeLeft = attackSystem.nextAttackTime - Time.time;
        if (timeLeft > 0)
            activeCooldown.fillAmount = timeLeft * currentAttack.attackRate;
        else
            activeCooldown.fillAmount = 0;

        // --- 2. UPDATE DALŠÍHO SLOTU ---
        if (nextIcon.sprite != nextAttack.icon)
            nextIcon.sprite = nextAttack.icon;

        // 🖼️ Nastavení SPRITU klávesy pro další zbraň
        if (nextKeyImage != null && keySprites != null && nextIdx < keySprites.Length)
        {
            if (nextKeyImage.sprite != keySprites[nextIdx])
                nextKeyImage.sprite = keySprites[nextIdx];
        }
    }
}