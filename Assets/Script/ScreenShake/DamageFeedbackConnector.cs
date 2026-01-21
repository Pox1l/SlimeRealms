using UnityEngine;
using MoreMountains.Feedbacks;

public class DamageFeedbackConnector : MonoBehaviour
{
    [Header("Feedbacks")]
    // Teď už to nemusíš přetahovat, ale nechám to public pro kontrolu v Inspektoru
    public MMF_Player smallShake;
    public MMF_Player bigShake;

    // 🔥 Tuhle funkci přidej - ta to najde automaticky
    void Awake()
    {
        // Najde všechny MMF Playery v potomcích (i kdyby byly vypnuté)
        MMF_Player[] allFeedbacks = GetComponentsInChildren<MMF_Player>(true);

        foreach (var feedback in allFeedbacks)
        {
            // Hledá klíčová slova v názvu GameObjectu
            if (feedback.gameObject.name.Contains("Small"))
            {
                smallShake = feedback;
            }
            else if (feedback.gameObject.name.Contains("Big"))
            {
                bigShake = feedback;
            }
        }
    }

    void OnEnable() { PlayerStats.OnPlayerHit += DecideShake; }
    void OnDisable() { PlayerStats.OnPlayerHit -= DecideShake; }

    void DecideShake(int damageAmount)
    {
        if (damageAmount >= 10)
        {
            if (bigShake != null) bigShake.PlayFeedbacks();
        }
        else
        {
            if (smallShake != null) smallShake.PlayFeedbacks();
        }
    }
}