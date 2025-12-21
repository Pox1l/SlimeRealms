using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public int itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public int maxStack = 10;
    public bool isUsable = false;

    [Header("Cooldown Settings")]
    public float cooldown = 1f;

    // Změnil jsem na protected, ale s [HideInInspector], aby to nešlo rozbít ručně
    [HideInInspector] public float lastTimeUsed = -999f;

    // 🔥 TOTO JE TA OPRAVA 🔥
    // OnEnable se zavolá, když Unity načte tento ScriptableObject (při startu hry)
    private void OnEnable()
    {
        lastTimeUsed = -999f;
    }

    public virtual bool UseItem()
    {
        return false;
    }

    public float GetCooldownPercentage()
    {
        // Pojistka: Kdyby se OnEnable nezavolal (třeba v buildu), 
        // a lastTimeUsed byl větší než aktuální čas (což je nesmysl), tak ho resetujeme.
        if (lastTimeUsed > Time.time) lastTimeUsed = -999f;

        if (Time.time >= lastTimeUsed + cooldown) return 0f;

        float remainingTime = (lastTimeUsed + cooldown) - Time.time;
        return remainingTime / cooldown;
    }
}