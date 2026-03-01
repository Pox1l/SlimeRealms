using FMODUnity;
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

    [HideInInspector] public float lastTimeUsed = -999f;

    [Header("Audio")]
    public EventReference pickupSound;
    public EventReference useSound; // 🔥 Přidáno pro zvuk použití

    private void OnEnable()
    {
        lastTimeUsed = -999f;
    }

    // PŘIDÁNO: Parametr pro předání chybové hlášky
    public virtual bool UseItem(out string failMessage)
    {
        failMessage = "";
        return false;
    }

    public float GetCooldownPercentage()
    {
        if (lastTimeUsed > Time.time) lastTimeUsed = -999f;

        if (Time.time >= lastTimeUsed + cooldown) return 0f;

        float remainingTime = (lastTimeUsed + cooldown) - Time.time;
        return remainingTime / cooldown;
    }
}